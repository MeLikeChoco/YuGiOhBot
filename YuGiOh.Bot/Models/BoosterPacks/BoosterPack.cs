using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Models.BoosterPacks
{
    public record BoosterPack
    {
        
        public string Name { get; set; }
        public List<BoosterPackDate> Dates { get; set; }
        public List<BoosterPackCard> Cards { get; set; }
        public string Url { get; set; }
        public bool TcgExists { get; set; }
        public bool OcgEcists { get; set; }

        // public string Name { get; set; }
        // public Dictionary<string, DateTime> ReleaseDates { get; }
        // public string[] Commons { get; }
        // public string[] Rares { get; }
        // public Dictionary<string, string[]> Foils { get; }
        // public Dictionary<string, string[]> RarityToCards { get; set; }
        // public string[] Cards { get; }
        // public string Url { get; }
        //
        // public BoosterPack() { }
        //
        // public BoosterPack(
        //     string name,
        //     Dictionary<string, DateTime> dates,
        //     Dictionary<string, List<string>> cards,
        //     string url
        // )
        // {
        //
        //     Name = name;
        //     ReleaseDates = dates;
        //     Url = url;
        //
        //     if (cards.TryGetValue("common", out var commons))
        //         Commons = cards["common"].ToArray();
        //     else
        //         Commons = new string[0];
        //
        //     if (cards.TryGetValue("rare", out var rares))
        //         Rares = cards["rare"].ToArray();
        //     else
        //         Rares = new string[0];
        //
        //     Foils = cards.Where(kv => kv.Key != "Common" && kv.Key != "Rare").ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
        //     RarityToCards = cards.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
        //     Cards = cards.SelectMany(kv => kv.Value).Distinct(StringComparer.InvariantCulture).ToArray();
        //
        // }

    }

    public class BoosterPackDate
    {
        
        public string Name { get; set; }
        public DateTime Date { get; set; }
        
    }

    public class BoosterPackCard
    {
        
        public string Name { get; set; }
        public List<string> Rarities { get; set; }
        
    }
}