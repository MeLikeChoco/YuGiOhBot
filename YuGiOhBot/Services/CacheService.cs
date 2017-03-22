using Discord;
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

        public static ConcurrentDictionary<string, EmbedBuilder> _yugiohCardCache { get; set; }
        private static Timer _yugiohCacheClearer;

        public static void InitializeService()
        {

            _yugiohCardCache = new ConcurrentDictionary<string, EmbedBuilder>();

            _yugiohCacheClearer = new Timer(async (state) =>
            {

                await AltConsole.PrintAsync("Service", "Cache", "Checking YuGiOh cache...");
                if (_yugiohCardCache.Count > 0)
                {

                    await AltConsole.PrintAsync("Service", "Cache", "Clearing YuGiOh cache...");
                    _yugiohCardCache.Clear();
                    await AltConsole.PrintAsync("Service", "Cache", "Cache cleared.");

                }
                else await AltConsole.PrintAsync("Service", "Cache", "YuGiOh cache does not need to be cleared.");

            }, null, 86400000, 86400000);

        }

    }
}
