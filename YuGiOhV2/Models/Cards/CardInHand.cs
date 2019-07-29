using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Cards
{
    public class CardInHand
    {

        public string Name { get; set; }
        public int InDeck { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

    }
}
