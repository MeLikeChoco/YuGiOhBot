using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Cards
{
    public class XyzPendulum : Xyz, IHasRank, IHasScale
    {
        
        public int PendulumScale { get; set; }

    }
}
