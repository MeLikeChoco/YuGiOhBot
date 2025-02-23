using System;
using System.Runtime.InteropServices;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Repositories;
using YuGiOh.Common.Repositories.Interfaces;
using RunMode = Discord.Commands.RunMode;

namespace YuGiOh.Bot.Extensions;

public static class ServiceCollectionExtensions
{

    #region Discord Client Configs

    private static readonly DiscordSocketConfig ClientConfig = new()
    {

        ConnectionTimeout = (int) TimeSpan.FromMinutes(1).TotalMilliseconds, //had to include this as my bot got bigger and there were more guilds to connect to per shard
        LogLevel = LogSeverity.Verbose,
        SuppressUnknownDispatchWarnings = true,
        MessageCacheSize = 20,
        GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.DirectMessages | GatewayIntents.AllUnprivileged

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

    private static readonly InteractiveConfig InteractiveConfig = new()
    {
        DefaultTimeout = TimeSpan.FromSeconds(60),
        LogLevel = LogSeverity.Verbose
    };

    #endregion Discord Client Configs

    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private const uint DisableNewlineAutoReturn = 0x0008;

    public static IServiceProvider BuildServices(this IServiceCollection serviceCollection)
    {

        return serviceCollection
            .AddLogging(BuildLogging)
            .AddSingleton(Config.Instance)
            .AddSingleton<IYuGiOhRepositoryConfiguration, RepoConfig>()
            .AddSingleton<IGuildConfigConfiguration, RepoConfig>()
            .AddSingleton<IYuGiOhRepository, YuGiOhRepository>()
            .AddSingleton<IGuildConfigRepository, GuildConfigRepository>()
            .AddSingleton<IYuGiOhDbService, YuGiOhDbService>()
            .AddSingleton<IYuGiOhPricesService, YuGiOhPricesService>()
            .AddSingleton<IGuildConfigDbService, GuildConfigDbService>()
            .AddTransient<IPerformanceMetrics>(_ => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? new LinuxPerformanceMetrics() : new WindowsPerformanceMetrics())
            .AddSingleton<Web>()
            .AddHttpClient()
            .AddSingleton<CommandHelpService>()
            .AddSingleton<DiscordShardedClient>()
            .AddSingleton<CommandService>()
            //.AddSingleton<InteractionService>()
            .AddSingleton(serviceProvider => new InteractionService(serviceProvider.GetRequiredService<DiscordShardedClient>())) //to fix InteractionService injection not picking up IRestClientProvider from DiscordShardedClient
            .AddSingleton(ClientConfig)
            .AddSingleton(CommandConfig)
            .AddSingleton(InteractionConfig)
            .AddSingleton<Cache>()
            .AddSingleton<Stats>()
            .AddSingleton<Random>()
            .AddSingleton(InteractiveConfig)
            .AddSingleton<InteractiveService>()
            .AddSingleton<PaginatorFactory>()
            .BuildServiceProvider();

    }

    private static void BuildLogging(ILoggingBuilder logBuilder)
    {

        EnableAnsiSupport();

        var serilogger = LoggerExtensions.CreateStaticLogger();

        logBuilder.AddSerilog(serilogger, true);

    }

    private static void EnableAnsiSupport()
    {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            EnableAnsiSupportWindows();

    }

    private static void EnableAnsiSupportWindows()
    {

        var stdOut = GetStdHandle(StdOutputHandle);

        GetConsoleMode(stdOut, out var consoleMode);

        consoleMode |= EnableVirtualTerminalProcessing | DisableNewlineAutoReturn;

        SetConsoleMode(stdOut, consoleMode);

    }

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

}