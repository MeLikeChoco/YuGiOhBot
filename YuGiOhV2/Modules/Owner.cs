using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//The getinvite command can be intruding, however, I only use it to ask for feedback on the bot, else there is no other way to get 
//any sort of information on what I could improve or add. Sure, I have the feedback command, but no one uses it.

namespace YuGiOhV2.Modules
{
    [RequireOwner]
    public class Owner : CustomBase
    {

        [Command("getinvite")]
        public async Task GetInviteCommand([Remainder]string name)
        {

            try
            {

                var guild = Context.Client.Guilds.FirstOrDefault(g => g.Name == name);
                var invite = await guild.DefaultChannel.CreateInviteAsync(3600, 1, true);

                await ReplyAsync(invite.Url);

            }catch(Exception e)
            {

                await ReplyAsync($"Error in creating invite! ```{e.Message}```");

            }

        }

        [Command("megaphone")]
        public async Task MegaphoneCommand([Remainder]string message)
        {

            foreach(var guild in Context.Client.Guilds.Where(guild => !guild.Name.Contains("Discord Bot")))
            {

                try
                {

                    await guild.DefaultChannel.SendMessageAsync($":megaphone: {message}");

                }
                catch { }

            }

            await ReplyAsync("Done!");

        }

    }
}
