using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Cards
{
    public class Xyz : Monster, IHasRank, IHasMaterials, IHasAtk, IHasDef
    {

        public int Rank { get; set; }
        public string Materials { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }
    }
}
