using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YuGiOhV2.Core
{
    public class EntryPoint
    {

        //private static CancellationTokenSource TkSource;

        private static async Task Main(string[] args)
        {

            AltConsole.Initialize();
            AltConsole.Write("Info", "Entry Point", "Welcome to YuGiOh Bot V2");

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
