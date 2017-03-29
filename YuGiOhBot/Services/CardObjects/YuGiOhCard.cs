using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Services.CardObjects
{
    public class YuGiOhCard
    {

        public string Name { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string Lore { get; set; } = string.Empty;
        public bool HasEffect { get; set; }
        public string Archetype { get; set; } = string.Empty;
        public string OcgStatus { get; set; } = string.Empty;
        public string TcgStatus { get; set; } = string.Empty;
        public string TcgTrnStatus { get; set; } = string.Empty;
        public bool OcgOnly { get; set; }
        public bool TcgOnly { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public YuGiOhPriceSerializer Prices { get; set; }
        
    }
}
