using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using YuGiOh.Bot.Handlers;
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
        private InteractionService _interactionService;
        private Cache _cache;
        //private ServiceObserver _serviceObserver;
        private IServiceProvider _services;
        private int _recommendedShards, _currentShards;
        private bool _isInitialized;
        private ConcurrentDictionary<DiscordSocketClient, Timer> _reconnectTimers;
        //private readonly List<Reconnector<DiscordSocketClient>> _reconnectors;

        private static readonly DiscordSocketConfig ClientConfig = new()
        {

            //AlwaysDownloadUsers = true,
            ConnectionTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds, //had to include this as my bot got bigger and there were more guilds to connect to per shard
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 30,
            TotalShards = 1

        };

        private static readonly CommandServiceConfig CommandConfig = new()
        {

            DefaultRunMode = Discord.Commands.RunMode.Async,
            LogLevel = LogSeverity.Verbose

        };

        private static readonly InteractionServiceConfig InteractionConfig = new()
        {

            DefaultRunMode = Discord.Interactions.RunMode.Async,
            LogLevel = LogSeverity.Verbose,
            UseCompiledLambda = true //let's try it out, it can't use that much more memory..... hopefully

        };

        private static readonly InteractiveServiceConfig InteractiveServiceConfig = new()
        {

            DefaultTimeout = Timeout.InfiniteTimeSpan

        };

        public async Task Initialize()
        {

            Log("Initializing events...");

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

            Log($"Launching with {_recommendedShards} shards...");

            ClientConfig.TotalShards = _recommendedShards;
            _client = new DiscordShardedClient(ClientConfig);
            _commandService = new CommandService(CommandConfig);
            _interactionService = new InteractionService(_client, InteractionConfig);
            //_web = new Web();
            _cache = new Cache();
            _reconnectTimers = new ConcurrentDictionary<DiscordSocketClient, Timer>();
            //_reconnectors = new List<Reconnector<DiscordSocketClient>>();
            //_serviceObserver = new ServiceObserver();

            RegisterLogging();

            Log("Finished initializing events.");

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

            Log($"Shard {shard.ShardId} disconnected. Started reconnection timer.");

            shard.Ready += () => Task.Run(() =>
            {

                _reconnectTimers.Remove(shard, out var reconnectTimer);

                reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                reconnectTimer.Dispose();

                Log($"Shard {shard.ShardId} reconnected. Reconnection timer disposed.");

            });

            _reconnectTimers.TryAdd(shard, new Timer(async (state) =>
            {

                var shard = state as DiscordSocketClient;

                if (_reconnectTimers.ContainsKey(shard) && (shard.ConnectionState == ConnectionState.Disconnected || shard.ConnectionState == ConnectionState.Disconnecting))
                {

                    Log($"Shard {shard.ShardId} failed to reconnect. Reconnecting forcefully...");

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
            Log("Starting client...");
            await _client.StartAsync();
            Log("ITS UP AND RUNNING BOIIIIIIIIIIIIIIS");

        }

        private async Task TunnelIn()
        {

            var config = Config.Instance;
            var isTest = config.IsTest;
            var token = isTest ? config.Tokens.Discord.Test : config.Tokens.Discord.Legit;

            Log($"Test: {isTest}");
            Log("Logging in...");
            await _client.LoginAsync(TokenType.Bot, token);
            Log("Logged in.");

        }

        private async Task YouAintDoneYet()
        {

            //LoadStats();
            BuildServices();
            AddFreshBlood();
            await RegisterCommands();
            await _client.SetGameAsync($"Support guild/server: {Config.Instance.GuildInvite}");

            _isInitialized = true;

        }

        public void AddFreshBlood()
        {

            Log("Processing guilds that were added when the bot was down...");

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

            Log("Processed guilds that were added when the bot was down.");

            _client.JoinedGuild += guild =>
            {

                var guildConfigService = _services.GetService<IGuildConfigDbService>();

                return guildConfigService.InsertGuildConfigAsync(new GuildConfig { Id = guild.Id });

            };


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
                .AddTransient<IPerformanceMetrics, PerformanceMetrics>()
                .AddTransient<Web>()
                .AddHttpClient()
                .AddSingleton<InteractiveService>()
                .AddSingleton(InteractionConfig)
                .AddSingleton<InteractiveService<SocketInteractionContext<SocketSlashCommand>>>()
                .AddSingleton<CommandHelpService>()
                .AddSingleton(InteractiveServiceConfig)
                .AddSingleton(_client)
                .AddSingleton(_commandService)
                .AddSingleton(_interactionService)
                .AddSingleton(_cache)
                .AddSingleton<Stats>()
                .AddSingleton<Random>()
                .BuildServiceProvider();

            //initialize the stats gathering timer
            _services.GetService<Stats>();

        }

        private async Task RegisterCommands()
        {

            Log("Registering commands...");

            //_chat = new ChatHandler(
            //    _cache,
            //    _services.GetService<Web>(),
            //    _services.GetService<IYuGiOhDbService>(),
            //    _services.GetService<IGuildConfigDbService>()
            // );

            //_client.MessageReceived += HandleCommand;
            //_client.MessageReceived += somethingsomethingchat

            //i will have to monitor these changes in case of performance issues
            _client.MessageReceived += (message) => ActivatorUtilities.CreateInstance<CommandHandler>(_services).HandleCommand(message);
            _client.MessageReceived += (message) => ActivatorUtilities.CreateInstance<ChatHandler>(_services).HandlePotentialInlineSearch(message);
            _client.InteractionCreated += (interaction) => ActivatorUtilities.CreateInstance<InteractionHandler>(_services).HandleInteraction(interaction);

            _commandService.AddTypeReader<string>(new StringInputTypeReader());
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            var appCmdModules = await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

//#if !DEBUG
//            Log("Registering slash commands...");
//            await _interactionService.AddModulesGloballyAsync(true, appCmdModules.ToArray());
//            Log("Finished registering slash commands.");
//#endif

            Log("Commands registered.");

        }

        private void RegisterLogging()
        {

            _client.Log += (message)
                => Task.Run(()
                => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception));
            _commandService.Log += (message)
                => Task.Run(()
                => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception));
            _interactionService.Log += (message)
                => Task.Run(()
                => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception));

            _interactionService.SlashCommandExecuted += (cmdInfo, context, result)
                => Task.Run(() => LogInteractionExecuted(cmdInfo, context, result));

            _interactionService.AutocompleteCommandExecuted += (cmdInfo, context, result)
                => Task.Run(() => LogInteractionExecuted(cmdInfo, context, result));

            _interactionService.ComponentCommandExecuted += (cmdInfo, context, result)
                => Task.Run(() => LogInteractionExecuted(cmdInfo, context, result));

        }

        private Task LogInteractionExecuted<T>(T cmdInfo, IInteractionContext context, Discord.Interactions.IResult result)
            where T : ICommandInfo
        {

            if (!result.IsSuccess)
                AltConsole.Write("Error", "Error", result.ErrorReason);

            return Task.CompletedTask;

        }

        private void Log(string message)
            => AltConsole.Write("Info", "Events", message);

    }
}
