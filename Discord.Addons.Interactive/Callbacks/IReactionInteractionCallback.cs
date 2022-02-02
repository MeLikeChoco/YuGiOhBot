using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public interface IReactionInteractionCallback<TContext>
        where TContext : IInteractionContext
    {

        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        TContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);

    }
}
