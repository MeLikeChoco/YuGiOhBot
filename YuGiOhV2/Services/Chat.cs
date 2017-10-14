using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Force.DeepCloner;
using MoreLinq;
using YuGiOhV2.Objects.Deserializers;

namespace YuGiOhV2.Services
{
    public class Chat
    {
        private const string Pattern = @"(\[\[.+?\]\])";
        private static Web _web;

        private readonly Cache _cache;
        private readonly Database _database;

        public Chat(Cache cache, Database database, Web web)
        {
            _cache = cache;
            _database = database;
            _web = web;
        }

        public async Task SOMEONEGETTINGACARDBOIS(SocketMessage message)
        {
            if (message.Author.IsBot || string.IsNullOrEmpty(message.Content))
                return;

            var mCollection = Regex.Matches(message.Content, Pattern);
            var watch = new Stopwatch();
            var channel = message.Channel;
            var minimal = false;

            if (mCollection.Count > 0 && mCollection.Count < 4)
                using (channel.EnterTypingState())
                {
                    if (channel is SocketTextChannel)
                    {
                        AltConsole.Print("Info", "Command",
                            $"{message.Author.Username} from {(channel as SocketTextChannel).Guild.Name}");
                        var id = (channel as SocketTextChannel).Guild.Id;
                        minimal = _database.Settings[id].Minimal;
                    }

                    AltConsole.Print("Info", "Inline", $"{message.Content}");

                    foreach (var match in mCollection)
                    {
                        watch.Start();

                        var cardName = match.ToString();
                        cardName = cardName.Substring(2, cardName.Length - 4).ToLower(); //lose the brackets

                        if (string.IsNullOrEmpty(cardName)) //return if there is no input
                            return;

                        var input = cardName.Split(' ');

                        //check if the card list contains anything from the input and return that instead
                        //ex. kaiju slumber would return Interrupted Kaiju Slumber
                        //note: it has problems such as "red eyes" will return Hundred Eyes Dragon instead of Red-Eyes Dragon
                        //how to accurately solve this problem is not easy                            
                        var closestCard = _cache.Lowercase.AsParallel().FirstOrDefault(card => card == cardName);

                        if (string.IsNullOrEmpty(closestCard))
                        {
                            closestCard = _cache.Lowercase.AsParallel().FirstOrDefault(card => card.Contains(cardName));

                            if (string.IsNullOrEmpty(closestCard))
                            {
                                closestCard = _cache.Lowercase.AsParallel()
                                    .FirstOrDefault(card => input.All(i => card.Contains(i)));

                                if (string.IsNullOrEmpty(closestCard))
                                    closestCard = _cache.Lowercase.AsParallel()
                                        .MinBy(card => YetiLevenshtein(card, cardName));
                            }
                        }

                        var time = watch.Elapsed;

                        watch.Stop();
                        AltConsole.Print("Info", "Inline", $"{cardName} took {time.TotalSeconds} seconds to complete.");

                        var embed = _cache.Cards[closestCard];

                        try
                        {
                            await channel.SendMessageAsync("", embed: (await EditEmbed(embed, minimal, time)).Build());
                        }
                        catch
                        {
                            AltConsole.Print("Service", "Chat", "No permission to send message");
                        }
                    }
                }
        } //look at em brackets

        public static async Task<EmbedBuilder> EditEmbed(EmbedBuilder embed, bool minimal, TimeSpan? searchTime = null)
        {
            var clone = embed.DeepClone();
            TimeSpan time;

            if (searchTime != null)
            {
                time = searchTime.Value;
                var rounded = Math.Round(time.TotalSeconds, 5, MidpointRounding.ToEven).ToString("0.00000");

                clone.Footer.WithText($"Search time: {rounded} seconds");
            }

            if (minimal)
            {
                clone.ThumbnailUrl = clone.ImageUrl;
                clone.ImageUrl = null;
            }
            else
            {
                string realName;

                if (clone.Description.Contains("Real Name"))
                {
                    var indexOne = clone.Description.IndexOf(':');
                    var indexTwo = clone.Description.IndexOf("**Format", StringComparison.Ordinal);
                    realName = clone.Description.Substring(indexOne, indexTwo).Trim();
                }
                else
                {
                    realName = clone.Author.Name;
                }

                var response = await _web.GetPrices(clone.Author.Name, realName);

                if (response.Data != null)
                {
                    List<Datum> prices;

                    if (response.Data.Count >= 4)
                    {
                        clone.AddField("Prices", "**Showing the first 3 prices due to too many to show**");

                        prices = response.Data.GetRange(0, 3);
                    }
                    else
                    {
                        prices = response.Data;
                    }

                    foreach (var info in prices)
                        if (string.IsNullOrEmpty(info.PriceData.Message))
                            clone.AddField(info.Name,
                                $"Rarity: {info.Rarity}\n" +
                                $"Average Price: {info.PriceData.Data.Prices.Average.ToString("0.00")}");
                        else
                            clone.AddField(info.Name, info.PriceData.Message);
                }
                else
                {
                    clone.AddField("Prices", "**No prices to show for this card!**");
                }
            }

            return clone;
        }

