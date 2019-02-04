using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.BoosterPacks
{
    public class BoosterPack
    {

        public string Name { get; }
        public Dictionary<string, DateTime> ReleaseDates { get; }
        public Dictionary<string, string[]> RarityToCards { get; }
        public string[] Cards { get; }
        public string Url { get; }

        public BoosterPack(string name, Dictionary<string, DateTime> dates, Dictionary<string, List<string>> cards, string url)
        {

            Name = name;
            ReleaseDates = dates;
            RarityToCards = cards.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray(), StringComparer.InvariantCultureIgnoreCase);
            Cards = RarityToCards.SelectMany(kv => kv.Value).ToArray();
            Url = url;

        }

    }
}
