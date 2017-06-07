using Discord;
using YuGiOhBot.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhBot.Services.CardObjects;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;
using Dapper;

//for those wondering why i couldnt just put each cache in its respect service class
//cause im picky with my using keyword, i didnt want yugiohservice to use Discord

namespace YuGiOhBot.Services
{
    public static class CacheService
    {
        
        public static Dictionary<string, EmbedBuilder> CardCache { get; private set; }
        public static HashSet<string> CardNames { get; private set; }

        private static readonly ParallelOptions POptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        private static readonly JsonSerializerSettings JSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.Auto };
        private static YuGiOhServices _service;

        public static async Task InitializeService(YuGiOhServices service)
        {

            await AltConsole.PrintAsync("Service", "Cache", "Initializing card cache...");

            List<YuGiOhCard> cards;
            ConcurrentDictionary<string, EmbedBuilder> embeds;
            ConcurrentBag<string> cardNames;
            _service = service; 
            var counter = 0;
            var DbPath = "Data Source = Databases/YgoSqliteDb/ygo.db";

            using (var connection = new SqliteConnection(DbPath))
            {

                await connection.OpenAsync();

                await Print("Getting regular monsters...");
                var regularMonsters = await connection.QueryAsync<RegularMonster>("select * from Card where level not like '' and pendulumScale like ''");
                await Print("Getting xyz monsters...");
                var xyzMonsters = await connection.QueryAsync<XyzMonster>("select * from Card where types like '%Xyz%'");
                await Print("Getting pendulum monsters...");
                var pendulumMonsters = await connection.QueryAsync<PendulumMonster>("select * from Card where types like '%Pendulum%'");
                await Print("Getting link monsters...");
                var linkMonsters = await connection.QueryAsync<LinkMonster>("select * from Card where types like '%Link%'");
                await Print("Getting spell and traps...");
                var spellTraps = await connection.QueryAsync<SpellTrapCard>("select * from Card where cardType like '%Spell%' or cardType like '%Trap%'");

                connection.Close();

                cards = regularMonsters.Concat<YuGiOhCard>(xyzMonsters).Concat(pendulumMonsters).Concat(linkMonsters).Concat(spellTraps).ToList();

            }

            var totalAmount = cards.Count;
            embeds = new ConcurrentDictionary<string, EmbedBuilder>();
            cardNames = new ConcurrentBag<string>();

            Parallel.ForEach(cards, POptions, card =>
            {

                var name = card.Name;
                var embed = GetEmbed(card);

                embeds[name.ToLower()] = embed;
                cardNames.Add(name);
                

                AltConsole.InLinePrint("Service", "Cache", $"Progress: {Interlocked.Increment(ref counter)}/{totalAmount}");

            });

            CardCache = new Dictionary<string, EmbedBuilder>(embeds);
            CardNames = new HashSet<string>(cardNames);

            await AltConsole.PrintAsync("Service", "Cache", "Card cache initialized.");

        }

