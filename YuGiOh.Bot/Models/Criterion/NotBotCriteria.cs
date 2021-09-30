using Discord.Addons.Interactive;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace YuGiOh.Bot.Models.Criterion
{
    public class NotBotCriteria : ICriterion<SocketMessage>
    {

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
            => Task.FromResult(!parameter.Author.IsBot);

    }
}
