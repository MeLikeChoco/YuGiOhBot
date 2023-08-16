using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria
{
    public class CharacterOnlyCriteria : ICriteria
    {

        public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
            => Task.FromResult(message.Content.Length == 1);

        public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
            => Task.FromResult(message.Content.Length == 1);

    }
}