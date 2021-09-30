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
    public class NotInlineSearchCriteria : ICriterion<SocketMessage>
    {

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
            => Task.FromResult(!parameter.Content.StartsWith("[[") && !parameter.Content.EndsWith("]]"));

    }
}
