using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YuGiOhV2.Models
{
    public class Service
    {

        //private string _name;
        //public string Name { get => _name; set => _name = GetFullPath(value); }
        //private string _serviceDirectory;
        //public string ServiceDirectory { get => _serviceDirectory; set =>_serviceDirectory = GetFullPath(value); }
        //private string _executablePath;
        //public string ExecutablePath { get => _executablePath; set => _executablePath = GetFullPath(value); }
        //private string _settingsPath;
        //public string SettingsPath { get => _settingsPath; set => _settingsPath = GetFullPath(value); }
        
        public string Name { get; set; }
        public string ServiceDirectory { get; set; }
        public string ExecutablePath { get; set; }
        public string SettingsPath { get; set; }

        public JObject Settings => JObject.Parse(File.ReadAllText(SettingsPath));
        public TimeSpan Delay => TimeSpan.ParseExact(Settings.Value<string>("Delay"), @"d\:hh\:mm\:ss", new CultureInfo("en-US"));
        public TimeSpan Period => TimeSpan.ParseExact(Settings.Value<string>("Period"), @"d\:hh\:mm\:ss", new CultureInfo("en-US"));

        protected Timer RunService;

        protected string DotNetRuntime
        {

            get
            {

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return Path.GetFullPath("/usr/share/dotnet/dotnet");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");

                else return null;

            }

        }

        public Service(string name, string directory, string executablePath, string settingsPath)
        {

            Name = name;
            ServiceDirectory = directory;
            ExecutablePath = executablePath;
            SettingsPath = settingsPath;
            RunService = new Timer(Execute, null, Delay, Period);

        }

        public Service() { }

        public virtual void Execute(object state)
        {

            var process = new Process()
            {

                StartInfo = new ProcessStartInfo(DotNetRuntime)
                {

                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = ServiceDirectory,
                    Arguments = ExecutablePath

                }

            };

            process.Start();
            process.WaitForExit();

        }

        private string GetFullPath(string path)
            => Path.Combine(Directory.GetCurrentDirectory(), path);

    }
}
