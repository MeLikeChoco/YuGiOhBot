//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Discord.Addons.Interactive.Paginator;
//using Discord.Interactions;
//using Discord.Rest;
//using Discord.WebSocket;

//namespace Discord.Addons.Interactive
//{

//    public class InteractiveSlashService : InteractiveInteractionService<SocketInteractionContext<SocketSlashCommand>>
//    {

//        public InteractiveSlashService(DiscordSocketClient discord, InteractiveServiceConfig config = null)
//            : this((BaseSocketClient)discord, config) { }

//        public InteractiveSlashService(DiscordShardedClient discord, InteractiveServiceConfig config = null)
//            : this((BaseSocketClient)discord, config) { }

//        public InteractiveSlashService(BaseSocketClient discord, InteractiveServiceConfig config = null)
//            : base(discord, config) { }

//    }

//    public class InteractiveInteractionService<TContext> : IDisposable
//        where TContext : IInteractionContext
//    {

//        public BaseSocketClient Discord { get; }

//        private readonly Dictionary<ulong, IReactionInteractionCallback<TContext>> _callbacks;
//        private readonly TimeSpan _defaultTimeout;

//        // helpers to allow DI containers to resolve without a custom factory
//        public InteractiveInteractionService(DiscordSocketClient discord, InteractiveServiceConfig config = null)
//            : this((BaseSocketClient)discord, config) { }

//        public InteractiveInteractionService(DiscordShardedClient discord, InteractiveServiceConfig config = null)
//            : this((BaseSocketClient)discord, config) { }

//        public InteractiveInteractionService(BaseSocketClient discord, InteractiveServiceConfig config = null)
//        {
//            Discord = discord;
//            Discord.ReactionAdded += HandleReactionAsync;

//            config ??= new InteractiveServiceConfig();
//            _defaultTimeout = config.DefaultTimeout;

//            _callbacks = new();
//        }

//        public void AddReactionCallback(IMessage message, IReactionInteractionCallback<TContext> callback)
//            => _callbacks[message.Id] = callback;

//        public void RemoveReactionCallback(IMessage message)
//            => RemoveReactionCallback(message.Id);

//        public void RemoveReactionCallback(ulong id)
//            => _callbacks.Remove(id);

//        public void ClearReactionCallbacks()
//            => _callbacks.Clear();

//        private async Task HandleReactionAsync(
//            Cacheable<IUserMessage, ulong> message,
//            Cacheable<IMessageChannel, ulong> channel,
//            SocketReaction reaction)
//        {
//            if (reaction.UserId == Discord.CurrentUser.Id) return;
//            if (!_callbacks.TryGetValue(message.Id, out var callback)) return;

//            if (!await callback.Criterion.JudgeAsync(callback.Context, reaction))
//                return;

//            switch (callback.RunMode)
//            {
//                case RunMode.Async:
//                    _ = Task.Run(async () =>
//                    {
//                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
//                            RemoveReactionCallback(message.Id);
//                    });
//                    break;
//                default:
//                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
//                        RemoveReactionCallback(message.Id);
//                    break;
//            }
//        }

//        public Task<SocketMessage> NextMessageAsync(
//            TContext context,
//            bool fromSourceUser = true,
//            bool inSourceChannel = true,
//            TimeSpan? timeout = null,
//            CancellationToken token = default)
//        {
//            var criterion = new Criteria<SocketMessage>();
//            if (fromSourceUser)
//                criterion.AddCriterion(new EnsureSourceUserCriterion());
//            if (inSourceChannel)
//                criterion.AddCriterion(new EnsureSourceChannelCriterion());
//            return NextMessageAsync(context, criterion, timeout, token);
//        }

//        public async Task<SocketMessage> NextMessageAsync(
//            TContext context,
//            ICriterion<SocketMessage> criterion,
//            TimeSpan? timeout = null,
//            CancellationToken token = default)
//        {
//            timeout ??= _defaultTimeout;

//            var eventTrigger = new TaskCompletionSource<SocketMessage>();
//            var cancelTrigger = new TaskCompletionSource<bool>();

//            token.Register(() => cancelTrigger.SetResult(true));

//            async Task Handler(SocketMessage message)
//            {
//                var result = await criterion.JudgeAsync(context, message).ConfigureAwait(false);
//                if (result)
//                    eventTrigger.SetResult(message);
//            }

//            (context.Client as DiscordSocketClient).MessageReceived += Handler;

//            var trigger = eventTrigger.Task;
//            var cancel = cancelTrigger.Task;
//            var delay = Task.Delay(timeout.Value);
//            var task = await Task.WhenAny(trigger, delay, cancel).ConfigureAwait(false);

//            (context.Client as DiscordSocketClient).MessageReceived -= Handler;

//            if (task == trigger)
//                return await trigger.ConfigureAwait(false);
//            else
//                return null;
//        }

//        public async Task<IUserMessage> ReplyAndDeleteAsync(
//            TContext context,
//            string content, bool isTTS = false,
//            Embed embed = null,
//            TimeSpan? timeout = null,
//            RequestOptions options = null,
//            bool isDeferred = false)
//        {

//            timeout ??= _defaultTimeout;
//            IUserMessage message;

//            if (isDeferred)
//                message = await context.Interaction.FollowupAsync(content, isTTS: isTTS, embed: embed, options: options).ConfigureAwait(false);
//            else
//            {

//                await context.Interaction.RespondAsync(content, isTTS: isTTS, embed: embed, options: options).ConfigureAwait(false);

//                message = await context.Interaction.GetOriginalResponseAsync().ConfigureAwait(false);

//            }

//            _ = Task.Delay(timeout.Value)
//                .ContinueWith(_ => context.Interaction.DeleteOriginalResponseAsync().ConfigureAwait(false))
//                .ConfigureAwait(false);

//            return message;

//        }

//        public async Task<IUserMessage> SendPaginatedMessageAsync(
//            TContext context,
//            PaginatedMessage pager,
//            ICriterion<SocketReaction> criterion = null,
//            bool isDeferred = false)
//        {
//            var callback = new PaginatedInteractionMessageCallback<TContext>(this, context, pager, criterion, isDeferred);

//            await callback.DisplayAsync().ConfigureAwait(false);

//            return callback.Message;
//        }

//        public async Task<IUserMessage> SendPaginatedComponentMessageAsync(
//            TContext context,
//            PaginatedMessage pager,
//            ICriterion<SocketMessageComponent> criterion = null,
//            bool isDeferred = false)
//        {



//        }

//        public void Dispose()
//        {
//            Discord.ReactionAdded -= HandleReactionAsync;
//        }

//    }

//}
