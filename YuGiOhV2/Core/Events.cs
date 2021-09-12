using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Interfaces;
using YuGiOhV2.Models;
using YuGiOhV2.Models.Services;
using YuGiOhV2.Services;

namespace YuGiOhV2.Core
{
    public class Events
    {

        private DiscordShardedClient _client;
        private CommandService _commands;
        private Services.Database _database;
        private Stats _stats;
        private Chat _chat;
        private readonly Cache _cache;
        private readonly Web _web;
        private readonly Config _config;
        //private readonly ServiceObserver _serviceObserver;
        private readonly InteractiveService _interactive;
        private IServiceProvider _services;
        private YuGiOhScraper _yugiohScraper;
        private int _recommendedShards, _currentShards;
        private bool _isInitialized;
        private readonly ConcurrentDictionary<DiscordSocketClient, Timer> _reconnectTimers;
        //private readonly List<Reconnector<DiscordSocketClient>> _reconnectors;

        private static DiscordSocketConfig _clientConfig;

        private DiscordSocketConfig ClientConfig
        {

            get
            {

                if (_clientConfig == null)
                {

                    _clientConfig = new DiscordSocketConfig()
                    {

                        //AlwaysDownloadUsers = true,
                        ConnectionTimeout = 60000, //had to include this as my bot got bigger and there were more guilds to connect to per shard
                        LogLevel = LogSeverity.Verbose,
                        MessageCacheSize = 1000,
                        TotalShards = 1,

                    };

                    //if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                    //    _clientConfigBacking.WebSocketProvider = WS4NetProvider.Instance;

                }

                return _clientConfig;

            }

        }

        private static readonly CommandServiceConfig CommandConfig = new()
        {

            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose

        };

        public Events()
        {

            Print("Initializing events...");

            try
            {
                _client = new DiscordShardedClient(ClientConfig);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Environment.Exit(0);
            }

            TunnelIn().GetAwaiter().GetResult();

            _recommendedShards = _client.GetRecommendedShardCountAsync().Result;

            _client.LogoutAsync();

            Print($"Launching with {_recommendedShards} shards...");

            ClientConfig.TotalShards = _recommendedShards;
            _client = new DiscordShardedClient(ClientConfig);
            _commands = new CommandService(CommandConfig);
            _web = new Web();
            _cache = new Cache();
            _interactive = new InteractiveService(_client);
            _config = Config.Instance;
            _reconnectTimers = new ConcurrentDictionary<DiscordSocketClient, Timer>();
            //_reconnectors = new List<Reconnector<DiscordSocketClient>>();
            //_serviceObserver = new ServiceObserver();

            RegisterLogging();

            Print("Finished initializing events.");

        }

        public async Task GetReadyForBlastOff()
        {

            await RevEngines();

            if (!_isInitialized)
            {

                _client.ShardReady += ReadyOtherStuff;
                _client.ShardDisconnected += Whoopsies;

            }

        }

        private async Task ReadyOtherStuff(DiscordSocketClient _)
        {

            if (Interlocked.Increment(ref _currentShards) == _recommendedShards)
            {

                var youAintDoneYet = YouAintDoneYet();

                _client.ShardReady -= ReadyOtherStuff;
                _client.ShardDisconnected -= Whoopsies;
                _currentShards = 0;

                //foreach (var client in _client.Shards)
                //    _reconnectors.Add(new Reconnector<DiscordSocketClient>(client));

                await youAintDoneYet;

            }

        }

        private Task GetBackToWork(Exception exception, DiscordSocketClient shard)
        {

            Print($"Shard {shard.ShardId} disconnected. Started reconnection timer.");

            shard.Ready += () => Task.Run(() =>
            {

                _reconnectTimers.Remove(shard, out var reconnectTimer);

                reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                reconnectTimer.Dispose();

                Print($"Shard {shard.ShardId} reconnected. Reconnection timer disposed.");

            });

            _reconnectTimers.TryAdd(shard, new Timer(async (state) =>
            {

                var shard = state as DiscordSocketClient;

                if (_reconnectTimers.ContainsKey(shard) && (shard.ConnectionState == ConnectionState.Disconnected || shard.ConnectionState == ConnectionState.Disconnecting))
                {

                    Print($"Shard {shard.ShardId} failed to reconnect. Reconnecting forcefully...");

                    await shard.StartAsync();

                }

            }, shard, TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(90)));

            return Task.CompletedTask;

        }

        private Task Whoopsies(Exception exception, DiscordSocketClient client)
        {

            Interlocked.Decrement(ref _currentShards);

            return Task.CompletedTask;

        }

