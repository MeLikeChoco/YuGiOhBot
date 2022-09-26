using System;
using System.Text;
using Discord;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using YuGiOh.Bot.Logger;
using YuGiOh.Bot.Models;
using ILogger = Serilog.ILogger;

#pragma warning disable CA2254

namespace YuGiOh.Bot.Extensions
{
    public static class LoggerExtensions
    {

        private static ILogger _logger;

        #region Info Logging

        public static void Info(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args)
            => logger.LogInformation(message, args);

        public static void Info(
            this Microsoft.Extensions.Logging.ILogger logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogInformation(eventId, message, args);

        public static void Info(
            this Microsoft.Extensions.Logging.ILogger logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogInformation(exception, message, args);

        public static void Info(
            this Microsoft.Extensions.Logging.ILogger logger,
            EventId eventId,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogInformation(eventId, exception, message, args);

        public static void Info<T>(this ILogger<T> logger, string message, params object[] args)
            => logger.LogInformation(message, args);

        public static void Info<T>(
            this ILogger<T> logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogInformation(eventId, message, args);

        public static void Info<T>(
            this ILogger<T> logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogInformation(exception, message, args);

        public static void Info<T>(
            this ILogger<T> logger,
            EventId eventId,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogInformation(eventId, exception, message, args);

        #endregion Info Logging

        #region Error Logging

        public static void Error(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args)
            => logger.LogError(message, args);

        public static void Error(
            this Microsoft.Extensions.Logging.ILogger logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogError(eventId, message, args);

        public static void Error(
            this Microsoft.Extensions.Logging.ILogger logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogError(exception, message, args);

        public static void Error(
            this Microsoft.Extensions.Logging.ILogger logger,
            EventId eventId,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogError(eventId, exception, message, args);

        public static void Error<T>(this ILogger<T> logger, string message, params object[] args)
            => logger.LogError(message, args);

        public static void Error<T>(
            this ILogger<T> logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogError(eventId, message, args);

        public static void Error<T>(
            this ILogger<T> logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogError(exception, message, args);

        public static void Error<T>(
            this ILogger<T> logger,
            EventId eventId,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogError(eventId, exception, message, args);

        #endregion Error Logging

        #region Verbose Logging

        public static void Verbose(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args)
            => logger.LogTrace(message, args);

        public static void Verbose(
            this Microsoft.Extensions.Logging.ILogger logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogTrace(eventId, message, args);

        public static void Verbose(
            this Microsoft.Extensions.Logging.ILogger logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogTrace(exception, message, args);

        public static void Verbose(
            this Microsoft.Extensions.Logging.ILogger logger,
            EventId eventId,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogTrace(eventId, exception, message, args);

        public static void Verbose<T>(this ILogger<T> logger, string message, params object[] args)
            => logger.LogTrace(message, args);

        public static void Verbose<T>(
            this ILogger<T> logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogTrace(eventId, message, args);

        public static void Verbose<T>(
            this ILogger<T> logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogTrace(exception, message, args);

        public static void Verbose<T>(
            this ILogger<T> logger,
            EventId eventId,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogTrace(eventId, exception, message, args);

        #endregion Verbose Logging

        public static void Log(this Microsoft.Extensions.Logging.ILogger logger, LogMessage message)
        {

            var logLevel = message.Severity switch
            {

                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Debug => LogLevel.Debug,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Trace,
                LogSeverity.Warning => LogLevel.Warning,
                _ => LogLevel.Information

            };

            if (message.Exception is not null)
                logger.Error(message.Exception, message.Message);
            else
                logger.Log(logLevel, message.Message);

        }

        public static ILogger CreateStaticLogger()
            => GetStaticLogger();

        public static ILogger CreateStaticLogger(string context)
            => GetStaticLogger().ForContext("SourceContext", context);

        private static ILogger GetStaticLogger()
        {

            if (_logger is not null)
                return _logger;

            Console.OutputEncoding = Encoding.UTF8;

            _logger = new LoggerConfiguration()
                .ConfigureMinimumLevel()
                .ConfigureEnrich()
                .ConfigureWriteTo()
                .CreateLogger();

            return _logger;

        }

        private static LoggerConfiguration ConfigureMinimumLevel(this LoggerConfiguration loggerConfiguration)
            => loggerConfiguration
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Error) //disable IHttpClientFactory logging except for Errors
                .MinimumLevel.Override("System.Net.Http", LogEventLevel.Error); //disable HttpClient logging except for Errors

        private static LoggerConfiguration ConfigureWriteTo(this LoggerConfiguration loggerConfiguration)
            => loggerConfiguration
                .WriteTo.File(Config.Instance.LogFilePath,
                    outputTemplate: Config.Instance.LoggingTemplates.File,
                    encoding: Encoding.UTF8,
                    shared: false,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 5
                )
                .WriteTo.Logger(lc =>
                    lc
                        .Filter.ByExcluding(logEvent
                            => logEvent.MessageTemplate.Text.StartsWith("unknown dispatch", StringComparison.OrdinalIgnoreCase)
                               || logEvent.MessageTemplate.Text.StartsWith("error handling dispatch", StringComparison.OrdinalIgnoreCase))
                        .WriteTo.Console(outputTemplate: Config.Instance.LoggingTemplates.Console, theme: ConsoleTheme.None)
                ); //ConsoleTheme.None to disable theme overwriting my own colors
        // .WriteTo.Console(
        //     outputTemplate: Config.Instance.LoggingTemplates.Console,
        //     theme: ConsoleTheme.None
        // );

        private static LoggerConfiguration ConfigureEnrich(this LoggerConfiguration loggerConfiguration)
            => loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.With<ColorEnricher>()
                .Enrich.With<LogLevelEnricher>();

    }
}

#pragma warning restore CA2254