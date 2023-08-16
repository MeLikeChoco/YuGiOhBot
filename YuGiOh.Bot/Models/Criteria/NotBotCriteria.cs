using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria
{
    public class NotBotCriteria : ICriteria
    {

        public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
            => Task.FromResult(!message.Author.IsBot);

        public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
            => Task.FromResult(!message.Author.IsBot);
        
    }
}