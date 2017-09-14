using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class CustomBase : InteractiveBase<SocketCommandContext>
    {

        public async Task SendEmbed(EmbedBuilder embed, bool minimal)
            => await ReplyAsync("", embed: (await Chat.EditEmbed(embed, minimal)).Build());

        public async Task NoResultError(string input = null)
        {

            var str = "Nothing was found with the given input";

            if (!string.IsNullOrEmpty(input))
                str += $" ({input})";

            str += "!";

            await ReplyAsync(str);            

        }

        public async Task TooManyError()
            => await ReplyAsync("Too many results were returned, please refine your search!");

    }
}
