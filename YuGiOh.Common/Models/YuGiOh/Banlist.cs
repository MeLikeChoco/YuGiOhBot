using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Common.Models.YuGiOh
{
    public class Banlist
    {

        public IEnumerable<string> Forbidden { get; set; }
        public IEnumerable<string> Limited { get; set; }
        public IEnumerable<string> SemiLimited { get; set; }

    }
}
