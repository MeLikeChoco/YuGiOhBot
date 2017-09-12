using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Services;

namespace YuGiOhV2.Core
{
    public class Events
    {

        private DiscordSocketClient _client;
        private CommandService _commands;
        private Database _database;
        private IServiceProvider _services;

        private static readonly DiscordSocketConfig ClientConfig = new DiscordSocketConfig()
        {

            AlwaysDownloadUsers = true,
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 1000

        };

        private static readonly CommandServiceConfig CommandConfig = new CommandServiceConfig()
        {

            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose

        };

        public Events()
        {

            Print("Initializing events...");

            _client = new DiscordSocketClient(ClientConfig);
            _commands = new CommandService(CommandConfig);
            _services = new ServiceCollection()
                .AddSingleton<Cache>()
                .BuildServiceProvider();

            RegisterLogging();

            Print("Finished initializing events.");

        }

        public async Task GetReadyForBlastOff()
        {

            await RevEngines();
            await LoadDatabase();
            //await RegisterCommands();

        }        

        private async Task RevEngines()
        {

            var isTest = Environment.GetCommandLineArgs().FirstOrDefault()?.ToLower();
            string token;

            if (isTest == "true")
                token = File.ReadAllText("Files/Bot Tokens/Test.txt");
            else
                token = File.ReadAllText("Files/Bot Tokens/Test.txt");

            Print("Logging in...");
            await _client.LoginAsync(TokenType.Bot, token);
            Print("Logged in.");
            Print("Starting client...");
            await _client.StartAsync();
            Print("ITS UP AND RUNNING BOIIIIIIIIIIIIIIS");

        }

        private async Task RegisterCommands()
        {

            Print("Registering commands...");

            _client.MessageReceived += HandleCommand;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            Print("Commands registered.");

        }

        private async Task LoadDatabase()
        {

            Print("Waiting for guilds to load...");
            await Task.Delay(10000);
            Print("Guilds loaded.");

            Print("Loading database...");
            _database = new Database(_client.Guilds);
            Print("Finished loading database.");

        }

        private async Task HandleCommand(SocketMessage possibleCmd)
        {



        }

        private void RegisterLogging()
        {

            _client.Log += (message)
                => Task.Run(() 
                => AltConsole.Print(message.Severity.ToString(), message.Source, message.Message, message.Exception));
            _commands.Log += (message)
                => Task.Run(()
                => AltConsole.Print(message.Severity.ToString(), message.Source, message.Message, message.Exception));

        }

        private void Print(string message)
            => AltConsole.Print("Info", "Events", message);

    }
}
