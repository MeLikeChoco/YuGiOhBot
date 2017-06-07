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
using MoreLinq;
using YuGiOhBot.Core;
using System.Diagnostics;
using Force.DeepCloner;

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

        public async Task InlineCardSearch(SocketMessage message)
        {

            if (message.Author.IsBot)
                return;

            string content = message?.Content;

            if (string.IsNullOrEmpty(content))
                return;

            ISocketMessageChannel channel = message.Channel;
            MatchCollection m = Regex.Matches(content, InlinePattern);
            Stopwatch stopwatch;

            if (m.Count != 0 && m.Count < 4)
            {

                stopwatch = new Stopwatch();
                stopwatch.Start();

                try
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
                            cardName = cardName.Substring(2, cardName.Length - 4).ToLower(); //lose the brackets

                            if (string.IsNullOrEmpty(cardName)) //return if there is no input
                                return;

                            var input = cardName.Split(' ');

                            //check if the card list contains anything from the input and return that instead
                            //ex. kaiju slumber would return Interrupted Kaiju Slumber
                            //note: it has problems such as "red eyes" will return Hundred Eyes Dragon instead of Red-Eyes Dragon
                            //how to accurately solve this problem is not easy                            
                            string closestCard = CacheService.CardNames.AsParallel().FirstOrDefault(card => card.ToLower() == cardName);

                            if (string.IsNullOrEmpty(closestCard))
                            {

                                closestCard = CacheService.CardNames.AsParallel().FirstOrDefault(card => input.All(i => card.ToLower().Contains(i)));

                                if (string.IsNullOrEmpty(closestCard))
                                    closestCard = CacheService.CardNames.AsParallel().MinBy(card => Compute(card.ToLower(), cardName));

                            }

                            bool minimal;

                            if (channel is SocketGuildChannel)
                            {
                                if (GuildServices.MinimalSettings.TryGetValue((channel as SocketGuildChannel).Guild.Id, out minimal)) { }
                                else
                                    minimal = false;
                            }
                            else
                                minimal = false;

                            if (CacheService.CardCache.TryGetValue(closestCard.ToLower(), out EmbedBuilder eBuilder))
                            {

                                try
                                {
                                    await channel.SendMessageAsync("", embed: await AddPriceAndImage(eBuilder, minimal));
                                }
                                catch { await AltConsole.PrintAsync("Service", "Chat", "No permission to send message"); }

                            }

                        });

                    }

                    stopwatch.Stop();
                    await AltConsole.PrintAsync("Command", "Stopwatch", $"Inline search completed in {stopwatch.Elapsed.TotalSeconds} seconds");

                }
                catch (Exception e)
                {

                    await AltConsole.PrintAsync("Service", "Chat", "", e);

                }
            }
            else if (m.Count > 3)
                await channel.SendMessageAsync("Too many cards searched at once.");

        }

        public async Task<EmbedBuilder> AddPriceAndImage(EmbedBuilder embed, bool isMinimal)
        {

            var clone = embed.DeepClone();
            string realName = "";

            if (clone.Description.Contains("Real Name"))
            {

                var description = clone.Description;
                var indexOne = description.IndexOf(':');
                var indexTwo = description.IndexOf("**Format");
                realName = description.Substring(indexOne, indexTwo).Trim();

            }
            else
                realName = clone.Title;

            if (!isMinimal)
            {

                var result = await _yugiohService.GetPrices(clone.Title, realName);
                clone.ImageUrl = await _yugiohService.GetImageUrl(clone.Title, realName);

                if (result.data != null)
                {

                    List<Datum> prices;

                    if (result.data.Count >= 4)
                    {

                        clone.AddField(x =>
                        {

                            x.Name = "Prices";
                            x.Value = "**Showing the first 3 prices due to too many to show**";
                            x.IsInline = false;

                        });

                        prices = result.data.GetRange(0, 3);

                    }
                    else
                        prices = result.data;

                    foreach (Datum info in prices)
                    {

                        if (string.IsNullOrEmpty(info.price_data.message)) //check if there is an error message
                        {

                            var tempString = $"Rarity: {info.rarity}\n" +
                            $"Average Price: ${info.price_data.data.prices.average.ToString("0.00")}";

                            clone.AddField(x =>
                            {

                                x.Name = info.name;
                                x.Value = tempString;
                                x.IsInline = false;

                            });

                        }
                        else
                        {

                            clone.AddField(x =>
                            {

                                x.Name = info.name;
                                x.Value = info.price_data.message;
                                x.IsInline = false;

                            });

                        }

                    }

                }
                else if (result.data == null)
                {

                    clone.AddField(x =>
                    {

                        x.Name = "Prices";
                        x.Value = "**No prices to show for this card!**";
                        x.IsInline = false;

                    });

                }

                return clone;

            }
            else
            {

                clone.ThumbnailUrl = await _yugiohService.GetImageUrl(clone.Title, realName);
                return clone;

            }

        }

        /// <summary> Levenshtein Distance </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private int Compute(string s, string t)
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

    }

}

