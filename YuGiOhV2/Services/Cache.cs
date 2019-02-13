using AngleSharp;
using Dapper;
using Dapper.Contrib.Extensions;
using Discord;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Models.Banlist;
using YuGiOhV2.Models.BoosterPacks;
using YuGiOhV2.Models.Cards;
using YuGiOhV2.Models.Exceptions;

namespace YuGiOhV2.Services
{
    public class Cache
    {

        public Dictionary<string, CardParser> Parsers { get; private set; }
        public Dictionary<string, Card> Cards { get; private set; }
        public Dictionary<string, EmbedBuilder> Embeds { get; private set; }
        public Dictionary<string, BoosterPack> BoosterPacks { get; private set; }
        public string[] ValidBoosterPacks { get; set; }
        public Dictionary<string, HashSet<string>> Archetypes { get; private set; }
        public Dictionary<string, HashSet<string>> Supports { get; private set; }
        public Dictionary<string, HashSet<string>> AntiSupports { get; private set; }
        public Dictionary<string, string> Images { get; private set; }
        public Dictionary<string, string> LowerToUpper { get; private set; }
        public HashSet<string> Uppercase { get; private set; }
        public HashSet<string> Lowercase { get; private set; }
        public ConcurrentDictionary<ulong, object> GuessInProgress { get; private set; }

        /// <summary>
        /// Maps name to passcode
        /// </summary>
        public Dictionary<string, string> Passcodes { get; private set; }
        public Banlist Banlist { get; private set; }
        public int FYeahYgoCardArtPosts { get; private set; }
        public string TumblrKey { get; private set; }

        private const string DbString = "Data Source = Databases/ygofandom.db";
        private static readonly ParallelOptions _pOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        private static SqliteConnection _db = new SqliteConnection(DbString);

        private static readonly StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

        public Cache()
        {

            Log("Beginning cache initialization...");

            GuessInProgress = new ConcurrentDictionary<ulong, object>();

            //made a seperate method so other classes may update the embeds when I want them to
            Initialize();

            Log("Finished cache initialization...");

        }

        public void Initialize()
        {

            var cardParsers = AquireGoodies();

            AquireFancyMessages(cardParsers);
            BuildHouse(cardParsers);
            AquireTheUntouchables(cardParsers);
            AquireGoodiePacks();

        }

        public async Task GetAWESOMECARDART(Web web)
        {

            Log("Getting photo posts on FYeahYgoCardArt tumblr...");

            TumblrKey = await File.ReadAllTextAsync("Files/OAuth/Tumblr.txt");
            var posts = await web.GetDeserializedContent<JObject>($"https://api.tumblr.com/v2/blog/fyeahygocardart/posts/photo?api_key={TumblrKey}&limit=1");
            FYeahYgoCardArtPosts = int.Parse(posts["response"]["total_posts"].ToString());

            Log($"Got {FYeahYgoCardArtPosts} photos.");

        }

