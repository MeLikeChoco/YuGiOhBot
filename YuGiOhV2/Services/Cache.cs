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
using YuGiOhV2.Objects.Banlist;
using YuGiOhV2.Objects.BoosterPacks;
using YuGiOhV2.Objects.Cards;
using YuGiOhV2.Objects.Exceptions;

namespace YuGiOhV2.Services
{
    public class Cache
    {

        public Dictionary<string, Card> Objects { get; private set; }
        public Dictionary<string, EmbedBuilder> Cards { get; private set; }
        public Dictionary<string, Booster> BoosterPacks { get; private set; }
        public Dictionary<string, HashSet<string>> Archetypes { get; private set; }
        public Dictionary<string, string> Images { get; private set; }
        public Dictionary<string, string> LowerToUpper { get; private set; }
        public HashSet<string> Uppercase { get; private set; }
        public HashSet<string> Lowercase { get; private set; }

        /// <summary>
        /// Maps name to passcode
        /// </summary>
        public Dictionary<string, string> Passcodes { get; private set; }

        public Banlist Banlist { get; private set; }

        public int FYeahYgoCardArtPosts { get; private set; }

        public string TumblrKey { get; private set; }

        private const string DbString = "Data Source = Databases/ygo.db";
        private static readonly ParallelOptions _pOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        private static SqliteConnection _db = new SqliteConnection(DbString);

        public Cache()
        {

            Print("Beginning cache initialization...");

            //made a seperate method so other classes may update the embeds when I want them to
            Initialize();

            Print("Finished cache initialization...");

        }

        public void Initialize()
        {

            var objects = AquireGoodies();

            AquireFancyMessages(objects);
            //AquireTheUntouchables();
            //AquireGoodiePacks();

        }

        public async Task GetAWESOMECARDART(Web web)
        {

            Print("Getting photo posts on FYeahYgoCardArt tumblr...");

            TumblrKey = await File.ReadAllTextAsync("Files/OAuth/Tumblr.txt");
            var posts = await web.GetDeserializedContent<JObject>($"https://api.tumblr.com/v2/blog/fyeahygocardart/posts/photo?api_key={TumblrKey}&limit=1");
            FYeahYgoCardArtPosts = int.Parse(posts["response"]["total_posts"].ToString());

            Print($"Got {FYeahYgoCardArtPosts} photos.");

        }

        private void AquireFancyMessages(IEnumerable<Card> objects)
        {

            var counter = 0;
            var total = objects.Count();
            var tempObjects = new ConcurrentDictionary<string, Card>();
            var tempDict = new ConcurrentDictionary<string, EmbedBuilder>();
            var tempImages = new ConcurrentDictionary<string, string>();
            var tempLowerToUpper = new ConcurrentDictionary<string, string>();
            var tempUpper = new ConcurrentBag<string>();
            var tempLower = new ConcurrentBag<string>();
            var tempPasscodes = new ConcurrentDictionary<string, string>();
            Archetypes = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);

            Print("Generating them fancy embed messages...");

            var aLock = new object();

            Parallel.ForEach(objects, _pOptions, cardobj =>
            {

                var name = cardobj.Name;
                EmbedBuilder embed;

                try
                {

                    embed = GenFancyMessage(cardobj);

                }
                catch (Exception ex)
                {

                    throw new EmbedGenerationException(name, ex);

                }

                tempUpper.Add(name);
                tempLower.Add(name.ToLower());

                if (!string.IsNullOrEmpty(cardobj.Passcode))
                    tempPasscodes[cardobj.Name] = cardobj.Passcode.TrimStart('0'); //man, why you guys gotta include 0's in the beginning sometimes

                tempObjects[name] = cardobj;
                tempDict[name] = embed;
                tempImages[name] = cardobj.Img;
                tempLowerToUpper[name.ToLower()] = name;

                lock (aLock)
                {

                    if (!string.IsNullOrEmpty(cardobj.Archetype))
                    {

                        var archetypes = cardobj.Archetype.Split(" , ");

                        foreach (var archetype in archetypes)
                        {

                            if (!Archetypes.ContainsKey(archetype))
                                Archetypes.Add(archetype, new HashSet<string>() { name });
                            else
                                Archetypes[archetype].Add(name);

                        }

                    }

                    var current = Interlocked.Increment(ref counter);

                    if (current != total)
                        InlinePrint($"Progress: {current}/{total}");
                    else
                        Print($"Progress: {current}/{total}");

                }

            });

            Print("Finished generating embeds.");

