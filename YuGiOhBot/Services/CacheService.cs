﻿using Discord;
using YuGiOhBot.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//for those wondering why i couldnt just put each cache in its respect service class
//cause im picky with my using keyword, i didnt want yugiohservice to use Discord

namespace YuGiOhBot.Services
{
    public static class CacheService
    {

        public static ConcurrentDictionary<string, EmbedBuilder> YuGiOhCardCache { get; set; }
        public static ConcurrentDictionary<ulong, IDMChannel> DMChannelCache { get; set; }
        private static Timer _yugiohCacheClearer;
        private static Timer _dmChannelCacheClearer;

        public static void InitializeService()
        {

            YuGiOhCardCache = new ConcurrentDictionary<string, EmbedBuilder>();
            DMChannelCache = new ConcurrentDictionary<ulong, IDMChannel>();

            //yes yes, i could merge both timers into one, but i like it when code is clear
            _yugiohCacheClearer = new Timer(async (state) =>
            {

                await AltConsole.PrintAsync("Service", "Cache", "Checking YuGiOh cache...");
                if (YuGiOhCardCache.Count > 0)
                {

                    await AltConsole.PrintAsync("Service", "Cache", "Clearing YuGiOh cache...");
                    YuGiOhCardCache.Clear();
                    await AltConsole.PrintAsync("Service", "Cache", "Cache cleared.");

                }
                else await AltConsole.PrintAsync("Service", "Cache", "YuGiOh cache does not need to be cleared.");

            }, null, 86400000, 86400000);

            _dmChannelCacheClearer = new Timer(async (state) =>
            {

                await AltConsole.PrintAsync("Service", "Cache", "Checking DMChannel cache...");
                if (DMChannelCache.Count > 0)
                {

                    await AltConsole.PrintAsync("Service", "Cache", "Clearing DMChannel cache...");
                    DMChannelCache.Clear();
                    await AltConsole.PrintAsync("Service", "Cache", "Cache cleared.");

                }
                else await AltConsole.PrintAsync("Service", "Cache", "DMChannel cache does not need to be cleared.");


            }, null, 86400000, 86400000);

        }

    }
}