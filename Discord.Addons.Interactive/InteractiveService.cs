using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive.Callbacks;
using Discord.Addons.Interactive.Paginator;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{

    public abstract class BaseInteractiveService : IDisposable
    {

        protected BaseSocketClient Discord { get; }
        protected TimeSpan DefaultTimeout { get; }

        protected BaseInteractiveService(BaseSocketClient client, InteractiveServiceConfig config = null)
        {

            Discord = client;

            Discord.ReactionAdded += HandleReactionAsync;
            Discord.InteractionCreated += HandleInteractionAsync;
            Discord.MessageDeleted += HandleMessageDeleteAsync;

            config ??= new InteractiveServiceConfig();
            DefaultTimeout = config.DefaultTimeout;

        }

        protected abstract Task HandleReactionAsync(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction
        );

        protected abstract Task HandleInteractionAsync(SocketInteraction interaction);

        protected abstract Task HandleMessageDeleteAsync(
            Cacheable<IMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel
        );

        public void Dispose()
        {

            Discord.ReactionAdded -= HandleReactionAsync;
            Discord.InteractionCreated -= HandleInteractionAsync;
            Discord.MessageDeleted -= HandleMessageDeleteAsync;

        }

    }

    public class InteractiveService : BaseInteractiveService
    {

        private readonly Dictionary<ulong, IReactionCallback> _callbacks;
        private readonly Dictionary<ulong, IInteractionCallback> _interactionCallbacks;

        // helpers to allow DI containers to resolve without a custom factory
        public InteractiveService(DiscordSocketClient discord, InteractiveServiceConfig config = null)
            : this((BaseSocketClient)discord, config) { }

        public InteractiveService(DiscordShardedClient discord, InteractiveServiceConfig config = null)
            : this((BaseSocketClient)discord, config) { }

        public InteractiveService(BaseSocketClient discord, InteractiveServiceConfig config = null)
            : base(discord, config)
        {
            _callbacks = new();
            _interactionCallbacks = new();
        }

        public void AddReactionCallback(IMessage message, IReactionCallback callback)
            => _callbacks[message.Id] = callback;
        public void RemoveReactionCallback(IMessage message)
            => RemoveReactionCallback(message.Id);
        public void RemoveReactionCallback(ulong id)
            => _callbacks.Remove(id);
        public void ClearReactionCallbacks()
            => _callbacks.Clear();

        public void AddInteractionCallback(IMessage message, IInteractionCallback callback)
            => _interactionCallbacks[message.Id] = callback;
        public void RemoveInteractionCallback(IMessage message)
            => RemoveInteractionCallback(message.Id);
        public void RemoveInteractionCallback(ulong id)
            => _interactionCallbacks.Remove(id);
        public void ClearInteractionCallback()
            => _interactionCallbacks.Clear();

        protected override async Task HandleReactionAsync(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            if (reaction.UserId == Discord.CurrentUser.Id) return;
            if (!_callbacks.TryGetValue(message.Id, out var callback)) return;

            if (!await callback.Criterion.JudgeAsync(callback.Context, reaction).ConfigureAwait(false))
                return;

            switch (callback.RunMode)
            {
                case Commands.RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                            RemoveReactionCallback(message.Id);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                        RemoveReactionCallback(message.Id);
                    break;
            }
        }

        protected override async Task HandleInteractionAsync(SocketInteraction interaction)
        {

            if (interaction.User.Id == Discord.CurrentUser.Id)
                return;

            ulong messageId;

            switch (interaction)
            {

                case SocketMessageComponent componentMsg:
                    messageId = componentMsg.Message.Id;
                    break;
                default:
                    return;

            }

            if (!_interactionCallbacks.TryGetValue(messageId, out var callback))
                return;

            if (!await callback.Criterion.JudgeAsync(callback.Context, interaction))
                return;

            async Task RemoveCallbackWrapper()
            {
                if (await callback.HandleCallbackAsync(interaction).ConfigureAwait(false))
                    RemoveInteractionCallback(messageId);
            }

            switch (callback.RunMode)
            {

                case Commands.RunMode.Async:
                    _ = RemoveCallbackWrapper();
                    break;
                default:
                    await RemoveCallbackWrapper();
                    break;

            }

        }

        protected override Task HandleMessageDeleteAsync(
            Cacheable<IMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel)
        {

            RemoveReactionCallback(message.Id);
            RemoveInteractionCallback(message.Id);

            return Task.CompletedTask;

        }

        public Task<SocketMessage> NextMessageAsync(
            SocketCommandContext context,
            bool fromSourceUser = true,
            bool inSourceChannel = true,
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            var criterion = new Criteria<SocketMessage>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureSourceUserCriterion());
            if (inSourceChannel)
                criterion.AddCriterion(new EnsureSourceChannelCriterion());
            return NextMessageAsync(context, criterion, timeout, token);
        }

        public async Task<SocketMessage> NextMessageAsync(
            SocketCommandContext context,
            ICriterion<SocketMessage> criterion,
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            timeout ??= DefaultTimeout;

            var eventTrigger = new TaskCompletionSource<SocketMessage>();
            var cancelTrigger = new TaskCompletionSource<bool>();

            token.Register(() => cancelTrigger.SetResult(true));

            async Task Handler(SocketMessage message)
            {
                var result = await criterion.JudgeAsync(context, message).ConfigureAwait(false);
                if (result)
                    eventTrigger.SetResult(message);
            }

            context.Client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var cancel = cancelTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay, cancel).ConfigureAwait(false);

            context.Client.MessageReceived -= Handler;

            if (task == trigger)
                return await trigger.ConfigureAwait(false);
            else
                return null;
        }

        public async Task<IUserMessage> ReplyAndDeleteAsync(
            SocketCommandContext context,
            string content, bool isTTS = false,
            Embed embed = null,
            TimeSpan? timeout = null,
            RequestOptions options = null)
        {
            timeout ??= DefaultTimeout;
            var message = await context.Channel.SendMessageAsync(content, isTTS, embed, options).ConfigureAwait(false);
            _ = Task.Delay(timeout.Value)
                .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            return message;
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(
            SocketCommandContext context,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null)
        {
            var callback = new PaginatedMessageCallback(this, context, pager, criterion);
            await callback.DisplayAsync().ConfigureAwait(false);
            return callback.Message;
        }

        public async Task<IUserMessage> SendPaginatedComponentMessageAsync(
            SocketCommandContext context,
            PaginatedMessage pager,
            ICriterion<SocketInteraction> criterion = null)
        {

            var callback = new PaginatedComponentMessageCallback(this, context, pager, criterion);
            var options = pager.Options;
            var timeout = options.Timeout ?? DefaultTimeout;

            await callback.DisplayAsync().ConfigureAwait(false);

            var message = callback.Message;

            _ = Task.Delay(timeout).ContinueWith(_ =>
            {

                RemoveInteractionCallback(message.Id);

                if (options.ShouldDeleteOnTimeout)
                    message.DeleteAsync();

            });

            return message;

        }

    }

    public class InteractiveService<TContext> : BaseInteractiveService
       where TContext : IInteractionContext
    {

        private readonly Dictionary<ulong, IReactionInteractionCallback<TContext>> _reactionCallbacks;
        private readonly Dictionary<ulong, IInteractionCallback<TContext>> _interactionCallbacks;

        // helpers to allow DI containers to resolve without a custom factory
        public InteractiveService(DiscordSocketClient discord, InteractiveServiceConfig config = null)
            : this((BaseSocketClient)discord, config) { }

        public InteractiveService(DiscordShardedClient discord, InteractiveServiceConfig config = null)
            : this((BaseSocketClient)discord, config) { }

        public InteractiveService(BaseSocketClient discord, InteractiveServiceConfig config = null)
            : base(discord, config)
        {
            _reactionCallbacks = new();
            _interactionCallbacks = new();
        }

        public void AddReactionCallback(IMessage message, IReactionInteractionCallback<TContext> callback)
            => _reactionCallbacks[message.Id] = callback;
        public void RemoveReactionCallback(IMessage message)
            => RemoveReactionCallback(message.Id);
        public void RemoveReactionCallback(ulong id)
            => _reactionCallbacks.Remove(id);
        public void ClearReactionCallbacks()
            => _reactionCallbacks.Clear();

        public void AddInteractionCallback(IMessage message, IInteractionCallback<TContext> callback)
            => _interactionCallbacks[message.Id] = callback;
        public void RemoveInteractionCallback(IMessage message)
            => RemoveInteractionCallback(message.Id);
        public void RemoveInteractionCallback(ulong id)
            => _interactionCallbacks.Remove(id);
        public void ClearInteractionCallback()
            => _interactionCallbacks.Clear();

        protected override async Task HandleReactionAsync(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            if (reaction.UserId == Discord.CurrentUser.Id) return;
            if (!_reactionCallbacks.TryGetValue(message.Id, out var callback)) return;

            if (!await callback.Criterion.JudgeAsync(callback.Context, reaction))
                return;

            switch (callback.RunMode)
            {
                case Interactions.RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                            RemoveReactionCallback(message.Id);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                        RemoveReactionCallback(message.Id);
                    break;
            }
        }

        protected override async Task HandleInteractionAsync(SocketInteraction interaction)
        {

            if (interaction.User.Id == Discord.CurrentUser.Id)
                return;

            ulong messageId;

            switch (interaction)
            {

                case SocketMessageComponent componentMsg:
                    messageId = componentMsg.Message.Id;
                    break;
                default:
                    return;

            }

            if (!_interactionCallbacks.TryGetValue(messageId, out var callback))
                return;

            if (!await callback.Criterion.JudgeAsync(callback.Context, interaction))
                return;

            async Task RemoveCallbackWrapper()
            {
                if (await callback.HandleCallbackAsync(interaction).ConfigureAwait(false))
                    RemoveInteractionCallback(messageId);
            }

            switch (callback.RunMode)
            {

                case Interactions.RunMode.Async:
                    _ = RemoveCallbackWrapper();
                    break;
                default:
                    await RemoveCallbackWrapper();
                    break;

            }

        }

        protected override Task HandleMessageDeleteAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {

            RemoveInteractionCallback(message.Id);
            RemoveInteractionCallback(message.Id);

            return Task.CompletedTask;

        }

        public Task<SocketMessage> NextMessageAsync(
            TContext context,
            bool fromSourceUser = true,
            bool inSourceChannel = true,
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            var criterion = new Criteria<SocketMessage>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureSourceUserCriterion());
            if (inSourceChannel)
                criterion.AddCriterion(new EnsureSourceChannelCriterion());
            return NextMessageAsync(context, criterion, timeout, token);
        }

        public async Task<SocketMessage> NextMessageAsync(
            TContext context,
            ICriterion<SocketMessage> criterion,
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            timeout ??= DefaultTimeout;

            var eventTrigger = new TaskCompletionSource<SocketMessage>();
            var cancelTrigger = new TaskCompletionSource<bool>();

            token.Register(() => cancelTrigger.SetResult(true));

            async Task Handler(SocketMessage message)
            {

                var result = await criterion.JudgeAsync(context, message).ConfigureAwait(false);

                if (result)
                    eventTrigger.SetResult(message);

            }

            (context.Client as DiscordSocketClient).MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var cancel = cancelTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay, cancel).ConfigureAwait(false);

            (context.Client as DiscordSocketClient).MessageReceived -= Handler;

            if (task == trigger)
                return await trigger.ConfigureAwait(false);
            else
                return null;
        }

        public async Task<IUserMessage> ReplyAndDeleteAsync(
            TContext context,
            string content, bool isTTS = false,
            Embed embed = null,
            TimeSpan? timeout = null,
            RequestOptions options = null,
            bool isDeferred = false)
        {

            timeout ??= DefaultTimeout;
            IUserMessage message;

            if (isDeferred)
                message = await context.Interaction.FollowupAsync(content, isTTS: isTTS, embed: embed, options: options).ConfigureAwait(false);
            else
            {

                await context.Interaction.RespondAsync(content, isTTS: isTTS, embed: embed, options: options).ConfigureAwait(false);

                message = await context.Interaction.GetOriginalResponseAsync().ConfigureAwait(false);

            }

            _ = Task.Delay(timeout.Value)
                .ContinueWith(_ => context.Interaction.DeleteOriginalResponseAsync().ConfigureAwait(false))
                .ConfigureAwait(false);

            return message;

        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(
            TContext context,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null,
            bool isDeferred = false)
        {
            var callback = new PaginatedInteractionMessageCallback<TContext>(this, context, pager, criterion, isDeferred);

            await callback.DisplayAsync().ConfigureAwait(false);

            return callback.Message;
        }

        public async Task<IUserMessage> SendPaginatedComponentMessageAsync(
            TContext context,
            PaginatedMessage pager,
            ICriterion<SocketInteraction> criterion = null,
            bool isDeferred = false)
        {

            var callback = new PaginatedComponentMessageCallback<TContext>(this, context, pager, criterion, isDeferred);
            var options = pager.Options;
            var timeout = options.Timeout ?? DefaultTimeout;

            await callback.DisplayAsync().ConfigureAwait(false);

            var message = callback.Message;

            _ = Task.Delay(timeout).ContinueWith(_ =>
            {

                RemoveInteractionCallback(message.Id);

                if (options.ShouldDeleteOnTimeout)
                    message.DeleteAsync();

            });

            return message;

        }

    }

}
