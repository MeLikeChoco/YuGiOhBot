using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criterion
{
    public class NotBotCriteria : ICriterion<SocketMessage>
    {

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
            => Task.FromResult(!parameter.Author.IsBot);

        public Task<bool> JudgeAsync(IInteractionContext sourceContext, SocketMessage parameter)
            => Task.FromResult(!parameter.Author.IsBot);
    }
}
