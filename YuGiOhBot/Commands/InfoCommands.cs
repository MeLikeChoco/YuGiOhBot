using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Commands
{
    public class InfoCommands : ModuleBase
    {

        [Command("invite")]
        [Summary("Get invite link")]
        public async Task InviteCommand()
        {

            ulong id = Context.Client.GetApplicationInfoAsync().Result.Id;

            await ReplyAsync($"{Context.User.Mention} https://discordapp.com/oauth2/authorize?client_id={id}&scope=bot&permissions=0");

        }

    }
}
