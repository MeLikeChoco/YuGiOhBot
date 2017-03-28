using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Core
{
    public static class AltConsole
    {

        //if anything is uncessarily async, this would be it
        public static async Task PrintAsync(string firstBracket, string secondBracket, string message, Exception exception = null)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            await Console.Out.WriteAsync($"{DateTime.Now.ToString()} ");
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Out.WriteAsync($"[{firstBracket}]");
            Console.ForegroundColor = ConsoleColor.Green;
            await Console.Out.WriteAsync($"[{secondBracket}] ");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (exception == null)
            {

                await Console.Out.WriteLineAsync($"{message}");

            }
            else
            {

                await Console.Out.WriteLineAsync($"{message}\t\t{exception}");

            }

        }

    }
}
