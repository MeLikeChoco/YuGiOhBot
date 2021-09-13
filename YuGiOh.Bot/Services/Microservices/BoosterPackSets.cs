using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Services.Microservices
{
    public class BoosterPackSets
    {

        private const string Url = "https://yugipedia.com/wiki/Template:Booster_Packs";
        private readonly Timer _scrapeScheduler;

        public BoosterPackSets(Cache cache)
        {

            _scrapeScheduler = new Timer(GetValidBoosterPacks, cache, TimeSpan.FromMinutes(1), TimeSpan.FromDays(7));

        }

        public void GetValidBoosterPacks(object info)
        {



        }

    }
}
