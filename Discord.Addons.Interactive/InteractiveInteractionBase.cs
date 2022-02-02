using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public abstract class InteractiveInteractionBase : InteractiveInteractionBase<SocketInteractionContext<SocketInteraction>>
    {

    }

    public abstract class InteractiveSlashBase : InteractiveInteractionBase<SocketInteractionContext<SocketSlashCommand>>
    {

    }

    public abstract class InteractiveUserBase : InteractiveInteractionBase<SocketInteractionContext<SocketUserCommand>>
    {

    }

    public abstract class InteractiveMessageBase : InteractiveInteractionBase<SocketInteractionContext<SocketMessageCommand>>
    {

    }

    public abstract class InteractiveInteractionBase<TContext> : InteractionModuleBase<TContext>
        where TContext : class, IInteractionContext
    {

        public InteractiveInteractionService<TContext> Interactive { get; set; }

        protected bool IsDeferred { get; set; }

        protected override async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
        {

            await base.DeferAsync(ephemeral, options);

            IsDeferred = true;

        }

        public Task<SocketMessage> NextMessageAsync(ICriterion<SocketMessage> criterion, TimeSpan? timeout = null, CancellationToken token = default)
            => Interactive.NextMessageAsync(Context, criterion, timeout, token);
        public Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null, CancellationToken token = default)
            => Interactive.NextMessageAsync(Context, fromSourceUser, inSourceChannel, timeout, token);

        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTTS = false, Embed embed = null, TimeSpan? timeout = null, RequestOptions options = null)
            => Interactive.ReplyAndDeleteAsync(Context, content, isTTS, embed, timeout, options, IsDeferred);

        public Task<IUserMessage> PagedReplyAsync(IEnumerable<object> pages, bool fromSourceUser = true)
        {
            var pager = new PaginatedMessage
            {
                Pages = pages
            };
            return PagedReplyAsync(pager, fromSourceUser);
        }
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true)
        {
            var criterion = new Criteria<SocketReaction>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            return PagedReplyAsync(pager, criterion);
        }
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<SocketReaction> criterion)
            => Interactive.SendPaginatedMessageAsync(Context, pager, criterion, IsDeferred);


    }
}
