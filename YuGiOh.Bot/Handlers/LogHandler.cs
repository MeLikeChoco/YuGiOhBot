using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Handlers
{
    public class LogHandler
    {

        private readonly ILoggerFactory _loggerFactory;

        public LogHandler(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Task HandleLogMessage(LogMessage logMsg)
        {

            Task.Run(() => _loggerFactory.CreateLogger(logMsg.Source).Log(logMsg));

            return Task.CompletedTask;

        }

        public Task HandleInteractionExecuted(ICommandInfo _, IInteractionContext __, IResult result)
        {

            Task.Run(() => _loggerFactory.CreateLogger("Interaction").Error(result.ErrorReason));

            return Task.CompletedTask;

        }

    }
}