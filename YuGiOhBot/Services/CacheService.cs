using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//for those wondering why i couldnt just put each cache in its respect service class
//cause im picky with my using keyword, i didnt want yugiohservice to use Discord

namespace YuGiOhBot.Services
{
    public class CacheService
    {

        public ConcurrentDictionary<string, Embed> _yuGiOhCardCache { get; set; }

    }
}
