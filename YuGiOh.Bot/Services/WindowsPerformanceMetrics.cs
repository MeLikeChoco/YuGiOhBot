using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Interfaces;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Services;

public class WindowsPerformanceMetrics : IPerformanceMetrics
{

    public Task<string> GetOperatingSystem()
        => Task.FromResult(RuntimeInformation.OSDescription);

    public async Task<float> GetCpuUsage()
    {

        var procStartInfo = new ProcessStartInfo
        {

            FileName = "wmic",
            Arguments = "cpu get loadpercentage /value",
            RedirectStandardOutput = true

        };

        string output;

        using (var proc = Process.Start(procStartInfo))
            output = await proc?.StandardOutput?.ReadToEndAsync();

        if (output is null)
            return float.NaN;

        output = output.Trim().Split('=')[1];

        return float.Parse(output);

    }

    public async Task<IMemoryMetrics> GetMemUsage()
    {

        var procStartInfo = new ProcessStartInfo
        {

            FileName = "wmic",
            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /value",
            RedirectStandardOutput = true

        };

        string output;

        using (var proc = Process.Start(procStartInfo))
            output = await proc?.StandardOutput?.ReadToEndAsync();

        if (output is null)
            return null;

        output = output.Trim();
        var entries = output.Split('\n');
        var totalMem = double.Parse(entries[1].Split('=')[1]) / 1024 / 1024;

        return new MemoryMetrics
        {

            TotalMem = totalMem,
            UsedMem = totalMem - (double.Parse(entries[0].Split('=')[1]) / 1024 / 1024)

        };

    }

}