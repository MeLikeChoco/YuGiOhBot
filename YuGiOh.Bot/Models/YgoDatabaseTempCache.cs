using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Models
{
    public class YgoDatabaseTempCache
    {

        public IDictionary<string, string> TCG;
        public IDictionary<string, string> OCG;

        public YgoDatabaseTempCache()
        {

            TCG = new ConcurrentDictionary<string, string>();
            OCG = new ConcurrentDictionary<string, string>();

        }

    }
}
