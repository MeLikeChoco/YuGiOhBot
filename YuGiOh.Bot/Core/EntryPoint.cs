using System;
using System.IO;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Core
{
    public static class EntryPoint
    {

        //private static CancellationTokenSource TkSource;

        public static async Task Main()
        {

            try
            {

                var events = new Events();

                await events.RunAsync();

            }
            catch (Exception e)
            {

                await File.WriteAllTextAsync("Error.txt", $"{e.Message}\n{e.StackTrace}");

            }

            await Task.Delay(-1);
            Console.ReadKey();

        }

    }
}