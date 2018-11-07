using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public class RegularMonster : Monster, IHasLevel, IHasAtk, IHasDef
    {

        public int Level { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }

    }
}
