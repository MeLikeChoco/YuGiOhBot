using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Handlers
{
    public class ChatHandler
    {

        //private const string Pattern = @"(\[{2}[^\[\]].+?[^\[\]]\]{2})";
        private const string Pattern = @"(?<=\[{2}).+?(?=\]{2})";
        private static readonly Regex InlineRegex = new(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly ILoggerFactory _loggerFactory;
        private readonly Cache _cache;
        private readonly Web _web;
        private readonly IYuGiOhDbService _yuGiOhDbService;
        private readonly IGuildConfigDbService _guildConfigDbService;

        public ChatHandler(
            ILoggerFactory loggerFactory,
            Cache cache,
            Web web,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService
        )
        {

            _loggerFactory = loggerFactory;
            _cache = cache;
            _web = web;
            _yuGiOhDbService = yuGiOhDbService;
            _guildConfigDbService = guildConfigDbService;
            //_ignoreCaseComparer = new IgnoreCaseComparer();

        }

        public Task HandlePotentialInlineSearchAsync(SocketMessage message)
        {

            Task.Run(async () =>
            {

                var logger = _loggerFactory.CreateLogger("Inline");

                if (message.Author.IsBot || string.IsNullOrEmpty(message.Content))
                    return;

                if (message.Channel is SocketGuildChannel guildChannel)
                {

                    var guild = guildChannel.Guild.Id;
                    var guildConfig = await _guildConfigDbService.GetGuildConfigAsync(guild);

                    if (!guildConfig.Inline)
                        return;

                }

                var matches = InlineRegex.Matches(message.Content);
                var watch = new Stopwatch();
                var channel = message.Channel;
                var minimal = false;

                if (matches.Count is > 0 and < 4)
                {

                    if (channel is SocketTextChannel textChannel)
                    {

                        logger.Info("{Username} from {GuildName}", message.Author.GetFullUsername(), textChannel.Guild.Name);
                        var id = textChannel.Guild.Id;
                        var guildConfig = await _guildConfigDbService.GetGuildConfigAsync(id);
                        minimal = guildConfig.Minimal;

                    }

                    logger.Info(message.Content);

                    foreach (var match in matches)
                    {

                        watch.Start();

                        var cardName = match.ToString().ConvertTypesetterToTypewriter().Trim();

                        if (string.IsNullOrEmpty(cardName) || string.IsNullOrWhiteSpace(cardName)) //continue if there is no input
                            continue;

                        // var input = cardName.Split(' ');
                        var closestCard = await _yuGiOhDbService.GetCardAsync(cardName) ??
                                          (await _yuGiOhDbService.SearchCardsAsync(cardName)).FirstOrDefault() ??
                                          (await _yuGiOhDbService.GetCardsContainsAllAsync(cardName)).FirstOrDefault() ??
                                          await _yuGiOhDbService.GetClosestCardAsync(cardName);

                        //for easier debugging
                        //var closestCard = await _yuGiOhDbService.GetCardAsync(cardName);

                        //if (closestCard == null)
                        //{

                        //    closestCard = await _yuGiOhDbService.SearchCardsAsync(cardName, 1).ContinueWith(task => task.Result.FirstOrDefault());

                        //    if (closestCard == null)
                        //    {

                        //        closestCard = await _yuGiOhDbService.GetCardsContainsAllAsync(cardName).ContinueWith(task => task.Result.FirstOrDefault());

                        //        if (closestCard == null)
                        //            closestCard = await _yuGiOhDbService.GetClosestCardAsync(cardName);

                        //    }

                        //}

                        watch.Stop();

                        var elapsed = watch.Elapsed;
                        var time = elapsed.TotalSeconds;

                        logger.Info("{CardName} took {Time} {Quantifier:l} to fetch", cardName, time > 1 ? time : time * 1000, time > 1 ? "seconds" : "milliseconds");

                        try
                        {

                            var embed = closestCard.GetEmbedBuilder();
                            await channel.SendMessageAsync(embed: (await embed.WithCardPrices(minimal, _web, elapsed)).Build());

                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, string.Empty);
                        }

                    }

                }

            });

            return Task.CompletedTask;

        }

        //dont even ask me what the fuck im doing

        #region Levenshtein Distance

        private unsafe int YetiLevenshtein(string s1, string s2)
        {
            fixed (char* p1 = s1)
            fixed (char* p2 = s2)
            {
                return YetiLevenshtein(p1, s1.Length, p2, s2.Length, 0); // substitutionCost = 1
            }
        }

        private unsafe int YetiLevenshtein(string s1, string s2, int substitionCost)
        {
            int xc = substitionCost - 1;
            if (xc < 0 || xc > 1)
            {
                throw new ArgumentException("", nameof(substitionCost));
            }

            fixed (char* p1 = s1)
            fixed (char* p2 = s2)
            {
                return YetiLevenshtein(p1, s1.Length, p2, s2.Length, xc);
            }
        }

        /// <summary>
        /// Cetin Sert, David Necas
        /// http://webcleaner.svn.sourceforge.net/viewvc/webcleaner/trunk/webcleaner2/wc/levenshtein.c?revision=6015&view=markup
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="l1"></param>
        /// <param name="s2"></param>
        /// <param name="l2"></param>
        /// <param name="xcost"></param>
        /// <returns></returns>
        private unsafe int YetiLevenshtein(
            char* s1,
            int l1,
            char* s2,
            int l2,
            int xcost
        )
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
                int nx = l1;
                char* sx = s1;
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
                    //return l2 + 1 - 2*(memchr(s2, *s1, l2) is not null);
                    return l2 + 1 - 2 * MemchrRPLC(s2, *s1, l2);
                else
                    //return l2 - (memchr(s2, *s1, l2) is not null);
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
                return (int) (-1);
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
                    int* p = row + 1;
                    char char1 = s1[i - 1];
                    char* char2p = s2;
                    int D = i;
                    int x = i;
                    while (p <= end)
                    {
                        if (char1 == *(char2p++))
                            x = --D;
                        else
                            x++;
                        D = *p;
                        D++;
                        if (x > D)
                            x = D;
                        *(p++) = x;
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
                    char char1 = s1[i - 1];
                    char* char2p;
                    int D, x;
                    /* skip the upper triangle */
                    if (i >= l1 - half)
                    {
                        int offset = i - (l1 - half);
                        int c3;

                        char2p = s2 + offset;
                        p = row + offset;
                        c3 = *(p++) + ((char1 != *(char2p++)) ? 1 : 0);
                        x = *p;
                        x++;
                        D = x;
                        if (x > c3)
                            x = c3;
                        *(p++) = x;
                    }
                    else
                    {
                        p = row + 1;
                        char2p = s2;
                        D = x = i;
                    }

                    /* skip the lower triangle */
                    if (i <= half + 1)
                        end = row + l2 + i - half - 2;
                    /* main */
                    while (p <= end)
                    {
                        int c3 = --D + ((char1 != *(char2p++)) ? 1 : 0);
                        x++;
                        if (x > c3)
                            x = c3;
                        D = *p;
                        D++;
                        if (x > D)
                            x = D;
                        *(p++) = x;
                    }

                    /* lower triangle sentinel */
                    if (i <= half)
                    {
                        int c3 = --D + ((char1 != *char2p) ? 1 : 0);
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
            char* p = buffer;
            char* e = buffer + count;
            while (p++ < e)
            {
                if (*p == c)
                    return 1;
            }

            return 0;
        }

        #endregion Levenshtein Distance

    }
}