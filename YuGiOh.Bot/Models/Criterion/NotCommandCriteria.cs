using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criterion
{
    public class NotCommandCriteria : ICriterion<SocketMessage>
    {

        private readonly GuildConfig _guildConfig;

        public NotCommandCriteria(GuildConfig guildConfig)
            => _guildConfig = guildConfig;

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
            => Task.FromResult(!parameter.Content.StartsWith(_guildConfig.Prefix));

    }
}
