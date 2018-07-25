using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class CustomBase : InteractiveBase
    {

        public Task DirectMessageAsync(string message, Embed embed)
            => Context.User.SendMessageAsync(message, embed: embed);

        public Task UploadAsync(Stream stream, string filename, string text = null)
            => Context.Channel.SendFileAsync(stream, filename, text);

        public async Task SendCardEmbed(EmbedBuilder embed, bool minimal)
            => await ReplyAsync("", embed: (await Chat.EditEmbed(embed, minimal)).Build());

        public async Task SendEmbed(EmbedBuilder embed)
            => await ReplyAsync("", embed: embed.Build());

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
