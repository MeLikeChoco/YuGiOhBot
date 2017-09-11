using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public class Card
    {

        public string Name { get; set; }
        public string RealName { get; set; }
        public string CardType { get; set; }
        public string Lore { get; set; }
        public bool HasEffect { get; set; }
        public string Archetype { get; set; }
        public int OcgOnly { get; set; }
        public int TcgOnly { get; set; }
        public string ImageUrl { get; set; }

    }
}
