using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2
{
    public static class AltConsole
    {

        public static void Print(string firstBracket, string secondBracket, string message, Exception exception = null)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{DateTime.Now.ToString()} ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[{firstBracket}]");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{secondBracket}] ");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (exception == null)
            {

                Console.WriteLine($"{message}");

            }
            else
            {

                Console.WriteLine($"{message}\t\t{exception}");

            }

        }

        public static void InlinePrint(string firstBracket, string secondBracket, string message, Exception exception = null)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{DateTime.Now.ToString()} ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[{firstBracket}]");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{secondBracket}] ");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (exception == null)
            {

                Console.Write($"{message}\r");

            }
            else
            {

                Console.WriteLine($"{message}\t\t{exception}");

            }

        }

    }
}
