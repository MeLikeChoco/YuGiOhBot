using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Services;

namespace YuGiOhV2.Models.Services
{
    public class GetValidBoosterPacks : Service
    {

        private readonly Cache _cache;

        public GetValidBoosterPacks(Cache cache)
        {

            Name = "GetValidBoosterPacks";
            ServiceDirectory = $"Services/Predefined/{Name}/";
            ExecutablePath = $"{ServiceDirectory}{Name}.dll";
            SettingsPath = $"{ServiceDirectory}{Name}.json";

            _cache = cache;

        }

        public override async void Execute(object state)
        {

            Log($"Starting service \"{Name}\"...");

            using (var pipe = new NamedPipeClientStream(".", "GetValidBoosterPacks.Pipe", PipeDirection.In))
            using (var process = new Process()
            {

                StartInfo = new ProcessStartInfo(DotNetRuntime)
                {

                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = $"{ExecutablePath}"

                }

            })
            {

                process.Start();
                await pipe.ConnectAsync();
                Log($"Connected to \"{Name}\".");

                _cache.ValidBoosterPacks = Serializer.Deserialize<string[]>(pipe);

                Log($"Finished getting info from \"{Name}\".");
                process.WaitForExit();
                pipe.Close();

            }

            Log($"Finished service \"{Name}\".");

        }

        private void Log(string message)
            => AltConsole.Write("Service", "Get Valid Booster Packs", message);

    }
}
