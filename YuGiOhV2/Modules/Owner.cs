using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using YuGiOhV2.Services;

//The getinvite command can be intruding, however, I only use it to ask for feedback on the bot, else there is no other way to get 
//any sort of information on what I could improve or add. Sure, I have the feedback command, but no one uses it.

namespace YuGiOhV2.Modules
{
    [RequireOwner]
    public class Owner : CustomBase
    {

        private Cache _cache;
        private Web _web;

        public Owner(Cache cache, Web web)
        {

            _cache = cache;
            _web = web;

        }

        [Command("getinvite")]
        [Summary("Gets invite to the default channel of the guild")]
        public async Task GetInviteCommand([Remainder]string name)
        {

            try
            {

                var guild = Context.Client.Guilds.FirstOrDefault(g => g.Name == name);
                var invite = await guild.DefaultChannel.CreateInviteAsync(3600, 1, true);

                await ReplyAsync(invite.Url);

            }
            catch (Exception e)
            {

                await ReplyAsync($"Error in creating invite! ```{e.Message}```");

            }

        }

        [Command("megaphone")]
        [Summary("Sends a message to all guilds USE WITH CAUTION")]
        public async Task MegaphoneCommand([Remainder]string message)
        {

            foreach (var guild in Context.Client.Guilds.Where(guild => !guild.Name.Contains("Discord Bot")))
            {

                try
                {

                    await guild.DefaultChannel.SendMessageAsync($":megaphone: {message}");

                }
                catch { }

            }

            await ReplyAsync("Done!");

        }

        [Command("reform")]
        [Summary("Re-initialize the cache")]
        public Task ReformCache()
        {
            
            _cache.Initialize();
            return Task.CompletedTask;

        }

        [Command("tell")]
        [Summary("Tell a specific channel on a guild something")]
        public async Task TellCommand([Remainder]string input)
        {

            var guild = Context.Client.Guilds.FirstOrDefault(g => g.Name == input);

            await ReplyAsync("Which channel would you like to use?");

            var response = await NextMessageAsync();
            var id = ulong.Parse(response.Content);
            var channel = guild.GetTextChannel(id);

            await ReplyAsync("What would you like to tell them?");

            response = await NextMessageAsync();

            await channel.SendMessageAsync(response.Content);

        }
    }
}