        private void AquireFancyMessages(IEnumerable<CardParser> parsers)
        {

            var counter = 0;
            var total = parsers.Count();
            var tempObjects = new ConcurrentDictionary<string, Card>();
            var tempDict = new ConcurrentDictionary<string, EmbedBuilder>();
            var tempArchetypes = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
            var tempSupports = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
            var tempAntiSupports = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
            Archetypes = new Dictionary<string, HashSet<string>>(IgnoreCase);

            Log("Generating them fancy embed messages...");

            var monitor = new object();

            Parallel.ForEach(parsers, _pOptions, parser =>
            {

                var name = parser.Name;
                var card = parser.Parse();
                EmbedBuilder embed;

                try
                {

                    embed = GenFancyMessage(card);

                }
                catch (Exception ex)
                {

                    throw new EmbedGenerationException(name, ex);

                }

                tempObjects[name] = card;
                tempDict[name] = embed;

                if (card.Archetypes != null && card.Archetypes.Any())
                {

                    var archetypes = card.Archetypes;

                    foreach (var archetype in archetypes)
                    {

                        var set = tempArchetypes.GetOrAdd(archetype, new ConcurrentDictionary<string, object>());
                        set[name] = null;

                    }

                }

                if (card.Supports != null && card.Supports.Any())
                {

                    var supports = card.Supports;

                    foreach (var support in supports)
                    {

                        var set = tempSupports.GetOrAdd(support, new ConcurrentDictionary<string, object>());
                        set[name] = null;

                    }

                }

                if (card.AntiSupports != null && card.AntiSupports.Any())
                {

                    var antiSupports = card.AntiSupports;

                    foreach (var antiSupport in antiSupports)
                    {

                        var set = tempAntiSupports.GetOrAdd(antiSupport, new ConcurrentDictionary<string, object>());
                        set[name] = null;

                    }

                }

                lock (monitor)
                {

                    var current = Interlocked.Increment(ref counter);

                    if (current != total)
                        InlineLog($"Progress: {current}/{total}");
                    else
                        Log($"Progress: {current}/{total}");

                }

            });

            Parsers = new Dictionary<string, CardParser>(parsers.ToDictionary(CardParser.GetCardName, parser => parser), IgnoreCase);
            Cards = new Dictionary<string, Card>(tempObjects, IgnoreCase);
            Embeds = new Dictionary<string, EmbedBuilder>(tempDict, IgnoreCase);
            Archetypes = new Dictionary<string, HashSet<string>>(tempArchetypes.ToDictionary(kv => kv.Key, kv => kv.Value.Keys.ToHashSet()), IgnoreCase);
            Supports = new Dictionary<string, HashSet<string>>(tempSupports.GroupBy(kv => kv.Key.Trim(), IgnoreCase).ToDictionary(group => group.Key, group => group.SelectMany(kv => kv.Value.Keys).ToHashSet()), IgnoreCase);
            AntiSupports = new Dictionary<string, HashSet<string>>(tempAntiSupports.GroupBy(kv => kv.Key.Trim(), IgnoreCase).ToDictionary(group => group.Key, group => group.SelectMany(kv => kv.Value.Keys).ToHashSet()), IgnoreCase);

            Log("Finished generating embeds.");

        }

        private void BuildHouse(IEnumerable<CardParser> parsers)
        {

            Log("Building cache...");

            Task.WaitAll(
                Task.Run(() => { Images = new Dictionary<string, string>(parsers.ToDictionary(CardParser.GetCardName, parser => parser.Img), IgnoreCase); }),
                Task.Run(() => { LowerToUpper = new Dictionary<string, string>(parsers.ToDictionary(parser => parser.Name.ToLower(), CardParser.GetCardName), IgnoreCase); }),
                Task.Run(() => { Uppercase = new HashSet<string>(parsers.Select(CardParser.GetCardName)); }),
                Task.Run(() => { Lowercase = new HashSet<string>(parsers.Select(parser => parser.Name.ToLower())); }),
                Task.Run(() => { Passcodes = new Dictionary<string, string>(parsers.Where(parser => !string.IsNullOrEmpty(parser.Passcode)).ToDictionary(CardParser.GetCardName, parser => parser.Passcode.TrimStart('0')), IgnoreCase); })
                );

            Log("Finished building cache.");

        }

        private EmbedBuilder GenFancyMessage(Card card)
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(GetIconUrl(card))
                .WithName(card.Name)
                .WithUrl(card.Url);

            var footer = new EmbedFooterBuilder()
                .WithIconUrl("http://1.bp.blogspot.com/-a3KasYvDBaY/VCQXuTjmb2I/AAAAAAAACZM/oQ6Hw71kLQQ/s1600/Cursed%2BHexagram.png")
                .WithText("Yu-Gi-Oh!");

            var body = new EmbedBuilder()
            {

                Author = author,
                Footer = footer,
                Color = GetColor(card),
                Description = GenDescription(card)

            };

            try
            {

                body.ImageUrl = card.Img;

            }
            catch { }

            if (card is Monster monster)
            {

                if (!string.IsNullOrEmpty(monster.Lore))
                {

                    //for some reason, newlines aren't properly recognized
                    monster.Lore = monster.Lore.Replace(@"\n", "\n");

                    //if (!string.IsNullOrEmpty(monster.Materials))
                    //    monster.Lore = monster.Lore.Replace($"{monster.Materials} ", $"{monster.Materials}");

                    if (monster.Lore.StartsWith("Pendulum Effect"))
                    {

                        var effects = monster.Lore.Split("Monster Effect");

                        body.AddField("Pendulum Effect", effects.First().Substring(15).Trim());
                        body.AddField($"[ {monster.Types.Join(" / ")} ]", effects[1].Trim());

                    }
                    else
                        body.AddField($"[ {monster.Types.Join(" / ")} ]", monster.Lore);

                }
                else
                    body.AddField("Not released yet", "\u200B");

                const string unknownValue = "???";

                if (monster is IHasAtk hasAtk)
                    body.AddField("ATK", string.IsNullOrEmpty(hasAtk.Atk) ? unknownValue : hasAtk.Atk, true);

                if (monster is IHasDef hasDef)
                    body.AddField("DEF", string.IsNullOrEmpty(hasDef.Def) ? unknownValue : hasDef.Def, true);

            }
            else
                body.AddField("Effect", card.Lore?.Replace(@"\n", "\n") ?? "Not yet released.");

