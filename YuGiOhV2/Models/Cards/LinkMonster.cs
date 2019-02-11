using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Cards
{
    public class LinkMonster : Monster, IHasLink, IHasAtk
    {
                
        public int Link { get; set; }
        public string[] LinkArrows { get; set; }
        public string Atk { get; set; }

    }
}
