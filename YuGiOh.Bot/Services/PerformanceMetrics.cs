using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MoreLinq;

namespace YuGiOh.Bot.Services
{
    public class PerformanceMetrics
    {

        public Task<string> GetOperatingSystem()
        {

            if (IsUnix())
                return GetOperatingSystemUnix();

            return Task.FromResult(RuntimeInformation.OSDescription);

        }

        public Task<float> GetCpuUsage()
        {

            if (IsUnix())
                return GetCpuUsageUnix();

            return GetCpuUsageWindows();

        }

        public Task<MemoryMetrics> GetMemUsage()
        {

            if (IsUnix())
                return GetMemUsageUnix();

            return GetMemUsageWindows();

        }

        private async Task<string> GetOperatingSystemUnix()
        {

            var procStartInfo = new ProcessStartInfo
            {

                FileName = "/bin/bash",
                Arguments = "-c \"cat /etc/os-release\"",
                RedirectStandardOutput = true

            };

            string output;

            using (var proc = Process.Start(procStartInfo))
                output = await proc.StandardOutput.ReadToEndAsync();

            output = output.Trim();
            var entries = output.Split('\n');
            output = Array.Find(entries, entry => entry.StartsWith("PRETTY_NAME"))?.Split('=')[1].Trim('"');

            if (string.IsNullOrEmpty(output))
            {

                var osName = entries[0].Split('=')[1].Trim('"');
                var osVer = entries[1].Split('=')[1].Trim('"');
                output = osName + " " + osVer;

            }

            return output;

        }

        private async Task<float> GetCpuUsageUnix()
        {

            var procStartInfo = new ProcessStartInfo
            {

                FileName = "/bin/bash",
                Arguments = $"-c \"cat {Constants.UnixCpuUsageCmdArgs}\"",
                RedirectStandardOutput = true

            };

            string output;

            using (var proc = Process.Start(procStartInfo))
                output = await proc.StandardOutput.ReadToEndAsync();

            output = output.Trim();

            return float.Parse(output);

        }

        private async Task<float> GetCpuUsageWindows()
        {

            var procStartInfo = new ProcessStartInfo
            {

                FileName = "wmic",
                Arguments = "cpu get loadpercentage /value",
                RedirectStandardOutput = true

            };

            string output;

            using (var proc = Process.Start(procStartInfo))
                output = await proc.StandardOutput.ReadToEndAsync();

            output = output.Trim().Split('=')[1];

            return float.Parse(output);

        }

        private async Task<MemoryMetrics> GetMemUsageUnix()
        {

            var procStartInfo = new ProcessStartInfo
            {

                FileName = "/bin/bash",
                Arguments = $"-c \"free --mega\"",
                RedirectStandardOutput = true

            };

            string output;

            using (var proc = Process.Start(procStartInfo))
                output = await proc.StandardOutput.ReadToEndAsync();

            output = output.Split('\n')[1];
            var entries = output.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return new MemoryMetrics
            {

                TotalMem = (int)Math.Round(double.Parse(entries[1]) / 1024),
                UsedMem = double.Parse(entries[2]) / 1024

            };

        }

        private async Task<MemoryMetrics> GetMemUsageWindows()
        {

            var procStartInfo = new ProcessStartInfo
            {

                FileName = "wmic",
                Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /value",
                RedirectStandardOutput = true

            };

            string output;

            using (var proc = Process.Start(procStartInfo))
                output = await proc.StandardOutput.ReadToEndAsync();

            output = output.Trim();
            var entries = output.Split('\n');
            var totalMem = (int)Math.Round(double.Parse(entries[1].Split('=')[1]) / 1024 / 1024);

            return new MemoryMetrics
            {

                TotalMem = totalMem,
                UsedMem = totalMem - (double.Parse(entries[0].Split('=')[1]) / 1024 / 1024)

            };

        }

        private bool IsUnix()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    }

    public class MemoryMetrics
    {

        public int TotalMem { get; set; }
        public double UsedMem { get; set; }

    }
}