            Objects = new Dictionary<string, Card>(tempObjects, StringComparer.InvariantCultureIgnoreCase);
            Cards = new Dictionary<string, EmbedBuilder>(tempDict, StringComparer.InvariantCultureIgnoreCase);
            Images = new Dictionary<string, string>(tempImages, StringComparer.InvariantCultureIgnoreCase);
            LowerToUpper = new Dictionary<string, string>(tempLowerToUpper, StringComparer.InvariantCultureIgnoreCase);
            Uppercase = new HashSet<string>(tempUpper);
            Lowercase = new HashSet<string>(tempLower);
            Passcodes = new Dictionary<string, string>(tempPasscodes);

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
                        body.AddField($"[ {monster.Types} ]", effects[1].Trim());

                    }
                    else
                        body.AddField($"[ {monster.Types} ]", monster.Lore);

                }
                else
                    body.AddField("Not released yet", "\u200B");

                var unknownValue = "???";

                body.AddField("Attack", string.IsNullOrEmpty(monster.Atk) ? unknownValue : monster.Atk, true);

                if (!(monster is LinkMonster))
                    body.AddField("Defence", string.IsNullOrEmpty(monster.Def) ? unknownValue : monster.Def, true);

            }
            else
                body.AddField("Effect", card.Lore.Replace(@"\n", "\n"));

            if (!string.IsNullOrEmpty(card.Archetype))
                body.AddField(card.Archetype.Split(",").Length > 1 ? "Archetypes" : "Archetype", card.Archetype.Replace(" ,", ","));

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

                if (monster is Xyz xyz)
                    desc += $"**Rank:** {xyz.Rank}\n";
                else if (monster is LinkMonster link)
                    desc += $"**Links:** {(link.Link == 0 ? link.LinkArrows.Split(',').Length : link.Link)}\n" +
                        $"**Link Markers:** {link.LinkArrows}\n";
                else
                    desc += $"**Level:** {(monster as RegularMonster).Level}\n";

                if (monster.PendulumScale != -1)
                    desc += $"**Scale:** {monster.PendulumScale}\n";

            }
            else
                desc += $"**Property:** {(card as SpellTrap).Property}\n";

            if (card.OcgStatus != "U")
                desc += $"**OCG:** {card.OcgStatus}\n";

            if (card.TcgAdvStatus != "U")
                desc += $"**TCG ADV:** {card.TcgAdvStatus}\n";

            if (card.TcgTrnStatus != "U")
                desc += $"**TCG TRAD:** {card.TcgTrnStatus}\n";

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

                if (monster is LinkMonster)
                    return new Color(0, 0, 139);
                else if (monster.PendulumScale != -1)
                    return new Color(150, 208, 189);
                else if (monster is Xyz)
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

        private void AquireTheUntouchables()
        {

            var tempban = new Banlist();

            _db.Open();

            Print("Getting OCG banlist...");
            tempban.OcgBanlist.Forbidden = _db.Query<string>("select name from Card where ocgStatus like 'forbidden' or ocgStatus like 'illegal'");
            tempban.OcgBanlist.Limited = _db.Query<string>("select name from Card where ocgStatus like 'limited'");
            tempban.OcgBanlist.SemiLimited = _db.Query<string>("select name from Card where ocgStatus like 'semi-limited'");
            Print("Getting TCG Adv banlist...");
            tempban.TcgAdvBanlist.Forbidden = _db.Query<string>("select name from Card where tcgAdvStatus like 'forbidden' or tcgAdvStatus like 'illegal'");
            tempban.TcgAdvBanlist.Limited = _db.Query<string>("select name from Card where tcgAdvStatus like 'limited'");
            tempban.TcgTradBanlist.SemiLimited = _db.Query<string>("select name from Card where tcgAdvStatus like 'semi-limited'");
            Print("Getting TCG Traditional banlist...");
            tempban.TcgTradBanlist.Forbidden = _db.Query<string>("select name from Card where tcgTrnStatus like 'forbidden' or tcgTrnStatus like 'illegal'");
            tempban.TcgTradBanlist.Limited = _db.Query<string>("select name from Card where tcgTrnStatus like 'limited'");
            tempban.TcgTradBanlist.SemiLimited = _db.Query<string>("select name from Card where tcgTrnStatus like 'semi-limited'");

            _db.Close();

            Banlist = tempban;

        }

        private IEnumerable<Card> AquireGoodies()
        {

            _db.Open();

            Print("Getting regular monsters...");
            var regulars = _db.Query<RegularMonster>("select * from Cards where Level not like -1 and PendulumScale like -1");
            Print("Getting xyz monsters...");
            var xyz = _db.Query<Xyz>("select * from Cards where Types like '%Xyz%'"); //includes xyz pendulums
            Print("Getting pendulum monsters...");
            var pendulums = _db.Query<RegularMonster>("select * from Cards where Types like '%Pendulum%' and Types not like '%Xyz%'"); //does not include xyz pendulums
            Print("Getting link monsters...");
            var links = _db.Query<LinkMonster>("select * from Cards where Types like '%Link%'");
            Print("Getting spell and traps...");
            var spelltraps = _db.Query<SpellTrap>("select * from Cards where CardType like '%Spell%' or CardType like '%Trap%'");

            _db.Close();

            return regulars.Concat<Card>(xyz).Concat(pendulums).Concat(links).Concat(spelltraps);

        }

        private void AquireGoodiePacks()
        {

            _db.Open();

            var boosterPacks = _db.Query<Booster>("select * from Boosters");

            _db.Close();

            boosterPacks = boosterPacks.GroupBy(bp => bp.Name).Select(group => group.First()).Where(bp => bp.Cards != null && bp.IsEllegible());
            BoosterPacks = new Dictionary<string, Booster>(boosterPacks.ToDictionary(bp => bp.Name, bp => bp), StringComparer.InvariantCultureIgnoreCase);

        }

        private void Print(string message)
            => AltConsole.Print("Info", "Cache", message);

        private void InlinePrint(string message)
            => AltConsole.InlinePrint("Info", "Cache", message, false);

    }

}
