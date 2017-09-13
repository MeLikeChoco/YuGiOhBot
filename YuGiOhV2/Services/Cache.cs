using Dapper;
using Discord;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Objects.Cards;

namespace YuGiOhV2.Services
{
    public class Cache
    {

        public Dictionary<string, EmbedBuilder> Cards { get; private set; }
        public Dictionary<string, string> Images { get; private set; }
        public HashSet<string> Uppercase { get; private set; }
        public HashSet<string> Lowercase { get; private set; }

        private const string DbString = "Data Source = Databases/ygo.db";
        private static readonly ParallelOptions POptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

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

        }

        private void AquireFancyMessages(IEnumerable<Card> objects)
        {

            var counter = 0;
            var total = objects.Count();
            var tempDict = new ConcurrentDictionary<string, EmbedBuilder>();
            var tempImages = new ConcurrentDictionary<string, string>();
            var tempUpper = new ConcurrentBag<string>();
            var tempLower = new ConcurrentBag<string>();

            Print("Generating them fancy embed messages...");

            var test = new object();

            Parallel.ForEach(objects, POptions, cardobj =>
            {

                var name = cardobj.Name;
                var embed = GenFancyMessage(cardobj);

                tempUpper.Add(name);
                tempLower.Add(name.ToLower());
                tempDict[name.ToLower()] = embed;
                tempImages[name.ToLower()] = cardobj.Img;

                lock (test)
                {

                    var current = Interlocked.Increment(ref counter);

                    if(current != total)
                        InlinePrint($"Progress: {current}/{total}");
                    else
                        Print($"Progress: {current}/{total}");

                }

            });

            Print("Finished generating embeds.");

            Cards = new Dictionary<string, EmbedBuilder>(tempDict);
            Uppercase = new HashSet<string>(tempUpper);
            Lowercase = new HashSet<string>(tempLower);

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
                ImageUrl = card.Img,
                Description = GenDescription(card)

            };

            if (card is Monster)
            {

                var monster = card as Monster;

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
                body.AddField("Effect", card.Lore);

            if (card is Monster)
            {

                var monster = card as Monster;

                body.AddField("Attack", monster.Atk, true);

                if (!(monster is Link))
                    body.AddField("Defence", monster.Def, true);

            }

            if (!string.IsNullOrEmpty(card.Archetype))
                body.AddField(card.Archetype.Split("/").Length > 1 ? "Archetypes" : "Archetype", card.Archetype.Replace(" /", ","));

            return body;

        }

        private string GenDescription(Card card)
        {

            string desc = "";

            if (!string.IsNullOrEmpty(card.RealName))
                desc += $"**Real Name:** {card.RealName}\n";

            desc += "**Format:** ";

            if (card.TcgOnly == 1)
                desc += "TCG\n";
            else if (card.OcgOnly == 1)
                desc += "OCG\n";
            else
                desc += "TCG/OCG\n";

            desc += $"**Card Type:** {card.CardType}\n";

            if (card is Monster)
            {

                var monster = card as Monster;
                desc += $"**Attribute:** {monster.Attribute}\n";

                if (monster is Xyz)
                {
                    var xyz = monster as Xyz;
                    desc += $"**Rank:** {xyz.Rank}\n";
                }
                else if (monster is Link)
                {
                    var link = monster as Link;
                    desc += $"**Links:** {link.Links}\n" +
                        $"**Link Markers:** {link.LinkMarkers}";
                }
                else
                {
                    var regular = monster as RegularMonster;
                    desc += $"**Level:** {regular.Level}\n";
                }

                if (!string.IsNullOrEmpty(monster.Scale))
                    desc += $"**Scale:** {monster.Scale}\n";

            }
            else
            {

                var spelltrap = card as SpellTrap;
                desc += $"**Property:** {spelltrap.Property}";

            }

            return desc;

        }

        private Color GetColor(Card card)
        {

            if (card.Name == "Obelisk the Tormentor")
                return new Color(50, 50, 153);
            else if (card.Name == "Slifer the Sky Dragon")
                return new Color(255, 0, 0);
            else if (card.Name == "The Winged Dragon of Ra")
                return new Color(255, 215, 0);

            if (card.CardType == "Spell")
                return new Color(29, 158, 116);
            else if (card.CardType == "Trap")
                return new Color(188, 90, 132);
            else
            {

                var monster = card as Monster;

                if (monster is Link)
                    return new Color(0, 0, 139);
                else if (!string.IsNullOrEmpty(monster.Scale))
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

            if (card is SpellTrap)
            {

                var spelltrap = card as SpellTrap;

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

        private IEnumerable<Card> AquireGoodies()
        {

            using (var db = new SqliteConnection(DbString))
            {

                db.Open();

                Print("Getting regular monsters...");
                var regulars = db.Query<RegularMonster>("select * from Card where level not like '' and pendulumScale like ''");
                Print("Getting xyz monsters...");
                var xyz = db.Query<Xyz>("select * from Card where types like '%Xyz%'"); //includes xyz pendulums
                Print("Getting pendulum monsters...");
                var pendulums = db.Query<RegularMonster>("select * from Card where types like '%Pendulum%' and types not like '%Xyz%'"); //does not include xyz pendulums
                Print("Getting link monsters...");
                var links = db.Query<Link>("select * from Card where types like '%Link%'");
                Print("Getting spell and traps...");
                var spelltraps = db.Query<SpellTrap>("select * from Card where cardType like '%Spell%' or cardType like '%Trap%'");

                db.Close();

                return regulars.Concat<Card>(xyz).Concat(pendulums).Concat(links).Concat(spelltraps).Where(card => !card.Name.Contains("Token"));

            }

        }

        private void Print(string message)
            => AltConsole.Print("Info", "Cache", message);

        private void InlinePrint(string message)
            => AltConsole.InlinePrint("Info", "Cache", message);

    }

}
