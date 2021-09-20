using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Repositories;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Bot.Core
{
    public class Events
    {

        private DiscordShardedClient _client;
        private CommandService _commandService;
        private Stats _stats;
        private Chat _chat;
        private Cache _cache;
        private Web _web;
        //private ServiceObserver _serviceObserver;
        private InteractiveService _interactive;
        private IServiceProvider _services;
        private int _recommendedShards, _currentShards;
        private bool _isInitialized;
        private ConcurrentDictionary<DiscordSocketClient, Timer> _reconnectTimers;
        //private readonly List<Reconnector<DiscordSocketClient>> _reconnectors;

        private static readonly DiscordSocketConfig ClientConfig = new()
        {

            //AlwaysDownloadUsers = true,
            ConnectionTimeout = 60000, //had to include this as my bot got bigger and there were more guilds to connect to per shard
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 30,
            TotalShards = 1,
            UseInternalRatelimiting = false

        };

        private static readonly CommandServiceConfig CommandConfig = new()
        {

            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose

        };

        public async Task Initialize()
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

            _recommendedShards = await GetRecommendedShardCountAsync();

            Print($"Launching with {_recommendedShards} shards...");

            ClientConfig.TotalShards = _recommendedShards;
            _client = new DiscordShardedClient(ClientConfig);
            _commandService = new CommandService(CommandConfig);
            _web = new Web();
            _cache = new Cache();
            _interactive = new InteractiveService(_client);
            _reconnectTimers = new ConcurrentDictionary<DiscordSocketClient, Timer>();
            //_reconnectors = new List<Reconnector<DiscordSocketClient>>();
            //_serviceObserver = new ServiceObserver();

            RegisterLogging();

            Print("Finished initializing events.");

            await GetReadyForBlastOff();

        }

        public async Task<int> GetRecommendedShardCountAsync()
        {

            await TunnelIn();

            var recommendedShards = await _client.GetRecommendedShardCountAsync();

            _ = _client.LogoutAsync();

            return recommendedShards;

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

            var config = Config.Instance;
            var isTest = config.IsTest;
            var token = isTest ? config.Tokens.Discord.Test : config.Tokens.Discord.Legit;

            Print($"Test: {isTest}");
            Print("Logging in...");
            await _client.LoginAsync(TokenType.Bot, token);
            Print("Logged in.");

        }

        private async Task YouAintDoneYet()
        {

            LoadStats();
            BuildServices();
            AddFreshBlood();
            await RegisterCommands();
            await _client.SetGameAsync($"Support guild/server: {Config.Instance.GuildInvite}");

            _isInitialized = true;

        }

        public void AddFreshBlood()
        {

            Print("Processing guilds that were added when the bot was down...");

            var guildConfigService = _services.GetService<IGuildConfigDbService>();

            //i await(get it?) the day ForEachAsync becomes official
            Parallel.ForEach(
                _client.Guilds,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                guild =>
                {

                    var doesExist = guildConfigService.GuildConfigDoesExistAsync(guild.Id).Result;

                    if (!doesExist)
                        guildConfigService.InsertGuildConfigAsync(new GuildConfig { Id = guild.Id }).GetAwaiter().GetResult();

                });

            Print("Processed guilds that were added when the bot was down.");

            _client.JoinedGuild += guild =>
            {

                var guildConfigService = _services.GetService<IGuildConfigDbService>();

                return guildConfigService.InsertGuildConfigAsync(new GuildConfig { Id = guild.Id });

            };


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
                .AddTransient<IGuildConfigConfiguration, RepoConfig>()
                .AddTransient<IYuGiOhRepository, YuGiOhRepository>()
                .AddTransient<IGuildConfigRepository, GuildConfigRepository>()
                .AddTransient<IYuGiOhDbService, YuGiOhDbService>()
                .AddTransient<IGuildConfigDbService, GuildConfigDbService>()
                .AddSingleton(_client)
                .AddSingleton(_cache)
                .AddSingleton(_interactive)
                .AddSingleton(_web)
                .AddSingleton(_stats)
                .AddSingleton<Random>()
                .BuildServiceProvider();

        }

        private async Task RegisterCommands()
        {

            Print("Registering commands...");

            _chat = new Chat(
                _cache,
                _services.GetService<Web>(),
                _services.GetService<IYuGiOhDbService>(),
                _services.GetService<IGuildConfigDbService>()
             );

            _client.MessageReceived += HandleCommand;
            _client.MessageReceived += (message) =>
            {

                Task.Run(() => _chat.SOMEONEGETTINGACARDBOIS(message));

                return Task.CompletedTask;

            };

            _commandService.AddTypeReader<string>(new StringInputTypeReader());
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Print("Commands registered.");

        }

        private async Task HandleCommand(SocketMessage message)
        {

            if (message is not SocketUserMessage
                || message.Author.IsBot
                || string.IsNullOrEmpty(message.Content))
                return;

            var prefix = "y!";

            if (message.Channel is SocketTextChannel textChannel)
            {

                var id = textChannel.Guild.Id;
                var guildConfigDbService = _services.GetService<IGuildConfigDbService>();
                var guildConfig = await guildConfigDbService.GetGuildConfigAsync(id);
                prefix = guildConfig.Prefix;

            }

            var possibleCmd = message as SocketUserMessage;
            var argPos = 0;

            if ((possibleCmd.HasStringPrefix(prefix, ref argPos) || possibleCmd.HasMentionPrefix(_client.CurrentUser, ref argPos)) &&
                possibleCmd.Content.Trim() != prefix)
            {

                var context = new ShardedCommandContext(_client, possibleCmd);

                if (message.Channel is SocketDMChannel)
                    AltConsole.Write("Info", "Command", $"{possibleCmd.Author.Username} in DM's");
                else if (message.Channel is SocketTextChannel txtChannel)
                    AltConsole.Write("Info", "Command", $"{possibleCmd.Author.Username} from {txtChannel.Guild.Name}");

                AltConsole.Write("Info", "Command", $"{possibleCmd.Content}");

                var result = await _commandService.ExecuteAsync(context, argPos, _services);

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
            _commandService.Log += (message)
                => Task.Run(()
                => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception));

        }

        private void Print(string message)
            => AltConsole.Write("Info", "Events", message);

    }
}
