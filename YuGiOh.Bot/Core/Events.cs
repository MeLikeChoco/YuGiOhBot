using System;
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
using IResult = Discord.Interactions.IResult;
using RunMode = Discord.Commands.RunMode;

namespace YuGiOh.Bot.Core
{
    public class Events
    {

        private DiscordShardedClient _client;
        private CommandService _commandService;

        private InteractionService _interactionService;

        private IServiceProvider _services;
        private int _recommendedShards, _currentShards;
        private bool _isInitialized;

        private static readonly DiscordSocketConfig ClientConfig = new()
        {

            ConnectionTimeout = (int) TimeSpan.FromMinutes(1).TotalMilliseconds, //had to include this as my bot got bigger and there were more guilds to connect to per shard
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 20,
            TotalShards = 1

        };

        private static readonly CommandServiceConfig CommandConfig = new()
        {

            DefaultRunMode = RunMode.Async,
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

                await youAintDoneYet;

            }

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

        private Task YouAintDoneYet()
        {

            _ = _client.SetGameAsync($"Support guild/server: {Config.Instance.GuildInvite}")
                .ContinueWith(_ => BuildServices())
                .ContinueWith(_ => AddFreshBlood())
                .ContinueWith(_ => RegisterCommands())
                .ContinueWith(_ => _isInitialized = true);

            return Task.CompletedTask;

        }

        public async Task AddFreshBlood()
        {

            Log("Processing guilds that were added when the bot was down...");

            var guildConfigService = _services.GetService<IGuildConfigDbService>();
            var count = 0;

            await Parallel.ForEachAsync(
                _client.Guilds,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                async (guild, _) =>
                {

                    var doesExist = await guildConfigService.GuildConfigDoesExistAsync(guild.Id);

                    if (!doesExist)
                    {
                        await guildConfigService.InsertGuildConfigAsync(new GuildConfig { Id = guild.Id });
                        Interlocked.Increment(ref count);
                    }

                }
            );

            Log($"Processed guilds that were added when the bot was down. There were {count} guilds that were added.");

            _client.JoinedGuild += guild =>
            {

                var guildConfigService = _services.GetService<IGuildConfigDbService>();

                return guildConfigService.InsertGuildConfigAsync(new GuildConfig { Id = guild.Id });

            };


        }

        private void BuildServices()
        {

            Log("Building services...");

            var serviceCollection = new ServiceCollection()
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
                .AddSingleton<Cache>()
                .AddSingleton<Stats>()
                .AddSingleton<Random>();

            _services = serviceCollection.BuildServiceProvider();

            //initialize the stats gathering timer
            _services.GetService<Stats>();

            Log($"Built {serviceCollection.Count} services.");

        }

        private async Task RegisterCommands()
        {

            Log("Registering commands...");

            //i will have to monitor these changes in case of performance issues
            _client.MessageReceived += (message) => ActivatorUtilities.CreateInstance<CommandHandler>(_services).HandleCommand(message);
            _client.MessageReceived += (message) => ActivatorUtilities.CreateInstance<ChatHandler>(_services).HandlePotentialInlineSearch(message);
            _client.InteractionCreated += (interaction) => ActivatorUtilities.CreateInstance<InteractionHandler>(_services).HandleInteraction(interaction);

            _commandService.AddTypeReader<string>(new StringInputTypeReader());

            var textCmds = await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            var appCmds = await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            var cmds = textCmds.Concat<object>(appCmds);

            Log($"Registered {cmds.Count()} commands.");

        }

        private void RegisterLogging()
        {

            _client.Log += message
                => Task.Run(()
                    => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception)
                );

            _commandService.Log += message
                => Task.Run(()
                    => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception)
                );

            _interactionService.Log += message
                => Task.Run(()
                    => AltConsole.Write(message.Severity.ToString(), message.Source, message.Message, message.Exception)
                );

            _interactionService.SlashCommandExecuted += (_, _, result)
                => Task.Run(() => LogInteractionExecuted(result));

            _interactionService.AutocompleteCommandExecuted += (_, _, result)
                => Task.Run(() => LogInteractionExecuted(result));

            _interactionService.ComponentCommandExecuted += (_, _, result)
                => Task.Run(() => LogInteractionExecuted(result));

        }

        private static Task LogInteractionExecuted(IResult result)
        {

            if (!result.IsSuccess)
                AltConsole.Write("Error", "Error", result.ErrorReason);

            return Task.CompletedTask;

        }

        private static void Log(string message)
            => AltConsole.Write("Info", "Events", message);

    }
}