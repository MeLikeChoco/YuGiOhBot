using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace YuGiOh.Bot.Core
{
    public static class EntryPoint
    {

        //private static CancellationTokenSource TkSource;

        public static async Task Main()
        {

            AltConsole.Write("Info", "Entry Point", $"Welcome to {Assembly.GetExecutingAssembly().GetName()}");
            AltConsole.Write("Info", "Entry Point", $"Using Discord.NET v{DiscordConfig.Version}");

            try
            {

                var events = new Events();

                await events.Initialize();

            }
            catch (Exception e)
            {

                await File.WriteAllTextAsync("Error.txt", $"{e.Message}\n{e.StackTrace}");

            }

            await Task.Delay(-1);

        }

    }
}