            if (card.Archetypes != null)
                body.AddField(card.Archetypes.Length > 1 ? "Archetypes" : "Archetype", card.Archetypes.Join(", "));

            return body;

        }

        private string GenDescription(Card card)
        {

            string desc = "";

            if (!string.IsNullOrEmpty(card.RealName))
                desc += $"**Real Name:** {card.RealName}\n";

            desc += "**Format:** ";

            if (card.TcgExists)
                desc += "TCG";

            if (card.OcgExists)
                desc += card.TcgExists ? "/OCG" : "OCG";

            desc += "\n";
            desc += $"**Card Type:** {card.CardType}\n";

            if (card is Monster monster)
            {

                desc += $"**Attribute:** {monster.Attribute}\n";

                if (monster is IHasRank xyz)
                    desc += $"**Rank:** {xyz.Rank}\n";
                else if (monster is IHasLink linkMonster)
                    desc += $"**Links:** {linkMonster.Link}\n" +
                        $"**Link Markers:** {linkMonster.LinkArrows.Join(", ")}\n";
                else
                    desc += $"**Level:** {(monster as IHasLevel).Level}\n";

                if (monster is IHasScale pendulumMonster)
                    desc += $"**Scale:** {pendulumMonster.PendulumScale}\n";

            }
            else
                desc += $"**Property:** {(card as SpellTrap).Property}\n";

            if (card.OcgExists)
                desc += $"**OCG:** {card.OcgStatus}\n";

            if (card.TcgExists)
            {

                desc += $"**TCG ADV:** {card.TcgAdvStatus}\n";
                desc += $"**TCG TRAD:** {card.TcgTrnStatus}\n";

            }

            if (!string.IsNullOrEmpty(card.Passcode))
                desc += $"**Passcode:** {card.Passcode}";

            return desc;

        }

        private Color GetColor(Card card)
        {

            if (card.Name == "Obelisk the Tormentor (original)")
                return new Color(50, 50, 153);
            else if (card.Name == "Slifer the Sky Dragon (original)")
                return new Color(255, 0, 0);
            else if (card.Name == "The Winged Dragon of Ra (original)")
                return new Color(255, 215, 0);

            if (card.CardType == "Spell")
                return new Color(29, 158, 116);
            else if (card.CardType == "Trap")
                return new Color(188, 90, 132);
            else
            {

                var monster = card as Monster;
                var test = monster.Types.FirstOrDefault(type => type.ToLower() == "fusion");

                if (monster is IHasLink)
                    return new Color(0, 0, 139);
                else if (monster is IHasScale)
                    return new Color(150, 208, 189);
                else if (monster is IHasRank)
                    return new Color(0, 0, 1);
                else if (monster.Types.Contains("Fusion"))
                    return new Color(160, 134, 183);
                else if (monster.Types.Contains("Synchro"))
                    return new Color(204, 204, 204);
                else if (monster.Types.Contains("Ritual"))
                    return new Color(157, 181, 204);
                else if (monster.Types.Contains("Effect"))
                    return new Color(255, 139, 83);
                else
                    return new Color(253, 230, 138);

            }

        }

