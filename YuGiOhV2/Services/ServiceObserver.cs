using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Models;
using YuGiOhV2.Models.Services;

namespace YuGiOhV2.Services
{
    public class ServiceObserver
    {

        public string[] Services => Directory.GetDirectories(ServicesDirectory).SelectMany(Directory.GetDirectories).Where(ContainsService).ToArray();
        public string[] Runnables => Directory.GetDirectories(Path.Combine(ServicesDirectory, "Runnables")).Where(ContainsService).ToArray();
        public string[] Predefined => Directory.GetDirectories(Path.Combine(ServicesDirectory, "Predefined")).Where(ContainsService).ToArray();

        private const string ServicesDirectory = "Services";

        private string[] _services => Directory.GetDirectories(ServicesDirectory);
        private readonly Timer _checkRunnables;
        private readonly Dictionary<string, Scheduler> _predefined, _runnables;

        public ServiceObserver()
        {

            _predefined = new Dictionary<string, Scheduler>();
            _runnables = new Dictionary<string, Scheduler>();
            _checkRunnables = new Timer(CheckRunnables, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));

        }

        public ServiceObserver AddPredefined(Service service)
        {

            _predefined[service.Name] = new Scheduler(service);

            return this;

        }

        public ServiceObserver AddRunnable(Service service)
        {

            _runnables[service.Name] = new Scheduler(service);

            return this;

        }

        private void CheckRunnables(object state)
        {

            var newRunnables = Runnables.Except(_predefined.Keys);

            foreach (var directory in newRunnables)
            {

                var service = new Service(Path.GetFileName(directory), directory, GetServiceExecutable(directory), GetExecutableSettings(directory));

                AddRunnable(service);

            }

        }

        private bool ContainsService(string directory)
        {

            var nameExt = Directory.GetFiles(directory).FirstOrDefault(file => Path.GetExtension(file) == "pdb");

            if (string.IsNullOrEmpty(nameExt))
                return false;

            var name = Path.GetFileNameWithoutExtension(nameExt);
            var nameDirectory = Path.GetDirectoryName(nameExt);

            return File.Exists(Path.Combine(nameDirectory, $"{name}.dll"));

        }

        private string GetServiceExecutable(string directory)
        {

            var pdbFile = Directory.GetFiles(directory).FirstOrDefault(file => Path.GetExtension(file) == "pdb");
            var name = Path.GetFileNameWithoutExtension(pdbFile);

            return Path.Combine(directory, $"{name}.dll");

        }

        private string GetExecutableSettings(string directory)
        {

            var pdbFile = Directory.GetFiles(directory).FirstOrDefault(file => Path.GetExtension(file) == "pdb");
            var name = Path.GetFileNameWithoutExtension(pdbFile);

            return Path.Combine(directory, $"{name}.json");

        }

    }
}
