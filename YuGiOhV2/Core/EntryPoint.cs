using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace YuGiOhV2.Core
{
    public class EntryPoint
    {

        //private static CancellationTokenSource TkSource;

        private static async Task Main(string[] args)
        {

            AltConsole.Initialize();
            AltConsole.Write("Info", "Entry Point", "Welcome to YuGiOh Bot V2");
            AltConsole.Write("Info", "Entry Point", $"Using Discord.NET v{DiscordConfig.Version}");

            try
            {

                var events = new Events();

                await events.GetReadyForBlastOff();

            }catch(Exception e)
            {

                await File.WriteAllTextAsync("Error.txt", $"{e.Message}\n{e.StackTrace}");

            }

            await Task.Delay(-1);

        }

    }
}
