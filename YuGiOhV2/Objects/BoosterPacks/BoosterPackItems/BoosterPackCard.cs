using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.BoosterPacks.BoosterPackItems
{
    public class BoosterPackCard
    {

        public string Name { get; set; }
        public string Rarity { get; set; }
        public string CardNumber { get; set; }

        public BoosterPackCard(string name, string rarity, string cardNumber)
        {

            Name = name;
            Rarity = rarity;
            CardNumber = cardNumber;

        }

        public BoosterPackCard(string name, string rarity)
            : this(name, rarity, null) { }

        public override string ToString()
            => Name + " / " + Rarity + " / " + CardNumber;

    }
}