        //dont even ask me what the fuck im doing
        private unsafe int YetiLevenshtein(string s1, string s2)
        {
            fixed (char* p1 = s1)
            fixed (char* p2 = s2)
            {
                return YetiLevenshtein(p1, s1.Length, p2, s2.Length, 0); // substitutionCost = 1
            }
        }

        /// <summary>
        ///     Cetin Sert, David Necas ///
        ///     http://webcleaner.svn.sourceforge.net/viewvc/webcleaner/trunk/webcleaner2/wc/levenshtein.c?revision=6015&view
        ///     =markup
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="l1"></param>
        /// <param name="s2"></param>
        /// <param name="l2"></param>
        /// <param name="xcost"></param>
        /// <returns></returns>
        private unsafe int YetiLevenshtein(char* s1, int l1, char* s2, int l2, int xcost)
        {
            int i;
            //int *row;  /* we only need to keep one row of costs */
            int* end;
            int half;

            /* strip common prefix */
            while (l1 > 0 && l2 > 0 && *s1 == *s2)
            {
                l1--;
                l2--;
                s1++;
                s2++;
            }

            /* strip common suffix */
            while (l1 > 0 && l2 > 0 && s1[l1 - 1] == s2[l2 - 1])
            {
                l1--;
                l2--;
            }

            /* catch trivial cases */
            if (l1 == 0)
                return l2;
            if (l2 == 0)
                return l1;

            /* make the inner cycle (i.e. string2) the longer one */
            if (l1 > l2)
            {
                var nx = l1;
                var sx = s1;
                l1 = l2;
                l2 = nx;
                s1 = s2;
                s2 = sx;
            }

            //check len1 == 1 separately
            if (l1 == 1)
            {
                //throw new NotImplementedException();
                if (xcost > 0)
                    //return l2 + 1 - 2*(memchr(s2, *s1, l2) != NULL);
                    return l2 + 1 - 2 * MemchrRPLC(s2, *s1, l2);
                return l2 - MemchrRPLC(s2, *s1, l2);
            }

            l1++;
            l2++;
            half = l1 >> 1;

            /* initalize first row */
            //row = (int*)malloc(l2*sizeof(int));
            int* row = stackalloc int[l2];
            if (l2 < 0)
                //if (!row)
                return -1;
            end = row + l2 - 1;
            for (i = 0; i < l2 - (xcost > 0 ? 0 : half); i++)
                row[i] = i;

            /* go through the matrix and compute the costs.  yes, this is an extremely
             * obfuscated version, but also extremely memory-conservative and
             * relatively fast.
             */
            if (xcost > 0)
            {
                for (i = 1; i < l1; i++)
                {
                    var p = row + 1;
                    var char1 = s1[i - 1];
                    var char2P = s2;
                    var d = i;
                    var x = i;
                    while (p <= end)
                    {
                        if (char1 == *char2P++)
                            x = --d;
                        else
                            x++;
                        d = *p;
                        d++;
                        if (x > d)
                            x = d;
                        *p++ = x;
                    }
                }
            }
            else
            {
                /* in this case we don't have to scan two corner triangles (of size len1/2)
                 * in the matrix because no best path can go throught them. note this
                 * breaks when len1 == len2 == 2 so the memchr() special case above is
                 * necessary */
                row[0] = l1 - half - 1;
                for (i = 1; i < l1; i++)
                {
                    int* p;
                    var char1 = s1[i - 1];
                    char* char2p;
                    int d, x;
                    /* skip the upper triangle */
                    if (i >= l1 - half)
                    {
                        var offset = i - (l1 - half);
                        int c3;

                        char2p = s2 + offset;
                        p = row + offset;
                        c3 = *p++ + (char1 != *char2p++ ? 1 : 0);
                        x = *p;
                        x++;
                        d = x;
                        if (x > c3)
                            x = c3;
                        *p++ = x;
                    }
                    else
                    {
                        p = row + 1;
                        char2p = s2;
                        d = x = i;
                    }
                    /* skip the lower triangle */
                    if (i <= half + 1)
                        end = row + l2 + i - half - 2;
                    /* main */
                    while (p <= end)
                    {
                        var c3 = --d + (char1 != *char2p++ ? 1 : 0);
                        x++;
                        if (x > c3)
                            x = c3;
                        d = *p;
                        d++;
                        if (x > d)
                            x = d;
                        *p++ = x;
                    }
                    /* lower triangle sentinel */
                    if (i <= half)
                    {
                        var c3 = --d + (char1 != *char2p ? 1 : 0);
                        x++;
                        if (x > c3)
                            x = c3;
                        *p = x;
                    }
                }
            }

            i = *end;
            return i;
        }

        private unsafe int MemchrRPLC(char* buffer, char c, int count)
        {
            var p = buffer;
            var e = buffer + count;
            while (p++ < e)
                if (*p == c)
                    return 1;
            return 0;
        }
    }
}