using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Services
{
    public class Prices
    {
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal average { get; set; }
    }

    public class Data
    {
        public List<string> listings { get; set; }
        public Prices prices { get; set; }
    }

    public class PriceData
    {
        public Data data { get; set; }
        public string message { get; set; }
    }

    public class Datum
    {
        public string name { get; set; }
        public string print_tag { get; set; }
        public string rarity { get; set; }
        public PriceData price_data { get; set; }
    }

    public class YuGiOhPriceSerializer
    {
        public List<Datum> data { get; set; }
    }
}