        private static EmbedBuilder GetEmbed(YuGiOhCard card)
        {

            var author = new EmbedAuthorBuilder()
            {

                Name = "YuGiOh",
                IconUrl = "http://card-masters.com/cardmasters/wp-content/uploads/2013/08/yugioh-product-icon-lg.png",
                Url = "http://www.yugioh-card.com/en/"

            };

            var footer = new EmbedFooterBuilder()
            {

                Text = "It's time to d-d-d-d-duel | Database made by chinhodado",
                IconUrl = "http://i722.photobucket.com/albums/ww227/omar_alami/icon_gold_classic_zps66eae1c7.png"

            };

            var organizedDescription = new StringBuilder();

            if (!string.IsNullOrEmpty(card.RealName))
                organizedDescription.AppendLine($"**Real Name:** {card.RealName}");

            organizedDescription.AppendLine($"**Format:** {GetFormat(card.TcgOnly == "1" ? true : false, card.OcgOnly == "1" ? true : false)}");
            organizedDescription.AppendLine($"**Card Type:** {card.CardType}");

            if (card is MonsterCard)
            {

                var monster = card as MonsterCard;
                organizedDescription.AppendLine($"**Attribute:** {monster.Attribute}");

                if (monster is XyzMonster)
                {
                    var xyz = monster as XyzMonster;
                    organizedDescription.AppendLine($"**Rank:** {xyz.Rank}");
                }
                else if (monster is LinkMonster)
                {
                    var link = monster as LinkMonster;
                    organizedDescription.AppendLine($"**Links:** {link.Links}");
                    organizedDescription.AppendLine($"**Link Markers:** {link.LinkMarkers}");
                }
                else
                {
                    var regular = monster as RegularMonster;
                    organizedDescription.AppendLine($"**Level:** {regular.Level}");
                }

                if (monster is PendulumMonster)
                {
                    var pendulum = monster as PendulumMonster;
                    organizedDescription.AppendLine($"**Scale:** {pendulum.PendulumScale}");
                }

            }
            else
            {

                var spelltrap = card as SpellTrapCard;
                organizedDescription.AppendLine($"**Property:** {spelltrap.Property}");

            }

            var embed = new EmbedBuilder()
            {

                Author = author,
                Color = WhatColorIsTheCard(card),
                Title = card.Name,
                Description = organizedDescription.ToString(),
                Footer = footer

            };

            string description;

            if (card.Lore.StartsWith("Pendulum Effect")) //not all pendulum monsters have an effect
            {

                var monster = card as MonsterCard;
                var tempArray = card.Lore.Split(new string[] { "Monster Effect" }, StringSplitOptions.None);
                description = $"{monster.Types}\n" + "__Pendulum Effect__\n" + tempArray[0].Replace("Pendulum Effect", "").Replace("\" ", "\"").Trim() + "\n__Monster Effect__\n" + tempArray[1].Replace("Monster Effect", "").Replace("\" ", "\"").Trim();

            }
            else if (card is MonsterCard)
            {

                var monster = card as MonsterCard;
                description = $"{monster.Types}\n" + card.Lore;

            }
            else description = card.Lore;

            embed.AddField(x =>
            {

                x.Name = card.HasEffect ? "Effect" : "Description";
                x.Value = description;
                x.IsInline = false;

            });

            if (card is MonsterCard)
            {

                var monster = card as MonsterCard;

                embed.AddField(x =>
                {
                    x.Name = "Attack";
                    x.Value = monster.Atk;
                    x.IsInline = !(monster is LinkMonster);
                });

                if (!(monster is LinkMonster))
                {

                    embed.AddField(x =>
                    {
                        x.Name = "Defense";
                        x.Value = monster.Def;
                        x.IsInline = true;
                    });

                }

            }

            if (!string.IsNullOrEmpty(card.Archetype))
            {

                embed.AddField(x =>
                {

                    x.Name = "Archetype(s)";
                    x.Value = card.Archetype;
                    x.IsInline = false;

                });

            }

            return embed;


        }

        private static string GetFormat(bool tcgOnly, bool ocgOnly)
        {

            if (tcgOnly || ocgOnly)
            {
                if (ocgOnly) return "OCG";
                else return "TCG";
            }
            else return "TCG/OCG";

        }

        private static Color WhatColorIsTheCard(YuGiOhCard card)
        {

            if (card.Name.Equals("Slifer the Sky Dragon")) return new Color(255, 0, 0);
            else if (card.Name.Equals("The Winged Dragon of Ra")) return new Color(255, 215, 0);
            else if (card.Name.Equals("Obelisk the Tormentor")) return new Color(50, 50, 153);

            if (card.CardType.Equals("Trap")) return new Color(188, 90, 132);
            else if (card.CardType.Equals("Spell")) return new Color(29, 158, 116);

            if (card is PendulumMonster) return new Color(175, 219, 205);
            else if (card is XyzMonster) return new Color(0, 0, 1);
            else if (card is LinkMonster) return new Color(17, 57, 146);
            else if (card is RegularMonster)
            {

                var monster = card as RegularMonster;
                if (monster.Types.Contains("Synchro")) return new Color(204, 204, 204);
                else if (monster.Types.Contains("Fusion")) return new Color(160, 134, 183);
                else if (monster.Types.Contains("Ritual")) return new Color(157, 181, 204);
                else if (monster.Types.Contains("Effect")) return new Color(174, 121, 66);
                else if (monster.Types.Contains("Token")) return new Color(192, 192, 192);

            }

            return new Color(216, 171, 12);

        }

        public static async Task Print(string message)
        => await AltConsole.PrintAsync("Service", "Cache", message);

    }
    
}
