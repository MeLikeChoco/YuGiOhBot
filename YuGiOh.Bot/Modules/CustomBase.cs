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

        public Task DirectMessageAsync(Embed embed)
            => Context.User.SendMessageAsync(embed: embed);

        public Task DirectMessageAsync(EmbedBuilder embedBuilder)
            => DirectMessageAsync(embedBuilder.Build());

        public Task DirectMessageAsync(string message, Embed embed)
            => Context.User.SendMessageAsync(message, embed: embed);

        public Task UploadAsync(Stream stream, string filename, string text = null)
            => Context.Channel.SendFileAsync(stream, filename, text);

        public async Task SendCardEmbedAsync(EmbedBuilder embed, bool minimal, Web web)
            => await Context.Channel.SendMessageAsync(embed: (await embed.WithPrices(minimal, web)).Build());

        public Task SendEmbedAsync(EmbedBuilder embed)
            => Context.Channel.SendMessageAsync(embed: embed.Build());

        public Task NoResultError(string input = null)
            => NoResultError("cards", input);

        public Task NoResultError(string objects, string input = null)
        {

            var str = $"No {objects} were found with the given input";

            if (!string.IsNullOrEmpty(input))
                str += $" ({input})";

            str += "!";

            return ReplyAsync(str);

        }

        public Task TooManyError()
            => ReplyAsync("Too many results were returned, please refine your search!");

    }
}
