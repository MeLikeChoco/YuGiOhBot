using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Handlers
{
    public class LogHandler
    {

        private static readonly ConcurrentDictionary<string, ILogger> SourceToLogger;

        private readonly ILoggerFactory _loggerFactory;

        static LogHandler()
        {
            SourceToLogger = new ConcurrentDictionary<string, ILogger>();
        }

        public LogHandler(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Task HandleLogMessage(LogMessage logMsg)
        {

            Task.Run(() =>
                SourceToLogger.GetOrAdd(logMsg.Source, source => _loggerFactory.CreateLogger(source)).Log(logMsg)
            );

            return Task.CompletedTask;

        }

        public Task HandleInteractionExecuted(ICommandInfo _, IInteractionContext __, IResult result)
        {

            Task.Run(() =>
            {
                if (!result.IsSuccess)
                    SourceToLogger.GetOrAdd("Interaction", source => _loggerFactory.CreateLogger(source)).Error(result.ErrorReason);
            });

            return Task.CompletedTask;

        }

    }
}