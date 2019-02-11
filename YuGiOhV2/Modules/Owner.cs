using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects;
using YuGiOhV2.Services;
using YuGiOhV2.Services.Microservices;

//The getinvite command can be intruding, however, I only use it to ask for feedback on the bot, else there is no other way to get 
//any sort of information on what I could improve or add. Sure, I have the feedback command, but no one uses it.

namespace YuGiOhV2.Modules
{
    [RequireOwner]
    public class Owner : CustomBase
    {

        public YgoDatabase YgoDatabase { get; set; }
        public Cache Cache { get; set; }
        public Web Web { get; set; }
        public Config Config { get; set; }

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
        public Task ReformCache(bool scrape = false)
        {

            if (scrape)
                YgoDatabase.ReformDatabase(new { Context.Client, Cache });
            else
                Cache.Initialize();

            return Task.CompletedTask;

        }

        [Command("reload")]
        [Summary("Reload the config")]
        public Task ReloadCommand()
        {

            Config.Reload();
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

        [Command("shutdown")]
        [Summary("Shutdown bot")]
        public async Task ShutdownCommand()
        {

            foreach (var id in Cache.GuessInProgress.Keys)
                await Context.Channel.SendMessageAsync("Stopping current guess game due to bot restarting!");

            Environment.Exit(0);

        }

    }
}
