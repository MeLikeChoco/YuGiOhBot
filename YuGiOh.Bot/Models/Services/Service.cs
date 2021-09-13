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

namespace YuGiOh.Bot.Models.Services
{
    public class Service
    {

        public const string PredefinedServicePath = "Services/Predefined/";
        public const string RunnableServicePath = "Services/Runnables/";
                
        ////public string Name { get => _name; set => _name = GetFullPath(value); }
        protected string _serviceDirectory;
        ////public string ServiceDirectory { get => _serviceDirectory; set =>_serviceDirectory = GetFullPath(value); }
        protected string _executablePath;
        ////public string ExecutablePath { get => _executablePath; set => _executablePath = GetFullPath(value); }
        protected string _settingsPath;
        ////public string SettingsPath { get => _settingsPath; set => _settingsPath = GetFullPath(value); }

        public string Name { get; set; }
        public string FileName { get; set; }
        protected string ServiceTypePath { get; set; }
        public string ServiceDirectory { get => Path.Combine(ServiceTypePath, _serviceDirectory); set => _serviceDirectory = value; }
        public string ExecutablePath { get => Path.Combine(ServiceDirectory, _executablePath); set => _executablePath = value; }
        public string SettingsPath { get => Path.Combine(ServiceDirectory, _settingsPath); set => _settingsPath = value; }
        public string Executable { get => $"{FileName}.dll"; }

        protected JObject _settings;
        public JObject Settings
        {

            get
            {

                if (_settings == null)
                    _settings = JObject.Parse(File.ReadAllText(SettingsPath));

                return _settings;

            }

        }

        public virtual TimeSpan Delay() => TimeSpan.ParseExact(Settings.Value<string>("Delay"), @"d\:hh\:mm\:ss", new CultureInfo("en-US"));
        public virtual TimeSpan Period() => TimeSpan.ParseExact(Settings.Value<string>("Period"), @"d\:hh\:mm\:ss", new CultureInfo("en-US"));

        protected Timer RunService;

        protected string _pipeName => Settings["PipeName"]?.ToObject<string>();

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
            RunService = new Timer(Execute, null, Delay(), Period());

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

        protected void Log(string message)
            => AltConsole.Write("Service", Name, message);

    }
}
