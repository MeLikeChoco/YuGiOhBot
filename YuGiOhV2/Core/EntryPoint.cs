using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YuGiOhV2.Core
{
    public class EntryPoint
    {

        //private static CancellationTokenSource TkSource;

        public static void Main(string[] args)
            => new EntryPoint().Run().GetAwaiter().GetResult();

        private async Task Run()
        {

            AltConsole.Print("Info", "Entry Point", "Welcome to YuGiOh Bot V2");

            var events = new Events();

            await events.GetReadyForBlastOff();

            await Task.Delay(-1);

        }

    }
}
