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
using YuGiOhV2.Models;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Utility : CustomBase
    {

        public Stats Stats { get; set; }
        public Random Rand { get; set; }
        public Config Config { get; set; }

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
                .WithRandomColor()
                .WithDescription(message);

            await (Context.Client.GetChannel(Config.FeedbackChannel) as SocketTextChannel).SendMessageAsync("", embed: body.Build());
            await ReplyAsync($"Feedback was sent!\n**Support guild/server: <{Config.GuildInvite}>**");

        }

        [Command("invite")]
        [Summary("Gets an invite to the bot!")]
        public async Task InviteCommand()
        {

            var id = Context.Client.GetApplicationInfoAsync().Result.Id;

            await ReplyAsync($"{Context.User.Mention} <{Config.BotInvite}>");
            
        }

        [Command("uptime")]
        [Summary("Gets the uptime of the bot!")]
        public async Task UptimeCommand() 
            => await ReplyAsync(GetUptime());

        [Command("stats")]
        [Summary("Gets the statistics of the bot!")]
        public async Task StatsCommand()
        {

            if (Stats.IsReady)
            {

                var bot = Context.Client.CurrentUser;

                var author = new EmbedAuthorBuilder()
                    .WithIconUrl(bot.GetAvatarUrl())
                    .WithName("Statistics");

                var desc = $"This bot is present on **{Stats.GuildCount}** guilds.\n" +
                    $"**{Stats.MaxGuild}** is the largest guild with **{Stats.MaxGuildCount}** users.\n" +
                    $"**{Stats.UniqueUserCount}** users are in the same guild as this bot.\n" +
                    $"{GetUptime()}";

                var body = new EmbedBuilder()
                    .WithAuthor(author)
                    .WithRandomColor()
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
                .WithRandomColor()
                .WithDescription($"**Discord API Version:** {DiscordConfig.APIVersion}\n" +
                $"**Operating System:** {Environment.OSVersion.VersionString}\n" +
                $"**Processor Count:** {Environment.ProcessorCount}\n" +
                $"**Shards:** {Context.Client.Shards.Count}\n");

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
