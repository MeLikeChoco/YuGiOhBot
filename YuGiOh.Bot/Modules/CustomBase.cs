using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Modules
{
    public class CustomBase : InteractiveBase<ShardedCommandContext>
    {

        protected Task DirectMessageAsync(Embed embed)
            => Context.User.SendMessageAsync(embed: embed);

        protected Task DirectMessageAsync(EmbedBuilder embedBuilder)
            => DirectMessageAsync(embedBuilder.Build());

        protected Task DirectMessageAsync(string message, Embed embed)
            => Context.User.SendMessageAsync(message, embed: embed);

        protected Task UploadAsync(Stream stream, string filename, string text = null)
            => Context.Channel.SendFileAsync(stream, filename, text);

        protected async Task SendCardEmbedAsync(EmbedBuilder embed, bool minimal, Web web)
            => await ReplyAsync(embed: (await embed.WithCardPrices(minimal, web)).Build());

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

            return ReplyAsync(str);

        }

        protected Task TooManyError()
            => ReplyAsync("Too many results were returned, please refine your search!");

    }
}
