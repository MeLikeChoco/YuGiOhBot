using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules
{
    public abstract class MainInteractionBase<TInteraction> : InteractiveInteractionBase<SocketInteractionContext<TInteraction>>
        where TInteraction : SocketInteraction
    {

        protected Cache Cache { get; }
        protected IYuGiOhDbService YuGiOhDbService { get; }
        protected IGuildConfigDbService GuildConfigDbService { get; }
        protected Web Web { get; }

        protected GuildConfig GuildConfig;

        protected static PaginatedAppearanceOptions PagedOptions => new()
        {

            DisplayInformationIcon = false,
            JumpDisplayOptions = JumpDisplayOptions.Never,
            FooterFormat = "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}"

        };

        protected MainInteractionBase(
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web
        )
        {

            Cache = cache;
            YuGiOhDbService = yuGiOhDbService;
            GuildConfigDbService = guildConfigDbService;
            Web = web;

        }

        public override async Task BeforeExecuteAsync(ICommandInfo command)
        {

            GuildConfig = Context.Channel is not SocketDMChannel ?
                await GuildConfigDbService.GetGuildConfigAsync(Context.Guild.Id) :
                await GuildConfigDbService.GetGuildConfigAsync(0);

        }

        protected override async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
        {

            await base.DeferAsync(ephemeral, options);

            IsDeferred = true;

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

        protected async Task SendCardEmbedAsync(EmbedBuilder embed, bool minimal, Web web = null)
        {

            embed = await embed.WithCardPrices(minimal, web ?? Web);

            if (IsDeferred)
                await FollowupAsync(embed: embed.Build());
            else if (Context.Interaction.HasResponded)
                await ReplyAsync(embed: embed.Build());
            else
                await RespondAsync(embed: embed.Build());

        }

        protected Task SendEmbedAsync(EmbedBuilder embed)
            => RespondAsync(embed: embed.Build());

        protected Task NoResultError(string input = null)
            => NoResultError("cards", input);

        protected Task NoResultError(string objects, string input = null)
        {

            var str = $"No {objects} were found with the given input";

            if (!string.IsNullOrEmpty(input))
                str += $" ({input})";

            str += "!";

            if (Context.Interaction.HasResponded)
                return ReplyAsync(str);
            else
                return RespondAsync(str);

        }

        protected Task TooManyError()
            => RespondAsync("Too many results were returned, please refine your search!");

        protected void Log(string msg, string currentCaller = null)
            => AltConsole.Write("Info", currentCaller ?? GetType().Name, msg);

    }
}