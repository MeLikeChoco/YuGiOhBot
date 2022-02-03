using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive.Callbacks
{

    public interface IInteractionCallback
    {

        SocketCommandContext Context { get; }
        Commands.RunMode RunMode { get; }
        ICriterion<SocketInteraction> Criterion { get; }
        TimeSpan? Timeout { get; }

        Task<bool> HandleCallbackAsync(SocketInteraction interaction);

    }

    public interface IInteractionCallback<TContext>
        where TContext : IInteractionContext
    {

        TContext Context { get; }
        Interactions.RunMode RunMode { get; }
        ICriterion<SocketInteraction> Criterion { get; }
        TimeSpan? Timeout { get; }

        Task<bool> HandleCallbackAsync(SocketInteraction interaction);

    }

}
