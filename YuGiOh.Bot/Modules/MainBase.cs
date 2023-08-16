using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
    public abstract class MainBase : ModuleBase<ShardedCommandContext>
    {

        protected ILogger Logger { get; }
        protected Cache Cache { get; }
        protected IYuGiOhDbService YuGiOhDbService { get; }
        protected IGuildConfigDbService GuildConfigDbService { get; }
        protected Web Web { get; }
        protected Random Rand { get; }
        protected InteractiveService InteractiveService { get; }

        protected GuildConfig GuildConfig { get; set; }

        // protected PaginatedAppearanceOptions PagedOptions => new()
        // {
        //
        //     DisplayInformationIcon = false,
        //     JumpDisplayOptions = JumpDisplayOptions.Never,
        //     FooterFormat = GuildConfig.AutoDelete ? "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}" : "This embed will not be deleted! | Page {0}/{1}",
        //     Timeout = GuildConfig.AutoDelete ? TimeSpan.FromSeconds(60) : TimeSpan.FromMilliseconds(-1)
        //
        // };

        protected MainBase(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand,
            InteractiveService interactiveService
        )
        {

            Logger = loggerFactory.CreateLogger(GetType().Name);
            Cache = cache;
            YuGiOhDbService = yuGiOhDbService;
            GuildConfigDbService = guildConfigDbService;
            Web = web;
            Rand = rand;
            InteractiveService = interactiveService;

        }

        protected override void BeforeExecute(CommandInfo command)
        {

            GuildConfig = Context.Channel is not SocketDMChannel ? GuildConfigDbService.GetGuildConfigAsync(Context.Guild.Id).GetAwaiter().GetResult() : GuildConfigDbService.GetGuildConfigAsync(0).GetAwaiter().GetResult();

        }

        protected Task DirectMessageAsync(Embed embed)
            => Context.User.SendMessageAsync(embed: embed);

        protected Task DirectMessageAsync(EmbedBuilder embedBuilder)
            => DirectMessageAsync(embedBuilder.Build());

        protected Task DirectMessageAsync(string message, Embed embed)
            => Context.User.SendMessageAsync(message, embed: embed);

        protected Task UploadAsync(Stream stream, string filename, string text = null)
            => Context.Channel.SendFileAsync(stream, filename, text);

        protected async Task SendCardEmbedAsync(EmbedBuilder embed, bool minimal, Web web = null)
            => await ReplyAsync(embed: (await embed.WithCardPrices(minimal, web ?? Web)).Build());

        protected Task SendEmbedAsync(EmbedBuilder embed)
            => ReplyAsync(embed: embed.Build());

        protected Task NoResultError(string input)
            => NoResultError("cards", input);

        protected Task NoResultError(string objects, string input)
        {

            var str = $"No {objects} were found with the given input";

            if (!string.IsNullOrEmpty(input))
                str += $" ({input})";

            str += "!";

            return ReplyAsync(str, allowedMentions: AllowedMentions.None);

        }

        protected Task TooManyError()
            => ReplyAsync("Too many results were returned, please refine your search!");

        protected override Task<IUserMessage> ReplyAsync(
            string message = null,
            bool isTTS = false,
            Embed embed = null,
            RequestOptions options = null,
            AllowedMentions allowedMentions = null,
            MessageReference messageReference = null,
            MessageComponent components = null,
            ISticker[] stickers = null,
            Embed[] embeds = null,
            MessageFlags flags = MessageFlags.None
        )
        {

            if (!string.IsNullOrWhiteSpace(message))
                message = message
                    .Replace("@everyone", "\\@everyone")
                    .Replace("@here", "\\@here");

            return base.ReplyAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds);

        }

        protected Task<InteractiveResult<SocketMessage>> NextMessageAsync(
            ICriteria criteria,
            TimeSpan timeSpan,
            CancellationToken ct = default
        )
        {

            return InteractiveService.NextMessageAsync(
                (message) => criteria.ValidateAsync(Context, message).Result,
                timeout: timeSpan,
                cancellationToken: ct
            );

        }

        protected Task<InteractiveResult<SocketMessage>> NextMessageAsync(Criteria criteria, CancellationToken ct = default)
        {

            return NextMessageAsync(
                criteria,
                TimeSpan.FromSeconds(60),
                ct
            );

        }

        protected Task<InteractiveResult<SocketMessage>> NextMessageAsync(CancellationToken ct = default)
        {

            return NextMessageAsync(
                new BaseCriteria(Context),
                TimeSpan.FromSeconds(60),
                ct
            );

        }

        protected Task<InteractiveMessageResult> SendPaginatorAsync(
            Paginator paginator,
            TimeSpan timeSpan,
            CancellationToken ct = default
        )
        {

            return InteractiveService.SendPaginatorAsync(
                paginator,
                Context.Channel,
                timeout: timeSpan,
                cancellationToken: ct
            );

        }

        protected Task<InteractiveMessageResult> SendPaginatorAsync(Paginator paginator, CancellationToken ct = default)
            => SendPaginatorAsync(paginator, TimeSpan.FromSeconds(60), ct);

    }
}