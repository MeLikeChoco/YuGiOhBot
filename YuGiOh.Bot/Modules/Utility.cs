using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules
{
    public class Utility : CustomBase
    {

        public Stats Stats { get; set; }
        public Random Rand { get; set; }
        public Config Config { get; set; }
        public IPerformanceMetrics PerfMetrics { get; set; }

        [Command("feedback")]
        [Summary("Send feedback to the bot owner!")]
        public async Task FeedbackCommand([Remainder] string message)
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
        public Task InviteCommand()
            => ReplyAsync($"{Context.User.Mention} <{Config.BotInvite}>");

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

                await SendEmbedAsync(body);

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
        public async Task InfoCommand()
        {

            using (Context.Channel.EnterTypingState())
            {

                var getAppInfoTask = Context.Client.GetApplicationInfoAsync();
                var getOSInfoTask = PerfMetrics.GetOperatingSystem();
                var calcCpuUsageTask = PerfMetrics.GetCpuUsage();
                var calcMemUsageTask = PerfMetrics.GetMemUsage();

                var strBuilder = new StringBuilder()
                    .Append("**Discord API Version:** ").Append(DiscordConfig.APIVersion).AppendLine()
                    .Append("**Discord.NET Version:** ").AppendLine(DiscordConfig.Version);

                var appInfo = await getAppInfoTask;

                strBuilder
                    .Append("**Owner/Developer:** ").AppendLine(appInfo.Owner.ToString())
                    .Append("**Shards:** ").Append(Context.Client.Shards.Count).AppendLine();

                var osInfo = await getOSInfoTask;

                strBuilder
                    .Append("**Operating System:** ").AppendLine(osInfo)
                    .Append("**Processor Count:** ").Append(Environment.ProcessorCount).AppendLine();

                var cpuUsage = await calcCpuUsageTask;
                var memUsage = await calcMemUsageTask;
                var usedMem = Math.Round(memUsage.UsedMem, 2);
                var totalMem = Math.Round(memUsage.TotalMem, 2);

                strBuilder.Append("**Cpu Usage:** ").Append(cpuUsage.ToString("0.##")).AppendLine("%");
                strBuilder.Append("**Memory Usage:** ").Append(usedMem.ToString("0.##")).Append(" GB / ").Append(totalMem.ToString("0.##")).AppendLine(" GB");

                var body = new EmbedBuilder()
                    .WithRandomColor()
                    .WithDescription(strBuilder.ToString());

                await SendEmbedAsync(body);

            }

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
