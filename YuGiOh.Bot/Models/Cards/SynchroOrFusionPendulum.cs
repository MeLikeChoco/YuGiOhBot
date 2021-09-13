using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Models.Cards
{
    public class SynchroOrFusionPendulum : PendulumMonster, IHasMaterials
    {

        public string Materials { get; set; }

    }
}
