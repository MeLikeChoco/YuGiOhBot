using System;
using System.Text;
using Discord;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using YuGiOh.Bot.Logger;
using YuGiOh.Bot.Models;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#pragma warning disable CA2254

namespace YuGiOh.Bot.Extensions
{
    public static class LoggerExtensions
    {

        private static Serilog.ILogger? _logger;

        #region Info Logging

        public static void Info(this ILogger logger, string message, params object[] args)
            => logger.LogInformation(message, args);

        public static void Info(
            this ILogger logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogInformation(eventId, message, args);

        public static void Info(
            this ILogger logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogInformation(exception, message, args);

        public static void Info(
            this ILogger logger,
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

        public static void Error(this ILogger logger, string message, params object[] args)
            => logger.LogError(message, args);

        public static void Error(
            this ILogger logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogError(eventId, message, args);

        public static void Error(
            this ILogger logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogError(exception, message, args);

        public static void Error(
            this ILogger logger,
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

        public static void Verbose(this ILogger logger, string message, params object[] args)
            => logger.LogTrace(message, args);

        public static void Verbose(
            this ILogger logger,
            EventId eventId,
            string message,
            params object[] args
        )
            => logger.LogTrace(eventId, message, args);

        public static void Verbose(
            this ILogger logger,
            Exception exception,
            string message,
            params object[] args
        )
            => logger.LogTrace(exception, message, args);

        public static void Verbose(
            this ILogger logger,
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

        public static void Log(this ILogger logger, LogMessage message)
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

            logger.Log(logLevel, message.Message);

        }

        public static Serilog.ILogger CreateStaticLogger()
            => GetStaticLogger();

        public static Serilog.ILogger CreateStaticLogger(string context)
            => GetStaticLogger().ForContext("SourceContext", context);

        private static Serilog.ILogger GetStaticLogger()
        {

            if (_logger is not null)
                return _logger;

            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Error) //disable IHttpClientFactory logging except for Errors
                .MinimumLevel.Override("System.Net.Http", LogEventLevel.Error) //disable HttpClient logging except for Errors
                .WriteTo.Console(outputTemplate: Config.Instance.LoggingTemplates.Console, theme: ConsoleTheme.None) //ConsoleTheme.None to disable theme overwriting my own colors
                .WriteTo.File(
                    Config.Instance.LogFilePath,
                    outputTemplate: Config.Instance.LoggingTemplates.File,
                    encoding: Encoding.UTF8,
                    shared: false,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 5
                )
                .Enrich.FromLogContext()
                .Enrich.With<ColorEnricher>()
                .Enrich.With<LogLevelEnricher>()
                .CreateLogger();

            return _logger;

        }

    }
}

#pragma warning restore CA2254