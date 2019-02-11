using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Cards
{
    public class SynchroAndFusionPendulum : RegularMonster, IHasScale, IHasMaterials
    {

        public int PendulumScale { get; set; }
        public string Materials { get; set; }

    }
}
