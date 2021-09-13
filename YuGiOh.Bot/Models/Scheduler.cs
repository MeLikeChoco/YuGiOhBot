using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOh.Bot.Models.Services;

namespace YuGiOh.Bot.Models
{
    public class Scheduler
    {

        private readonly Timer _scheduler;

        public Scheduler(Service service)
        {

            Log($"Registering {service.Name}...");

            try
            {
                _scheduler = new Timer(service.Execute, null, service.Delay(), service.Period());
            }catch(Exception exception)
            {
                AltConsole.Write("Scheduler", service.Name, "There was a problem registering a service module.", exception);
            }

            Log($"Finished registering {service.Name}.");

        }

        private void Log(string message)
            => AltConsole.Write("Scheduler", "Scheduler", message);

    }
}
