using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Observer
{
    public static class Program
    {

        private static Process _botProcess;

        public static async Task Main(string[] _)
        {
            CreateProcess(null, null);
            await Task.Delay(-1);
        }

        private static void CreateProcess(object _, EventArgs _e)
        {

            Console.WriteLine("Starting bot...");

            _botProcess = new Process
            {

                StartInfo = new ProcessStartInfo
                {

                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = "dotnet",
                    Arguments = $"{File.ReadAllText("file.txt")}"

                },
                EnableRaisingEvents = true

            };

            _botProcess.OutputDataReceived += RedirectOutput;
            _botProcess.ErrorDataReceived += RedirectOutput;
            _botProcess.Exited += async (object _, EventArgs _) =>
            {

                Console.WriteLine("Bot exited. Waiting 10 seconds for bot restart...");

                _botProcess.OutputDataReceived -= RedirectOutput;
                _botProcess.ErrorDataReceived -= RedirectOutput;

                await Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ => CreateProcess(null, null));

            };

            _botProcess.Start();
            _botProcess.BeginOutputReadLine();
            _botProcess.BeginErrorReadLine();

        }

        private static void RedirectOutput(object _, DataReceivedEventArgs args)
            => Console.WriteLine(args.Data);

    }
}
