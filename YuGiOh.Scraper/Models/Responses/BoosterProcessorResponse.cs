using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Scraper.Models.Responses
{
    public class BoosterProcessorResponse
    {

        public int Count { get; set; }
        public ConcurrentBag<Error> Errors { get; set; }

    }
}
