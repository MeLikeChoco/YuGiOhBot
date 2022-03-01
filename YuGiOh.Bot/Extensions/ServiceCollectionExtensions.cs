using System;
using System.Runtime.InteropServices;
using System.Threading;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
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

    #endregion Discord Client Configs

    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private const uint DisableNewlineAutoReturn = 0x0008;

    public static IServiceProvider BuildServices(this IServiceCollection serviceCollection)
    {

        return serviceCollection
            .AddLogging(BuildLogging)
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
            .AddSingleton<CommandHelpService>()
            .AddSingleton<DiscordShardedClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<InteractionService>()
            .AddSingleton(ClientConfig)
            .AddSingleton(CommandConfig)
            .AddSingleton(InteractionConfig)
            .AddSingleton<Cache>()
            .AddSingleton<Stats>()
            .AddSingleton<Random>()
            .AddSingleton<InteractiveService>()
            .AddSingleton(InteractiveServiceConfig)
            .AddSingleton<InteractiveService<SocketInteractionContext<SocketSlashCommand>>>()
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