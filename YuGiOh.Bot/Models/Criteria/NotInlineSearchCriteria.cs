using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria
{
    public class NotInlineSearchCriteria : ICriteria
    {

        public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
            => Task.FromResult(!message.Content.StartsWith("[[") && !message.Content.EndsWith("]]"));

        public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
            => Task.FromResult(!message.Content.StartsWith("[[") && !message.Content.EndsWith("]]"));

    }
}