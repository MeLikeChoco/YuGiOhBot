using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria
{
    public class NotCommandCriteria : ICriteria
    {

        private readonly GuildConfig _guildConfig;

        public NotCommandCriteria(GuildConfig guildConfig)
            => _guildConfig = guildConfig;

        public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
            => Task.FromResult(!message.Content.StartsWith(_guildConfig.Prefix));

        public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
            => Task.FromResult(!message.Content.StartsWith(_guildConfig.Prefix));

    }
}