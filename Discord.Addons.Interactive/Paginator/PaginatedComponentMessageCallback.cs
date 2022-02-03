using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive.Callbacks;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace Discord.Addons.Interactive.Paginator
{

    public abstract class BasePaginatedComponentMessageCallback
    {

        private const string FirstId = "first";
        private const string PreviousId = "previous";
        private const string NextId = "next";
        private const string LastId = "last";
        private const string StopId = "stop";
        private const string JumpId = "jump";
        private const string InfoId = "info";

        protected PaginatedMessage _pager;
        protected int _pages;
        protected int _page = 1;

        protected PaginatedAppearanceOptions Options => _pager.Options;

        protected virtual Embed BuildEmbed()
        {

            var builder = new EmbedBuilder()
                .WithAuthor(_pager.Author)
                .WithColor(_pager.Color)
                .WithFooter(f => f.Text = string.Format(Options.FooterFormat, _page, _pages))
                .WithTitle(_pager.Title);

            if (_pager.Pages is IEnumerable<EmbedFieldBuilder> efb)
            {
                builder.Fields = efb.Skip((_page - 1) * Options.FieldsPerPage).Take(Options.FieldsPerPage).ToList();
                builder.Description = _pager.AlternateDescription;
            }
            else
            {
                builder.Description = _pager.Pages.ElementAt(_page - 1).ToString();
            }

            return builder.Build();

        }

        protected virtual MessageComponent BuildComponents(IChannel channel, IUser user)
        {

            const ButtonStyle buttonStyle = ButtonStyle.Secondary;

            var builder = new ComponentBuilder()
                .WithButton(style: buttonStyle, emote: Options.First, customId: FirstId, disabled: _page <= 1)
                .WithButton(style: buttonStyle, emote: Options.Back, customId: PreviousId, disabled: _page <= 1)
                .WithButton(style: buttonStyle, emote: Options.Next, customId: NextId, disabled: _page >= _pages)
                .WithButton(style: buttonStyle, emote: Options.Last, customId: LastId, disabled: _page >= _pages);

            var manageMessages = (channel is IGuildChannel guildChannel)
                   && (user as IGuildUser).GetPermissions(guildChannel).ManageMessages;

            if (Options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || (Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                builder.WithButton(style: buttonStyle, emote: Options.Jump, customId: JumpId);

            builder.WithButton(style: buttonStyle, emote: new Emoji("🛑"), customId: StopId);

            if (Options.DisplayInformationIcon)
                builder.WithButton(style: buttonStyle, emote: Options.Info, customId: InfoId);

            return builder.Build();

        }

        protected virtual async Task RenderAsync(SocketMessageComponent interaction)
        {

            var embed = BuildEmbed();
            var components = BuildComponents(interaction.Channel, interaction.User);

            await interaction.UpdateAsync(message =>
            {
                message.Embed = embed;
                message.Components = components;
            }).ConfigureAwait(false);

        }

    }

    public class PaginatedComponentMessageCallback :
        BasePaginatedComponentMessageCallback,
        IInteractionCallback
    {

        private const string FirstId = "first";
        private const string PreviousId = "previous";
        private const string NextId = "next";
        private const string LastId = "last";
        private const string StopId = "stop";
        private const string JumpId = "jump";
        private const string InfoId = "info";

        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; }
        public IUserMessage Message { get; private set; }

        public Commands.RunMode RunMode => Commands.RunMode.Async;
        public ICriterion<SocketInteraction> Criterion { get; }
        public TimeSpan? Timeout => Options.Timeout;

        private readonly bool _isDeferred;

        public PaginatedComponentMessageCallback(
            InteractiveService interactive,
            SocketCommandContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketInteraction> criterion,
            bool isDeferred)
        {

            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketInteraction>();
            _pager = pager;
            _pages = _pager.Pages.Count();
            _isDeferred = isDeferred;

            if (_pager.Pages is IEnumerable<EmbedFieldBuilder>)
                _pages = ((_pager.Pages.Count() - 1) / Options.FieldsPerPage) + 1;

        }

        public async Task DisplayAsync()
        {

            var embed = BuildEmbed();
            var components = BuildComponents(Context.Channel, Context.User);
            IUserMessage message = await Context.Channel.SendMessageAsync(_pager.Content, embed: embed, components: components);
            Message = message;

            Interactive.AddInteractionCallback(message, this);

            if (Timeout.HasValue && Timeout.HasValue != default)
            {

                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {

                    Interactive.RemoveInteractionCallback(message);
                    Context.Message.DeleteAsync();

                });

            }

        }

        public async Task<bool> HandleCallbackAsync(SocketInteraction interaction)
        {

            var msgComponent = interaction as SocketMessageComponent;

            switch (msgComponent.Data.CustomId)
            {

                case FirstId:
                    _page = 1;
                    break;
                case NextId:
                    {

                        if (_page >= _pages)
                            return false;

                        ++_page;
                        break;

                    }
                case PreviousId:
                    {

                        if (_page <= 1)
                            return false;

                        --_page;
                        break;

                    }
                case LastId:
                    _page = _pages;
                    break;
                case StopId:
                    await interaction.DeleteOriginalResponseAsync().ConfigureAwait(false);
                    return true;
                case JumpId:
                    {

                        _ = Task.Run(async () =>
                        {

                            var criteria = new Criteria<SocketMessage>()
                                .AddCriterion(new EnsureSourceChannelCriterion())
                                .AddCriterion(new EnsureFromUserCriterion(interaction.User.Id))
                                .AddCriterion(new EnsureIsIntegerCriterion());

                            var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                            var request = int.Parse(response.Content);
                            if (request < 1 || request > _pages)
                            {
                                _ = response.DeleteAsync().ConfigureAwait(false);
                                await Interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name);
                                return;
                            }
                            _page = request;
                            _ = response.DeleteAsync().ConfigureAwait(false);
                            await RenderAsync(msgComponent).ConfigureAwait(false);

                        });

                        break;

                    }
                case InfoId:
                    await Interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
                    return false;

            }

            await RenderAsync(msgComponent).ConfigureAwait(false);
            return false;

        }

    }

    public class PaginatedComponentMessageCallback<TContext> :
        BasePaginatedComponentMessageCallback,
        IInteractionCallback<TContext>
        where TContext : IInteractionContext
    {

        private const string FirstId = "first";
        private const string PreviousId = "previous";
        private const string NextId = "next";
        private const string LastId = "last";
        private const string StopId = "stop";
        private const string JumpId = "jump";
        private const string InfoId = "info";

        public TContext Context { get; }
        public InteractiveService<TContext> Interactive { get; }
        public IUserMessage Message { get; private set; }

        public Interactions.RunMode RunMode => Interactions.RunMode.Async;
        public ICriterion<SocketInteraction> Criterion { get; }
        public TimeSpan? Timeout => Options.Timeout;

        private readonly bool _isDeferred;

        public PaginatedComponentMessageCallback(
            InteractiveService<TContext> interactive,
            TContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketInteraction> criterion,
            bool isDeferred)
        {

            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketInteraction>();
            _pager = pager;
            _pages = _pager.Pages.Count();
            _isDeferred = isDeferred;

            if (_pager.Pages is IEnumerable<EmbedFieldBuilder>)
                _pages = ((_pager.Pages.Count() - 1) / Options.FieldsPerPage) + 1;

        }

        public async Task DisplayAsync()
        {

            var embed = BuildEmbed();
            var components = BuildComponents(Context.Channel, Context.User);
            IUserMessage message;

            if (_isDeferred)
                message = await Context.Interaction.FollowupAsync(_pager.Content, embed: embed, components: components).ConfigureAwait(false);
            else
            {

                await Context.Interaction.RespondAsync(_pager.Content, embed: embed, components: components).ConfigureAwait(false);

                message = await Context.Interaction.GetOriginalResponseAsync().ConfigureAwait(false);

            }

            Message = message;

            Interactive.AddInteractionCallback(message, this);

            if (Timeout.HasValue && Timeout.HasValue != default)
            {

                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {

                    Interactive.RemoveInteractionCallback(message);
                    Context.Interaction.DeleteOriginalResponseAsync();

                });

            }

        }

        public async Task<bool> HandleCallbackAsync(SocketInteraction interaction)
        {

            var msgComponent = interaction as SocketMessageComponent;

            switch (msgComponent.Data.CustomId)
            {

                case FirstId:
                    _page = 1;
                    break;
                case NextId:
                    ++_page;
                    break;
                case PreviousId:
                    --_page;
                    break;
                case LastId:
                    _page = _pages;
                    break;
                case StopId:
                    await Message.DeleteAsync().ConfigureAwait(false);
                    return true;
                case JumpId:
                    {

                        _ = Task.Run(async () =>
                        {

                            var criteria = new Criteria<SocketMessage>()
                                .AddCriterion(new EnsureSourceChannelCriterion())
                                .AddCriterion(new EnsureFromUserCriterion(interaction.User.Id))
                                .AddCriterion(new EnsureIsIntegerCriterion());

                            var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                            var request = int.Parse(response.Content);
                            if (request < 1 || request > _pages)
                            {
                                _ = response.DeleteAsync().ConfigureAwait(false);
                                await Interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name);
                                return;
                            }
                            _page = request;
                            _ = response.DeleteAsync().ConfigureAwait(false);
                            await RenderAsync(msgComponent).ConfigureAwait(false);

                        });

                        break;

                    }
                case InfoId:
                    await Interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
                    return false;

            }

            await RenderAsync(msgComponent).ConfigureAwait(false);
            return false;

        }
    }

}
