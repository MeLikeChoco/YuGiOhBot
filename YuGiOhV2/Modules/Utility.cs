using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Utility : CustomBase
    {

        private Stats _stats;

        public Utility(Stats stats)
        {

            _stats = stats;

        }

        [Command("feedback")]
        public async Task FeedbackCommand([Remainder]string message)
        {
            
            var author = new EmbedAuthorBuilder()
                .WithName(Context.User.Username)
                .WithIconUrl(Context.User.GetAvatarUrl());

            var footer = new EmbedFooterBuilder()
                .WithText($"{Context.Guild.Name} | {Context.Guild.Id}")
                .WithIconUrl(Context.Guild.IconUrl);

            var body = new EmbedBuilder()
                .WithAuthor(author)
                .WithFooter(footer)
                .WithColor(new Color(Rand.NextInt(256), Rand.NextInt(256), Rand.NextInt(256)))
                .WithDescription(message);

            await (Context.Client.GetChannel(296117398132752384) as SocketTextChannel).SendMessageAsync("", embed: body.Build());

        }

        [Command("invite")]
        public async Task InviteCommand()
        {

            var id = Context.Client.GetApplicationInfoAsync().Result.Id;

            await ReplyAsync($"{Context.User.Mention} <https://discordapp.com/oauth2/authorize?client_id={id}&scope=bot&permissions=0>");

        }

        [Command("uptime")]
        public async Task UptimeCommand() 
            => await ReplyAsync(GetUptime());

        [Command("stats")]
        public async Task StatsCommand()
        {

            if (_stats.IsReady)
            {

                var bot = Context.Client.CurrentUser;

                var author = new EmbedAuthorBuilder()
                    .WithIconUrl(bot.GetAvatarUrl())
                    .WithName("Statistics");

                var desc = $"This bot is present on **{_stats.GuildCount}** guilds.\n" +
                    $"**{_stats.MaxGuild}** is the largest guild with **{_stats.MaxGuildCount}** users.\n" +
                    $"**{_stats.UniqueUserCount}** users are in the same guild as this bot.\n" +
                    $"{GetUptime()}";

                var body = new EmbedBuilder()
                    .WithAuthor(author)
                    .WithColor(new Color(Rand.NextInt(256), Rand.NextInt(256), Rand.NextInt(256)))
                    .WithDescription(desc);

                await ReplyAsync("", embed: body.Build());

            }
            else
                await ReplyAsync("Statistics are still being calculated.");

        }

        private string GetUptime()
        {

            var time = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var str = "The bot has been up for ";

            if (time.Days != 0)
                str += $"**{time.Days}** days, ";

            if (time.Hours != 0)
                str += $"**{time.Hours}** hours, ";

            if (time.Minutes != 0)
                str += $"**{time.Minutes}** minutes, ";

            if (time.Seconds != 0)
                str += $"**{time.Seconds}** seconds.";

            return str;

        }

    }
}
