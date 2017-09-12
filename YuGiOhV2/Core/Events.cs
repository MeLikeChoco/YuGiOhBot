using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        private IServiceProvider _services;

        private static readonly DiscordSocketConfig ClientConfig = new DiscordSocketConfig()
        {

            AlwaysDownloadUsers = true,
            ConnectionTimeout = 10000,
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

            Print("Registering events...");

            _client = new DiscordSocketClient(ClientConfig);
            _commands = new CommandService(CommandConfig);
            _services = new ServiceCollection()
                .AddSingleton<Cache>()
                .BuildServiceProvider();

            RegisterLogging();
            RegisterCommands();

            Print("Finished registering events.");

        }

        private void RegisterCommands()
        {

            _client.MessageReceived += HandleCommand;
            _commands.AddModulesAsync(Assembly.GetEntryAssembly()).Wait(); //this will 99.9999999% likely succeed

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

        public void Print(string message)
            => AltConsole.Print("Info", "Events", message);

    }
}
