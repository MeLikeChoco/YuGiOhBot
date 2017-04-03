using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YuGiOhBot.Services.CardObjects;
using YuGiOhBot.Services;
using MoreLinq;
using YuGiOhBot.Core;

namespace YuGiOhBot.Services
{
    public class ChatService
    {

        private YuGiOhServices _yugiohService;
        private const string InlinePattern = @"(\[\[.+?\]\])";

        public ChatService(YuGiOhServices yugiohServiceParams)
        {

            _yugiohService = yugiohServiceParams;

        }

        public async Task SendCard(IMessageChannel channel, YuGiOhCard card, bool minimal)
        {

            EmbedBuilder eBuilder;

            var authorBuilder = new EmbedAuthorBuilder()
            {

                Name = "YuGiOh",
                IconUrl = "http://card-masters.com/cardmasters/wp-content/uploads/2013/08/yugioh-product-icon-lg.png",
                Url = "http://www.yugioh-card.com/en/"

            };

            var footerBuilder = new EmbedFooterBuilder()
            {

                Text = "It's time to d-d-d-d-duel | Database made by chinhodado",
                IconUrl = "http://i722.photobucket.com/albums/ww227/omar_alami/icon_gold_classic_zps66eae1c7.png"

            };

            var organizedDescription = new StringBuilder();

            if (!string.IsNullOrEmpty(card.RealName)) organizedDescription.AppendLine($"**Real Name:** {card.RealName}");

            organizedDescription.AppendLine($"**Format:** {GetFormat(card.TcgOnly, card.OcgOnly)}");
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

            if (minimal)
            {

                eBuilder = new EmbedBuilder()
                {

                    Author = authorBuilder,
                    Color = WhatColorIsTheCard(card),
                    //ImageUrl = card.ImageUrl,
                    ThumbnailUrl = card.ImageUrl,
                    Title = card.Name,
                    Description = organizedDescription.ToString(),
                    Footer = footerBuilder

                };

            }
            else
            {

                eBuilder = new EmbedBuilder()
                {

                    Author = authorBuilder,
                    Color = WhatColorIsTheCard(card),
                    ImageUrl = card.ImageUrl,
                    //ThumbnailUrl = card.ImageUrl,
                    Title = card.Name,
                    Description = organizedDescription.ToString(),
                    Footer = footerBuilder

                };

            }

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

            eBuilder.AddField(x =>
            {

                x.Name = card.HasEffect ? "Effect" : "Description";
                x.Value = description;
                x.IsInline = false;

            });

            if (card is MonsterCard)
            {

                var monster = card as MonsterCard;

                eBuilder.AddField(x =>
                {
                    x.Name = "Attack";
                    x.Value = monster.Atk;
                    x.IsInline = !(monster is LinkMonster);
                });

                if (!(monster is LinkMonster))
                {

                    eBuilder.AddField(x =>
                    {
                        x.Name = "Defense";
                        x.Value = monster.Def;
                        x.IsInline = true;
                    });

                }

            }

            if (!string.IsNullOrEmpty(card.Archetype))
            {

                eBuilder.AddField(x =>
                {

                    x.Name = "Archetype(s)";
                    x.Value = card.Archetype;
                    x.IsInline = false;

                });

            }

            if (!minimal)
            {

                if (card.Prices.data != null)
                {

                    if (card.Prices.data.Count >= 4)
                    {

                        eBuilder.AddField(x =>
                        {

                            x.Name = "Prices";
                            x.Value = "**Showing the first 3 prices due to too many to show**";
                            x.IsInline = false;

                        });

                        List<Datum> prices = card.Prices.data.GetRange(0, 3);

                        foreach (Datum info in prices)
                        {

                            if (string.IsNullOrEmpty(info.price_data.message)) //check if there is an error message
                            {

                                var tempString = $"Rarity: {info.rarity}\n" +
                                $"Low: {info.price_data.data.prices.low.ToString("0.00")}\n" +
                                $"Average: {info.price_data.data.prices.average.ToString("0.00")}\n" +
                                $"High: {info.price_data.data.prices.high.ToString("0.00")}";

                                eBuilder.AddField(x =>
                                {

                                    x.Name = info.name;
                                    x.Value = tempString;
                                    x.IsInline = false;

                                });

                            }
                            else
                            {

                                eBuilder.AddField(x =>
                                {

                                    x.Name = info.name;
                                    x.Value = info.price_data.message;
                                    x.IsInline = false;

                                });

                            }

                        }

                    }
                    else if (card.Prices.data.Count < 4)
                    {

                        foreach (Datum info in card.Prices.data)
                        {

                            if (string.IsNullOrEmpty(info.price_data.message)) //check if there is an error message
                            {

                                var tempString = $"Rarity: {info.rarity}\n" +
                                $"Low: {info.price_data.data.prices.low.ToString("0.00")}\n" +
                                $"Average: {info.price_data.data.prices.average.ToString("0.00")}\n" +
                                $"High: {info.price_data.data.prices.high.ToString("0.00")}";

                                eBuilder.AddField(x =>
                                {

                                    x.Name = info.name;
                                    x.Value = tempString;
                                    x.IsInline = false;

                                });

                            }
                            else
                            {

                                eBuilder.AddField(x =>
                                {

                                    x.Name = info.name;
                                    x.Value = info.price_data.message;
                                    x.IsInline = false;

                                });

                            }

                        }

                    }

                }
                else if (card.Prices.data == null)
                {

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Prices";
                        x.Value = "**No prices to show for this card!**";
                        x.IsInline = false;

                    });

                }

            }

