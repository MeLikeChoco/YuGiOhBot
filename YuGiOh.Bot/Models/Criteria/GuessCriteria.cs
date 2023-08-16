using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Criteria
{
    public class GuessCriteria : ICriteria
    {

        public IEnumerable<string> PossibleAnswers { get; }
        public string Answer { get; private set; }

        public GuessCriteria(params string[] possibleAnswers)
        {
            PossibleAnswers = possibleAnswers.Where(possibleAnswer => !string.IsNullOrEmpty(possibleAnswer));
        }

        public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
        {

            var input = message.Content.ConvertTypesetterToTypewriter();
            Answer = PossibleAnswers.FirstOrDefault(possibleAnswer => possibleAnswer.EqualsIgnoreCase(input));

            return Task.FromResult(!string.IsNullOrEmpty(Answer));

        }

        public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
        {

            var input = message.Content.ConvertTypesetterToTypewriter();
            Answer = PossibleAnswers.FirstOrDefault(possibleAnswer => possibleAnswer.EqualsIgnoreCase(input));

            return Task.FromResult(!string.IsNullOrEmpty(Answer));

        }

    }
}