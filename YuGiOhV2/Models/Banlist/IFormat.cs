using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Banlist
{
    public interface IFormat
    {

        IEnumerable<string> Forbidden { get; set; }
        IEnumerable<string> Limited { get; set; }
        IEnumerable<string> SemiLimited { get; set; }

    }
}
