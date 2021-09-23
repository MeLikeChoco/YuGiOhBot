using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

Process botProc;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("Starting bot...");

CreateProc();

await Task.Delay(-1);

void CreateProc()
{

    botProc = new Process
    {

        StartInfo = new ProcessStartInfo
        {

            RedirectStandardError = true,
            RedirectStandardOutput = true,
            StandardOutputEncoding = Encoding.UTF8,
            UseShellExecute = false,
            FileName = "dotnet",
            Arguments = $"{File.ReadAllText("file.txt")}"

        },
        EnableRaisingEvents = true

    };

    botProc.OutputDataReceived += RedirectOutput;
    botProc.ErrorDataReceived += RedirectOutput;
    botProc.Exited += (object _, EventArgs _) =>
    {

        Console.WriteLine("Bot exited. Waiting 10 seconds for bot restart...");

        botProc.OutputDataReceived -= RedirectOutput;
        botProc.ErrorDataReceived -= RedirectOutput;

        Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ => CreateProc());

    };

    botProc.Start();
    botProc.BeginOutputReadLine();
    botProc.BeginErrorReadLine();

}

static void RedirectOutput(object _, DataReceivedEventArgs args)
    => Console.WriteLine(args.Data);