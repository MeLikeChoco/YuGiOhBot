using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public class XyzPendulum : Monster, IHasRank, IHasScale, IHasMaterials
    {

        public int Rank { get; set; }
        public int PendulumScale { get; set; }
        public string Materials { get; set; }

    }
}
