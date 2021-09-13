using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Models.Services
{
    public class GetValidBoosterPacks : PredefinedService
    {

        private readonly Cache _cache;

        public GetValidBoosterPacks(Cache cache)
            : base("GetValidBoosterPacks", "GetValidBoosterPacks", "GetValidBoosterPacks")
        {
            
            _cache = cache;

        }

        public override async void Execute(object state)
        {

            Log($"Starting service \"{Name}\"...");

            using (var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.In))
            using (var process = new Process()
            {

                StartInfo = new ProcessStartInfo(DotNetRuntime)
                {

                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = $"{ExecutablePath} -pipe {_pipeName}"

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

    }
}
