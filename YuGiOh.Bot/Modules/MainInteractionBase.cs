using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Criteria;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules
{
    public abstract class
        MainInteractionBase<TInteraction> : InteractionModuleBase<ShardedInteractionContext<TInteraction>>
        where TInteraction : SocketInteraction
    {
        protected ILogger Logger { get; }
        protected Cache Cache { get; }
        protected IYuGiOhDbService YuGiOhDbService { get; }
        protected IGuildConfigDbService GuildConfigDbService { get; }
        protected Web Web { get; }
        protected InteractiveService InteractiveService { get; }

        protected GuildConfig GuildConfig { get; private set; }
        protected bool IsDeferred { get; set; }

        // protected static PaginatedAppearanceOptions PagedOptions => new()
        // {
        //
        //     DisplayInformationIcon = false,
        //     JumpDisplayOptions = JumpDisplayOptions.Never,
        //     FooterFormat = "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}"
        //
        // };

        protected MainInteractionBase(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            InteractiveService interactiveService)
        {
            Logger = loggerFactory.CreateLogger(GetType().Name);
            Cache = cache;
            YuGiOhDbService = yuGiOhDbService;
            GuildConfigDbService = guildConfigDbService;
            Web = web;
            IsDeferred = false;
            InteractiveService = interactiveService;
        }

        public override async Task BeforeExecuteAsync(ICommandInfo command)
        {
            GuildConfig = Context.Channel is not SocketDMChannel
                ? await GuildConfigDbService.GetGuildConfigAsync(Context.Guild.Id)
                : await GuildConfigDbService.GetGuildConfigAsync(0);
        }

        protected override async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
        {
            await base.DeferAsync(ephemeral, options);

            IsDeferred = true;
        }

        protected override Task RespondAsync(
            string text = null,
            Embed[] embeds = null,
            bool isTTS = false,
            bool ephemeral = false,
            AllowedMentions allowedMentions = null,
            RequestOptions options = null,
            MessageComponent components = null,
            Embed embed = null,
            PollProperties poll = null
        )
        {
            return IsDeferred
                ? FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, components, embed)
                : !Context.Interaction.HasResponded
                    ? base.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, components, embed,
                        poll)
                    : base.ReplyAsync(text, isTTS, embed, options, allowedMentions, components: components);
        }

        protected Task DirectMessageAsync(Embed embed)
            => Context.User.SendMessageAsync(embed: embed);

        protected Task DirectMessageAsync(EmbedBuilder embedBuilder)
            => DirectMessageAsync(embedBuilder.Build());

        protected Task DirectMessageAsync(string message, Embed embed)
            => Context.User.SendMessageAsync(message, embed: embed);

        //public Task UploadAsync(Stream stream, string filename, string text = null)
        //    => Context.Channel.SendFileAsync(stream, filename, text);

        protected async Task<RestFollowupMessage> UploadAsync(Stream stream, string filename, string text = null)
        {
            await DeferAsync();

            return await Context.Interaction.FollowupWithFileAsync(new FileAttachment(stream, filename), text);
        }

        protected async Task SendCardEmbedAsync(EmbedBuilder embed, bool minimal, IYuGiOhPricesService yuGiOhPricesService = null)
        {
            embed = await embed.WithCardPrices(minimal, yuGiOhPricesService);

            await RespondAsync(embed: embed.Build());
        }

        protected Task SendEmbedAsync(EmbedBuilder embed)
            => RespondAsync(embed: embed.Build());

        protected Task NoResultError(string input)
            => NoResultError("cards", input);

        protected Task NoResultError(string objects, string input)
        {
            if (objects.Length + input.Length >= 1800)
                input = input[..(1800 - objects.Length)] + "...";

            var str = $"No {objects} were found with the given input";

            if (!string.IsNullOrWhiteSpace(input))
                str += $" ({input})";

            str += "!";

            return RespondAsync(str, allowedMentions: AllowedMentions.None);
        }

        // protected override Task<IUserMessage> ReplyAsync(
        //     string text = null,
        //     bool isTTS = false,
        //     Embed embed = null,
        //     RequestOptions options = null,
        //     AllowedMentions allowedMentions = null,
        //     MessageReference messageReference = null,
        //     MessageComponent components = null,
        //     ISticker[] stickers = null,
        //     Embed[] embeds = null,
        //     MessageFlags flags = MessageFlags.None,
        //     PollProperties poll = null
        // )
        //     => base.ReplyAsync(text, isTTS, embed, options, AllowedMentions.None, messageReference, components);

        protected Task<InteractiveMessageResult> SendPaginatorAsync(
            Paginator paginator,
            TimeSpan timeSpan,
            CancellationToken ct = default
        )
        {
            if (IsDeferred)
            {
                return InteractiveService.SendPaginatorAsync(
                    paginator,
                    Context.Interaction,
                    timeout: timeSpan,
                    responseType: InteractionResponseType.DeferredChannelMessageWithSource,
                    cancellationToken: ct
                );
            }

            if (!Context.Interaction.HasResponded)
            {
                return InteractiveService.SendPaginatorAsync(
                    paginator,
                    Context.Interaction,
                    timeout: timeSpan,
                    cancellationToken: ct
                );
            }

            return InteractiveService.SendPaginatorAsync(
                paginator,
                Context.Channel,
                timeout: timeSpan,
                cancellationToken: ct
            );
        }

        protected Task<InteractiveMessageResult> SendPaginatorAsync(
            Paginator paginator,
            CancellationToken ct = default
        )
            => SendPaginatorAsync(
                paginator,
                TimeSpan.FromSeconds(60),
                ct
            );


        protected Task<InteractiveResult<SocketMessage>> NextMessageAsync(
            ICriteria criteria,
            TimeSpan timespan,
            CancellationToken ct = default
        )
        {
            return InteractiveService.NextMessageAsync(
                (message) => criteria.ValidateAsync(Context, message).Result,
                timeout: timespan,
                cancellationToken: ct
            );
        }

        protected Task TooManyError()
            => RespondAsync("Too many results were returned, please refine your search!");

        protected void Log(string msg, params object[] parameters)
            => Logger.Info(msg, parameters);
    }
}