using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Models.Interfaces
{
    public interface IMemoryMetrics
    {

        double TotalMem { get; }
        double UsedMem { get; }

    }
}
