using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criterion
{
    public class CharacterOnlyCriteria : ICriterion<SocketMessage>
    {

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
            => Task.FromResult(parameter.Content.Length == 1);

        public Task<bool> JudgeAsync(IInteractionContext sourceContext, SocketMessage parameter)
            => Task.FromResult(parameter.Content.Length == 1);

    }
}
