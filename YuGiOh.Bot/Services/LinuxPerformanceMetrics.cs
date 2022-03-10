using System;
using System.Diagnostics;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Interfaces;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Services;

public class LinuxPerformanceMetrics : IPerformanceMetrics
{

    public async Task<string> GetOperatingSystem()
    {

        var procStartInfo = new ProcessStartInfo
        {

            FileName = "/bin/bash",
            Arguments = "-c \"cat /etc/os-release\"",
            RedirectStandardOutput = true

        };

        string output;

        using (var proc = Process.Start(procStartInfo))
            output = await proc?.StandardOutput?.ReadToEndAsync();

        if (output is null)
            return null;

        output = output.Trim();
        var entries = output.Split('\n');
        output = Array.Find(entries, entry => entry.StartsWith("PRETTY_NAME"))?.Split('=')[1].Trim('"');

        if (!string.IsNullOrEmpty(output))
            return output;

        var osName = entries[0].Split('=')[1].Trim('"');
        var osVer = entries[1].Split('=')[1].Trim('"');
        output = osName + " " + osVer;

        return output;

    }

    public async Task<float> GetCpuUsage()
    {

        var procStartInfo = new ProcessStartInfo
        {

            FileName = "/bin/bash",
            Arguments = $"-c \"cat {Constants.UnixCpuUsageCmdArgs}\"",
            RedirectStandardOutput = true

        };

        string output;

        using (var proc = Process.Start(procStartInfo))
            output = await proc?.StandardOutput?.ReadToEndAsync();

        if (output is null)
            return float.NaN;

        output = output.Trim();

        return float.Parse(output);

    }

    public async Task<IMemoryMetrics> GetMemUsage()
    {

        var procStartInfo = new ProcessStartInfo
        {

            FileName = "/bin/bash",
            Arguments = "-c \"free --mega\"",
            RedirectStandardOutput = true

        };

        string output;

        using (var proc = Process.Start(procStartInfo))
            output = await proc?.StandardOutput?.ReadToEndAsync();

        if (output is null)
            return null;

        output = output.Split('\n')[1];
        var entries = output.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return new MemoryMetrics
        {

            TotalMem = double.Parse(entries[1]) / 1024,
            UsedMem = double.Parse(entries[2]) / 1024

        };

    }

}