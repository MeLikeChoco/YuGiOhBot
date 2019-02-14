using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Services
{
    public class PredefinedService : Service
    {

        public PredefinedService(string name, string folderName, string serviceName)
        {

            Name = name;
            ServiceDirectory = $"Services/Predefined/{folderName}/";
            ExecutablePath = Path.Combine(ServiceDirectory, $"{Name}.dll");
            SettingsPath = Path.Combine(ServiceDirectory, $"{Name}.json");

        }

    }
}