        private async Task RevEngines()
        {

            await TunnelIn();
            Print("Starting client...");
            await _client.StartAsync();
            Print("ITS UP AND RUNNING BOIIIIIIIIIIIIIIS");

        }

        private async Task TunnelIn()
        {

            var isTest = Environment.GetCommandLineArgs().Contains("test");
            string token;

            if (isTest)
                token = File.ReadAllText("Files/Bot Tokens/Test.txt");
            else
                token = File.ReadAllText("Files/Bot Tokens/Legit.txt");

            Print($"Test: {isTest}");

            Print("Logging in...");
            await _client.LoginAsync(TokenType.Bot, token);
            Print("Logged in.");

        }

        private async Task YouAintDoneYet()
        {

            //await _cache.GetAWESOMECARDART(_web);
            await LoadDatabase();
            LoadStats();

            _yugiohScraper = new YuGiOhScraper(_client, _cache);

            BuildServices();
            await RegisterCommands();
            await _client.SetGameAsync($"Support guild/server: {_config.GuildInvite}");

            _isInitialized = true;

        }

        private async Task RegisterCommands()
        {

            Print("Registering commands...");

            _chat = new Chat(_cache, _database, new Web());

            _client.MessageReceived += HandleCommand;
            _client.MessageReceived += (message) =>
            {

                Task.Run(() => _chat.SOMEONEGETTINGACARDBOIS(message));

                return Task.CompletedTask;

            };

            _commands.AddTypeReader<string>(new StringInputTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Print("Commands registered.");

        }

        private async Task LoadDatabase()
        {

            Print("Loading database...");
            _database = new Services.Database();
            await _database.Initialize(_client.Guilds);
            Print("Finished loading database.");

            _client.JoinedGuild += _database.AddGuild;

        }

        private void LoadStats()
        {

            _stats = new Stats(_client, _web);

        }

        private void BuildServices()
        {

            _services = new ServiceCollection()
                .AddSingleton(Config.Instance)
                .AddTransient<IYuGiOhRepositoryConfiguration, RepoConfig>()
                .AddTransient<IYuGiOhDbService, YuGiOhDbService>()
                .AddSingleton(_client)
                .AddSingleton(_cache)
                .AddSingleton(_database)
                .AddSingleton(_interactive)
                .AddSingleton(_web)
                .AddSingleton(_stats)
                .AddSingleton(_config)
                .AddSingleton(_yugiohScraper)
                //.AddSingleton(_serviceObserver
                //.AddPredefined(new GetValidBoosterPacks(_cache))
                //.AddPredefined(_yugiohScraper))
                .AddSingleton<Random>()
                .BuildServiceProvider();

        }

        private async Task HandleCommand(SocketMessage message)
        {

            if (!(message is SocketUserMessage)
                || message.Author.IsBot
                || string.IsNullOrEmpty(message.Content))
                return;

            ulong id = 1;
            string prefix = "y!";

            if (!(message.Channel is SocketDMChannel))
            {

                id = (message.Channel as SocketTextChannel).Guild.Id;
                prefix = _database.Settings[id].Prefix;

            }

            var possibleCmd = message as SocketUserMessage;
            var argPos = 0;

            if ((possibleCmd.HasStringPrefix(prefix, ref argPos) || possibleCmd.HasMentionPrefix(_client.CurrentUser, ref argPos))
                && possibleCmd.Content.Trim() != prefix)
            {

                var context = new ShardedCommandContext(_client, possibleCmd);

                if (message.Channel is SocketDMChannel)
                    AltConsole.Write("Info", "Command", $"{possibleCmd.Author.Username} in DM's");
                else if (message.Channel is SocketTextChannel textChannel)
                    AltConsole.Write("Info", "Command", $"{possibleCmd.Author.Username} from {textChannel.Guild.Name}");

                AltConsole.Write("Info", "Command", $"{possibleCmd.Content}");

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {

                    if (result.ErrorReason.Contains("unknown command", StringComparison.OrdinalIgnoreCase))
                        return;
                    else if (result.ErrorReason.Contains("you are currently in timeout", StringComparison.OrdinalIgnoreCase))
                        await context.Channel.SendMessageAsync("Please wait 5 seconds between each type of paginator command!");

                    //await context.Channel.SendMessageAsync("https://goo.gl/JieFJM");

                    AltConsole.Write("Error", "Error", result.ErrorReason);
                    //debug purposes
                    //await context.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");

                }

            }

        }

        private void RegisterLogging()
        {

            _client.Log += (message)
                => Task.Run(()
                => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception));
            _commands.Log += (message)
                => Task.Run(()
                => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception));

        }

        private void Print(string message)
            => AltConsole.Write("Info", "Events", message);

    }
}
