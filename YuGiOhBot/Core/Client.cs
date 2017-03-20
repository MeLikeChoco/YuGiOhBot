using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Core
{
    public class Client
    {

        public static void main(string[] args)
            => new Client().Run().GetAwaiter().GetResult();

        private DiscordSocketClient _yugiohBot;
        private CommandService _commandService;
        private DependencyMap _map;
        private int _latencyMessageLimiter = 10; //pls no spam console

        public async Task Run()
        {

            _yugiohBot = new DiscordSocketClient(new DiscordSocketConfig()
            {

                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 1000

            });

            _commandService = new CommandService(new CommandServiceConfig()
            {

                CaseSensitiveCommands = false

            });

            //await AltConsole.PrintAsync("Info", "YuGiOh", "Populating hexcode dictionary...");
            //_yugiohService = new YuGiOhServices();
            //await _yugiohService.InitializeService();
            //await AltConsole.PrintAsync("Info", "YuGiOh", "Population of hexcode dictionary finished.");

            _map = new DependencyMap();

            _map.Add(_yugiohBot);

            Log();

        }

        private void Log()
        {

            _yugiohBot.Log += async (message) =>
            {

                if (string.IsNullOrEmpty(message.Message)) return;

                if (message.Message.Contains("Latency"))
                {


                    if (_latencyMessageLimiter == 10)
                    {

                        _latencyMessageLimiter = 0;

                    }
                    else
                    {

                        //debug purposes
                        //Console.ForegroundColor = ConsoleColor.White;
                        //Console.WriteLine("blocked");
                        _latencyMessageLimiter++;
                        return;

                    }

                }

                await AltConsole.PrintAsync(message.Severity.ToString(), message.Source, message.Message, message.Exception);

            };

        }
    }
}
