using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;

namespace Discord.Addons.Interactive.Paginator
{
    public class PaginatedInteractionMessageCallback<TContext> : IReactionInteractionCallback<TContext>
        where TContext : IInteractionContext
    {

        public TContext Context { get; }
        public InteractiveInteractionService<TContext> Interactive { get; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion { get; }
        public TimeSpan? Timeout => Options.Timeout;

        private readonly PaginatedMessage _pager;
        private readonly bool _isDeferred;

        private PaginatedAppearanceOptions Options => _pager.Options;

        private readonly int _pages;
        private int _page = 1;


        public PaginatedInteractionMessageCallback(
            InteractiveInteractionService<TContext> interactive,
            TContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null,
            bool isDeferred = false)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _pager = pager;
            _pages = _pager.Pages.Count();
            _isDeferred = isDeferred;
            if (_pager.Pages is IEnumerable<EmbedFieldBuilder>)
                _pages = ((_pager.Pages.Count() - 1) / Options.FieldsPerPage) + 1;
        }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            IUserMessage message;

            if (_isDeferred)
                message = await Context.Interaction.FollowupAsync(_pager.Content, embed: embed).ConfigureAwait(false);
            else
            {

                await Context.Interaction.RespondAsync(_pager.Content, embed: embed).ConfigureAwait(false);

                message = await Context.Interaction.GetOriginalResponseAsync().ConfigureAwait(false);

            }

            Message = message;

            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                await message.AddReactionAsync(Options.First);
                await message.AddReactionAsync(Options.Back);
                await message.AddReactionAsync(Options.Next);
                await message.AddReactionAsync(Options.Last);

                var manageMessages = (Context.Channel is IGuildChannel guildChannel)
                    && (Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages;

                if (Options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || (Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                    await message.AddReactionAsync(Options.Jump);

                await message.AddReactionAsync(Options.Stop);

                if (Options.DisplayInformationIcon)
                    await message.AddReactionAsync(Options.Info);
            });

            // TODO: (Next major version) timeouts need to be handled at the service-level!
            if (Timeout.HasValue && Timeout.Value != null)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    Context.Interaction.DeleteOriginalResponseAsync();
                    //_ = Message.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(Options.First))
                _page = 1;
            else if (emote.Equals(Options.Next))
            {
                if (_page >= _pages)
                    return false;
                ++_page;
            }
            else if (emote.Equals(Options.Back))
            {
                if (_page <= 1)
                    return false;
                --_page;
            }
            else if (emote.Equals(Options.Last))
                _page = _pages;
            else if (emote.Equals(Options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(Options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
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
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(Options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
                return false;
            }
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }

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

        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Context.Interaction.ModifyOriginalResponseAsync(m => m.Embed = embed).ConfigureAwait(false);
        }

    }
}