            await channel.SendMessageAsync("", embed: eBuilder);
            CacheService.YuGiOhCardCache.TryAdd(card.Name.ToLower(), eBuilder);

        }

        public async Task InlineCardSearch(SocketMessage message)
        {

            if (message.Author.IsBot)
                return;

            string content = message?.Content;

            if (string.IsNullOrEmpty(content))
                return;

            ISocketMessageChannel channel = message.Channel;
            MatchCollection m = Regex.Matches(content, InlinePattern);

            if (m.Count != 0)
            {

                using (channel.EnterTypingState())
                {

                    if (channel is SocketGuildChannel)
                        await AltConsole.PrintAsync("Service", "Chat", $"{(channel as SocketGuildChannel).Guild.Name}");

                    await AltConsole.PrintAsync("Service", "Chat", $"Inline card recieved, message was: {content}");
                    //had to use m.OfType<Match>() due to matches not implementing generic IEnumerable
                    //thanks stackoverflow :D
                    Parallel.ForEach(m.OfType<Match>(), async (match) =>
                    {

                        string cardName = match.ToString();
                        cardName = cardName.Substring(2, cardName.Length - 4).ToLower();
                        var input = cardName.Split(' ');

                        //check if the card list contains anything from the input and return that instead
                        //ex. kaiju slumber would return Interrupted Kaiju Slumber
                        //note: it has problems such as "red eyes" will return Hundred Eyes Dragon instead of Red-Eyes Dragon
                        //how to accurately solve this problem is not easy
                        string closestCard = _yugiohService.CardList.FirstOrDefault(card => input.All(i => card.Contains(i)));
                        //string closestCard = await _yugiohService.LazyGetCardName(cardName);

                        if (string.IsNullOrEmpty(closestCard))
                            closestCard = _yugiohService.CardList.MinBy(card => Compute(card, cardName));

                        bool minimal;

                        if (channel is SocketGuildChannel)
                        {
                            if (GuildServices.MinimalSettings.TryGetValue((channel as SocketGuildChannel).Guild.Id, out minimal)) { }
                            else
                                minimal = false;
                        }
                        else
                            minimal = false;

                        if (CacheService.YuGiOhCardCache.TryGetValue(closestCard, out EmbedBuilder eBuilder))
                        {

                            if (minimal)
                            {
                                string imgUrl = eBuilder.ImageUrl;

                                if (!string.IsNullOrEmpty(imgUrl))
                                {
                                    eBuilder.ImageUrl = null;
                                    eBuilder.ThumbnailUrl = imgUrl;
                                }
                            }
                            else
                            {

                                string thumbUrl = eBuilder.ThumbnailUrl;

                                if (!string.IsNullOrEmpty(thumbUrl))
                                {
                                    eBuilder.ThumbnailUrl = null;
                                    eBuilder.ImageUrl = thumbUrl;
                                }

                            }

                            await channel.SendMessageAsync("", embed: eBuilder);

                        }
                        else
                        {

                            var correctCard = await _yugiohService.GetCard(closestCard);
                            await SendCard(channel, correctCard, minimal);

                        }


                    });

                }
            }

        }

        /// <summary> Levenshtein Distance </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }

            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {

                for (int j = 1; j <= m; j++)
                {

                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);

                }

            }

            return d[n, m];
        }

        private string GetFormat(bool tcgOnly, bool ocgOnly)
        {

            if (tcgOnly || ocgOnly)
            {
                if (ocgOnly) return "OCG";
                else return "TCG";
            }
            else return "TCG/OCG";

        }

        private Color WhatColorIsTheCard(YuGiOhCard card)
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

    }

}

