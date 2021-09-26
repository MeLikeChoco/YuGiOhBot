using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Models.Interfaces;

namespace YuGiOh.Bot.Services.Interfaces
{
    public interface IPerformanceMetrics
    {

        Task<string> GetOperatingSystem();
        Task<float> GetCpuUsage();
        Task<IMemoryMetrics> GetMemUsage();

    }
}
