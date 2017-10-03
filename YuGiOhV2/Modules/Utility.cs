using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Utility : CustomBase
    {

        private Stats _stats;
        private Random _rand;

        public Utility(Stats stats, Random rand)
        {

            _stats = stats;
            _rand = rand;

        }

        [Command("feedback")]
        [Summary("Send feedback to the bot owner!")]
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
                .WithColor(_rand.GetColor())
                .WithDescription(message);

            await (Context.Client.GetChannel(296117398132752384) as SocketTextChannel).SendMessageAsync("", embed: body.Build());

        }

        [Command("invite")]
        [Summary("Gets an invite to the bot!")]
        public async Task InviteCommand()
        {

            var id = Context.Client.GetApplicationInfoAsync().Result.Id;

            await ReplyAsync($"{Context.User.Mention} <https://discordapp.com/oauth2/authorize?client_id={id}&scope=bot&permissions=0>");
            
        }

        [Command("uptime")]
        [Summary("Gets the uptime of the bot!")]
        public async Task UptimeCommand() 
            => await ReplyAsync(GetUptime());

        [Command("stats")]
        [Summary("Gets the statistics of the bot!")]
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
                    .WithColor(_rand.GetColor())
                    .WithDescription(desc);

                await SendEmbed(body);

            }
            else
                await ReplyAsync("Statistics are still being calculated.");

        }

        [Command("ping")]
        [Summary("Get the latency of the bot to the gateway!")]
        public Task PingCommand()
            => ReplyAsync($"**{Context.Client.Latency}ms**");

        [Command("info")]
        [Summary("Get info on the bot!")]
        public Task InfoCommand()
        {

            var body = new EmbedBuilder()
                .WithColor(_rand.GetColor())
                .WithDescription($"**Discord API Version:** {DiscordConfig.APIVersion}\n" +
                $"**Operating System:** {Environment.OSVersion.VersionString}\n" +
                $"**Processor Count:** {Environment.ProcessorCount}\n");

            return SendEmbed(body);

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
