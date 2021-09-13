using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Models.Services
{
    public class PredefinedService : Service
    {

        public PredefinedService(string name, string folderName, string serviceName)
        {

            Name = name;
            FileName = serviceName;
            ServiceTypePath = PredefinedServicePath;
            ServiceDirectory = folderName;
            ExecutablePath = $"{Name}.dll";
            SettingsPath = $"{Name}.json";

        }

    }
}
