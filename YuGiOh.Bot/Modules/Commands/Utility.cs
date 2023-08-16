using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Commands
{
    public class Utility : MainBase
    {

        private readonly Stats _stats;
        private readonly Config _config;
        private readonly IPerformanceMetrics _perfMetrics;

        public Utility(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand,
            InteractiveService interactiveService,
            Stats stats,
            Config config,
            IPerformanceMetrics perfMetrics
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, rand, interactiveService)
        {
            _stats = stats;
            _config = config;
            _perfMetrics = perfMetrics;
        }

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

            await (Context.Client.GetChannel(_config.FeedbackChannel) as SocketTextChannel).SendMessageAsync("", embed: body.Build());
            await ReplyAsync($"Feedback was sent!\n**Support guild/server: <{_config.GuildInvite}>**");

        }

        [Command("invite")]
        [Summary("Gets an invite to the bot!")]
        public Task InviteCommand()
            => ReplyAsync($"{Context.User.Mention} <{_config.BotInvite}>");

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
                var getOSInfoTask = _perfMetrics.GetOperatingSystem();
                var calcCpuUsageTask = _perfMetrics.GetCpuUsage();
                var calcMemUsageTask = _perfMetrics.GetMemUsage();

                var strBuilder = new StringBuilder()
                    .Append("**Discord API Version:** ")
                    .Append(DiscordConfig.APIVersion)
                    .AppendLine()
                    .Append("**Discord.NET Version:** ")
                    .AppendLine(DiscordConfig.Version);

                var appInfo = await getAppInfoTask;

                strBuilder
                    .Append("**Owner/Developer:** ")
                    .AppendLine(appInfo.Owner.ToString())
                    .Append("**Shards:** ")
                    .Append(Context.Client.Shards.Count)
                    .AppendLine();

                var osInfo = await getOSInfoTask;

                strBuilder
                    .Append("**Operating System:** ")
                    .AppendLine(osInfo)
                    .Append("**Processor Count:** ")
                    .Append(Environment.ProcessorCount)
                    .AppendLine();

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