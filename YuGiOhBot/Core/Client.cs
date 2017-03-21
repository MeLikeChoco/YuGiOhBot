using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using YuGiOhBot.Services;

namespace YuGiOhBot.Core
{
    public class Client
    {

        public static void Main(string[] args)
            => new Client().Run().GetAwaiter().GetResult();

        private DiscordSocketClient _yugiohBot;
        private CommandService _commandService;
        private DependencyMap _map;
        private YuGiOhServices _yugiohService;
        private int _latencyMessageLimiter = 10; //pls no spam console
        private const string DiscordTokenPath = "Tokens/Discord.txt";

        public async Task Run()
        {

            _yugiohBot = new DiscordSocketClient(new DiscordSocketConfig()
            {
                
                WebSocketProvider = WS4NetCore.WS4NetProvider.Instance,
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 1000

            });

            _commandService = new CommandService(new CommandServiceConfig()
            {

                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Verbose,

            });

            //await AltConsole.PrintAsync("Info", "YuGiOh", "Populating hexcode dictionary...");
            //_yugiohService = new YuGiOhServices();
            //await _yugiohService.InitializeService();
            //await AltConsole.PrintAsync("Info", "YuGiOh", "Population of hexcode dictionary finished.");

            _map = new DependencyMap();
            _yugiohService = new YuGiOhServices();
            await AltConsole.PrintAsync("Service", "YuGiOh", "Populating hexcode list...");
            await _yugiohService.InitializeService();
            await AltConsole.PrintAsync("Service", "YuGiOh", "Hexcode list populated.");

            _map.Add(_yugiohBot);
            _map.Add(_yugiohService);

            Log();

            await AltConsole.PrintAsync("Info", "Commands", "Registering commands...");
            await RegisterCommands();
            await AltConsole.PrintAsync("Info", "Commands", "Commands registered.");

            await LoginAndConnect();
            SetGame();
            ForceReconnect();

            await Task.Delay(-1);

        }

        private void ForceReconnect()
        {

            _yugiohBot.Disconnected += async (ex) =>
            {

                await Task.Delay(10000); //wait 10 seconds before force reconnect

                if (!_yugiohBot.ConnectionState.Equals(ConnectionState.Connected))
                {

                    await AltConsole.PrintAsync("Severe", "Client", "Reconnecting...");
                    await Run();

                }

            };

        }

        private void SetGame()
        {

            _yugiohBot.Ready += async () =>
            {

                await AltConsole.PrintAsync("Info", "Client", "Setting game...");
                var message = File.ReadAllText("Tokens/GameMessage.txt");
                await _yugiohBot.SetGameAsync(message);
                await AltConsole.PrintAsync("Info", "Client", "Game set.");

            };

        }

        private async Task RegisterCommands()
        {

            _yugiohBot.MessageReceived += CommandHandler;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());

        }

        public async Task CommandHandler(SocketMessage possibleCommand)
        {

            var message = possibleCommand as SocketUserMessage;

            if (message == null) return;

            var argPos = 0;

            if (!(message.HasStringPrefix("e$", ref argPos) || message.HasMentionPrefix(_yugiohBot.CurrentUser, ref argPos))) return;

            var context = new CommandContext(_yugiohBot, message);
            IResult result = await _commandService.ExecuteAsync(context, argPos, _map);

            if (!result.IsSuccess)
            {

                await context.Channel.SendMessageAsync("It seems you have encountered the shadow realm, command used wrong.");
                //await context.Channel.SendMessageAsync("https://goo.gl/JieFJM");

                await AltConsole.PrintAsync("Error", "Error", result.ErrorReason);

                //debug purposes
                //await context.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");

            }

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

        private async Task LoginAndConnect()
        {

            var token = File.ReadAllText(DiscordTokenPath);

            await AltConsole.PrintAsync("Info", "Client", "Logging in...");
            await _yugiohBot.LoginAsync(TokenType.Bot, token);
            await AltConsole.PrintAsync("Info", "Client", "Logged in.");
            await AltConsole.PrintAsync("Info", "Client", "Starting up...");
            await _yugiohBot.StartAsync();
            await AltConsole.PrintAsync("Info", "Client", "Finished starting up.");

        }
    }
}
