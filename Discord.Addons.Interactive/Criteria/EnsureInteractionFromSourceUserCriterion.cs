using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class EnsureInteractionFromSourceUserCriterion : ICriterion<SocketInteraction>
    {

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketInteraction parameter)
            => Task.FromResult(sourceContext.User.Id == parameter.User.Id);

        public Task<bool> JudgeAsync(IInteractionContext sourceContext, SocketInteraction parameter)
            => Task.FromResult(sourceContext.User.Id == parameter.User.Id);

    }
}