        private string GetIconUrl(Card card)
        {

            if (card is SpellTrap spelltrap)
            {

                switch (spelltrap.Property)
                {

                    case "Ritual":
                        return "http://1.bp.blogspot.com/-AuufBN2P_2Q/UxXrMJAkPJI/AAAAAAAAByQ/ZFuEQPj-UtQ/s1600/Ritual.png";
                    case "Quick-Play":
                        return "http://4.bp.blogspot.com/-4neFVlt9xyk/UxXrMO1cynI/AAAAAAAAByY/WWRyA3beAl4/s1600/Quick-Play.png";
                    case "Field":
                        return "http://1.bp.blogspot.com/-3elroOLxcrM/UxXrK5AzXuI/AAAAAAAABxo/qrMUuciJm8s/s1600/Field.png";
                    case "Equip":
                        return "http://1.bp.blogspot.com/-_7q4XTlAX_g/UxXrKeKbppI/AAAAAAAABxY/uHl2cPYY6PA/s1600/Equip.png";
                    case "Counter":
                        return "http://3.bp.blogspot.com/-EoqEY8ef698/UxXrJRfgnPI/AAAAAAAABxA/e9-pD6CSdwk/s1600/Counter.png";
                    case "Continuous":
                        return "http://3.bp.blogspot.com/-O_1NZeHQBSk/UxXrJfY0EEI/AAAAAAAABxI/vKg5txOFlog/s1600/Continuous.png";
                    default:
                        if (spelltrap.CardType == "Spell")
                            return "http://2.bp.blogspot.com/-RS2Go77CqUw/UxXrMaDiM-I/AAAAAAAAByU/cjc2OyyUzvM/s1600/Spell.png";
                        else
                            return "http://3.bp.blogspot.com/-o8wNPTv-VVw/UxXrNA8kTMI/AAAAAAAAByw/uXwjDLJZPxI/s1600/Trap.png";

                }

            }
            else
            {

                var monster = card as Monster;

                switch (monster.Attribute)
                {

                    case "WIND":
                        return "http://1.bp.blogspot.com/-ndLNmGIXXKk/UxXrNXeUH-I/AAAAAAAABys/rdoqo1Bkhnk/s1600/Wind.png";
                    case "DARK":
                        return "http://1.bp.blogspot.com/-QUU5KSFMYig/UxXrJZoOOfI/AAAAAAAABxE/7p8CLfWdTXA/s1600/Dark.png";
                    case "LIGHT":
                        return "http://1.bp.blogspot.com/-MxQabegkthM/UxXrLHywzrI/AAAAAAAABx8/h86nYieq9nc/s1600/Light.png";
                    case "EARTH":
                        return "http://2.bp.blogspot.com/-5fLcEnHAA9M/UxXrKAcSUII/AAAAAAAABxc/5fEingbdyXQ/s1600/Earth.png";
                    case "FIRE":
                        return "http://4.bp.blogspot.com/-sS0-GqQ19gQ/UxXrLIymRVI/AAAAAAAAByA/aOAdiLerXoQ/s1600/Fire.png";
                    case "WATER":
                        return "http://4.bp.blogspot.com/-A43QT1n8o5k/UxXrNJcG-fI/AAAAAAAAByo/0KFlRXQbZjI/s1600/Water.png";
                    case "DIVINE":
                        return "http://1.bp.blogspot.com/-xZZF5E2NXi4/UxXrJwDWkaI/AAAAAAAABxg/EG-7ajL9WGc/s1600/Divine.png";
                    default:
                        return "http://3.bp.blogspot.com/-12VDHRVnjYk/VHdt3uHWbdI/AAAAAAAACyA/fOgzigv-9XU/s1600/Level.png"; //its a star, rofl

                }

            }

        }

