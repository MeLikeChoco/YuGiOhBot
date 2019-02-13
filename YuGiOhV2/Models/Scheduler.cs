using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YuGiOhV2.Models
{
    public class Scheduler
    {

        private readonly Timer _scheduler;

        public Scheduler(Service service)
        {

            Log($"Registering {service.Name}...");

            _scheduler = new Timer(service.Execute, null, service.Delay, service.Period);

            Log($"Finished registering {service.Name}.");

        }

        private void Log(string message)
            => AltConsole.Write("Scheduler", "Scheduler", message);

    }
}
