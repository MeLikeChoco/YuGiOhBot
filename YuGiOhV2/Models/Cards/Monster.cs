using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Cards
{
    public class Monster : Card
    {

        public string Attribute { get; set; }
        public string[] Types { get; set; }

    }
}