        private void AquireTheUntouchables(IEnumerable<CardParser> parsers)
        {

            var tempban = new Banlist();

            Log("Getting OCG banlist...");

            var ocgBanlist = tempban.OcgBanlist;
            var ocgCards = parsers.Where(parser => parser.OcgExists);
            ocgBanlist.Forbidden = ocgCards.Where(parser => parser.OcgStatus == "Forbidden").Select(CardParser.GetCardName);
            ocgBanlist.Limited = ocgCards.Where(parser => parser.OcgStatus == "Limited").Select(CardParser.GetCardName);
            ocgBanlist.SemiLimited = ocgCards.Where(parser => parser.OcgStatus == "Semi-Limited").Select(CardParser.GetCardName);

            Log("Getting TCG Adv banlist...");

            var tcgAdvBanlist = tempban.TcgAdvBanlist;
            var tcgCards = parsers.Where(parser => parser.TcgExists);
            tcgAdvBanlist.Forbidden = tcgCards.Where(parser => parser.TcgAdvStatus == "Forbidden").Select(CardParser.GetCardName);
            tcgAdvBanlist.Limited = tcgCards.Where(parser => parser.TcgAdvStatus == "Limited").Select(CardParser.GetCardName);
            tcgAdvBanlist.SemiLimited = tcgCards.Where(parser => parser.TcgAdvStatus == "Semi-Limited").Select(CardParser.GetCardName);

            Log("Getting TCG Traditional banlist...");

            var tcgTradBanlist = tempban.TcgTradBanlist;
            tcgTradBanlist.Forbidden = tcgCards.Where(parser => parser.TcgTrnStatus == "Forbidden").Select(CardParser.GetCardName);
            tcgTradBanlist.Limited = tcgCards.Where(parser => parser.TcgTrnStatus == "Limited").Select(CardParser.GetCardName);
            tcgTradBanlist.SemiLimited = tcgCards.Where(parser => parser.TcgTrnStatus == "Semi-Limited").Select(CardParser.GetCardName);

            //_db.Open();

            //Log("Getting OCG banlist...");
            //tempban.OcgBanlist.Forbidden = _db.Query<string>("select name from Card where ocgStatus like 'forbidden' or ocgStatus like 'illegal'");
            //tempban.OcgBanlist.Limited = _db.Query<string>("select name from Card where ocgStatus like 'limited'");
            //tempban.OcgBanlist.SemiLimited = _db.Query<string>("select name from Card where ocgStatus like 'semi-limited'");
            //Log("Getting TCG Adv banlist...");
            //tempban.TcgAdvBanlist.Forbidden = _db.Query<string>("select name from Card where tcgAdvStatus like 'forbidden' or tcgAdvStatus like 'illegal'");
            //tempban.TcgAdvBanlist.Limited = _db.Query<string>("select name from Card where tcgAdvStatus like 'limited'");
            //tempban.TcgTradBanlist.SemiLimited = _db.Query<string>("select name from Card where tcgAdvStatus like 'semi-limited'");
            //Log("Getting TCG Traditional banlist...");
            //tempban.TcgTradBanlist.Forbidden = _db.Query<string>("select name from Card where tcgTrnStatus like 'forbidden' or tcgTrnStatus like 'illegal'");
            //tempban.TcgTradBanlist.Limited = _db.Query<string>("select name from Card where tcgTrnStatus like 'limited'");
            //tempban.TcgTradBanlist.SemiLimited = _db.Query<string>("select name from Card where tcgTrnStatus like 'semi-limited'");

            //_db.Close();

            Banlist = tempban;

        }

        private IEnumerable<CardParser> AquireGoodies()
        {

            _db.Open();

            Log("Retrieving all cards from ygofandom.db...");
            var parsers = _db.Query<CardParser>("select * from Cards where Types not like '%Pegasus%' or Types is null");

            //Log("Getting regular monsters...");
            //var regulars = _db.Query<RegularMonster>("select * from Cards where Level not like -1 and PendulumScale like -1");
            //Log("Getting xyz monsters...");
            //var xyz = _db.Query<Xyz>("select * from Cards where Types like '%Xyz%'"); //includes xyz pendulums
            //Log("Getting pendulum monsters...");
            //var pendulums = _db.Query<RegularMonster>("select * from Cards where Types like '%Pendulum%' and Types not like '%Xyz%'"); //does not include xyz pendulums
            //Log("Getting link monsters...");
            //var links = _db.Query<LinkMonster>("select * from Cards where Types like '%Link%'");
            //Log("Getting spell and traps...");
            //var spelltraps = _db.Query<SpellTrap>("select * from Cards where CardType like '%Spell%' or CardType like '%Trap%'");

            _db.Close();

            return parsers;

        }

        private void AquireGoodiePacks()
        {

            Log("Retrieving all booster packs from ygofandom.db...");

            _db.Open();

            var boosterPacks = _db.Query<BoosterPackParser>("select * from BoosterPacks");

            _db.Close();

            BoosterPacks = boosterPacks
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .ToDictionary(parser => parser.Name, parser => parser.Parse(), StringComparer.InvariantCultureIgnoreCase);

            Log("Finished retrieving all booster packs from ygo.db");

        }

        private void Log(string message)
            => AltConsole.Write("Info", "Cache", message);

        private void InlineLog(string message)
            => AltConsole.InlineWrite("Info", "Cache", message, false);

    }

}
