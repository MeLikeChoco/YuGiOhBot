using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Handlers;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;
using IResult = Discord.Interactions.IResult;

namespace YuGiOh.Bot.Core
{
    public class Events
    {

        private readonly IServiceProvider _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly InteractionService _interactionService;
        private int _currentShards;
        private bool _isInitialized;

        public Events()
        {

            _services = new ServiceCollection().BuildServices();
            _loggerFactory = _services.GetService<ILoggerFactory>() ?? throw new NullReferenceException(nameof(_logger));
            _logger = _loggerFactory.CreateLogger("YuGiOh Bot");

            _logger.Info($"Welcome to {Assembly.GetExecutingAssembly().GetName()}");
            _logger.Info($"Using Discord.NET v{DiscordConfig.Version}");
            _logger.Info("Initializing events...");

            _client = _services.GetService<DiscordShardedClient>() ?? throw new NullReferenceException(nameof(_client));
            _commandService = _services.GetService<CommandService>() ?? throw new NullReferenceException(nameof(_commandService));
            _interactionService = _services.GetService<InteractionService>() ?? throw new NullReferenceException(nameof(_interactionService));

            _logger.Info("Finished initializing events.");

        }

        public async Task Initialize()
        {

            RegisterLogging();
            await RevEngines();

            if (!_isInitialized)
            {

                _client.ShardReady += ReadyOtherStuff;
                _client.ShardDisconnected += Whoopsies;

            }

        }

        private async Task ReadyOtherStuff(DiscordSocketClient _)
        {

            if (Interlocked.Increment(ref _currentShards) == _client.Shards.Count)
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

        private async Task AddFreshBlood()
        {

            Log("Processing guilds that were added when the bot was down...");

            var guildConfigService = _services.GetService<IGuildConfigDbService>()!;
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

            _client.JoinedGuild += guild => ActivatorUtilities.CreateInstance<GuildHandler>(_services).HandleAddedToGuildAsync(guild);

        }

        private void BuildServices()
        {

            Log("Building services...");

            //initialize the stats gathering timer
            _services.GetService<Stats>();

            Log($"Built services.");

        }

        private async Task RegisterCommands()
        {

            Log("Registering commands...");

            //i will have to monitor these changes in case of performance issues
            _client.MessageReceived += ActivatorUtilities.CreateInstance<CommandHandler>(_services).HandleCommandAsync;
            _client.MessageReceived += ActivatorUtilities.CreateInstance<ChatHandler>(_services).HandlePotentialInlineSearchAsync;
            _client.InteractionCreated += ActivatorUtilities.CreateInstance<InteractionHandler>(_services).HandleInteractionAsync;

            _commandService.AddTypeReader<string>(new StringInputTypeReader());

            var textCmdModules = await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            var appCmdModules = await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            var cmds = textCmdModules.SelectMany(module => module.Commands).Concat<object>(appCmdModules.SelectMany(module => module.SlashCommands));

            Log($"Registered {cmds.Count()} commands.");

        }

        private void RegisterLogging()
        {

            _client.Log += message
                => Task.Run(() => _loggerFactory.CreateLogger(message.Source).Log(message));

            _commandService.Log += message
                => Task.Run(() => _loggerFactory.CreateLogger(message.Source).Log(message));

            _interactionService.Log += message
                => Task.Run(() => _loggerFactory.CreateLogger(message.Source).Log(message));

            _interactionService.SlashCommandExecuted += (_, _, result)
                => Task.Run(() => LogInteractionExecuted(result));

            _interactionService.AutocompleteCommandExecuted += (_, _, result)
                => Task.Run(() => LogInteractionExecuted(result));

            _interactionService.ComponentCommandExecuted += (_, _, result)
                => Task.Run(() => LogInteractionExecuted(result));

        }

        private Task LogInteractionExecuted(IResult result)
        {

            if (!result.IsSuccess)
                _loggerFactory.CreateLogger("Error").Error(result.ErrorReason);

            return Task.CompletedTask;

        }

        private void Log(string message)
            // => AltConsole.Write("Info", "Events", message);
            => _logger.Info(message);

    }
}