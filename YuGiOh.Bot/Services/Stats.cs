using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBotsList.Api;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using System.Text.Json;

namespace YuGiOh.Bot.Services
{
    public class Stats
    {

        public int UniqueUserCount { get; private set; }
        public string? MaxGuild { get; private set; }
        public int MaxGuildCount { get; private set; }
        public int GuildCount { get; private set; }
        public bool IsReady { get; private set; }

        private readonly ILogger _logger;
        private readonly Timer _calculateStats;
        private readonly ulong _id;
        private readonly Web _web;
        private readonly AuthDiscordBotListApi _topGG;

        public Stats(ILoggerFactory loggerFactory, DiscordShardedClient client, Web web)
        {

            _logger = loggerFactory.CreateLogger(nameof(Stats));
            _web = web;
            IsReady = false;
            _id = client.CurrentUser.Id;
            _topGG = new AuthDiscordBotListApi(_id, Config.Instance.Tokens.BotList.TopGG);
            _calculateStats = new Timer(CalculateStats!, client, TimeSpan.FromSeconds(60), TimeSpan.FromHours(1));

        }

        private async void CalculateStats(object state)
        {

            try
            {

                if (state is not DiscordShardedClient client || client.Shards.Any(socket => socket.ConnectionState != ConnectionState.Connected))
                    return;

                _logger.Info("Calculating stats...");

                var guilds = client.Guilds;
                var maxGuild = guilds.Where(guild => !guild.Name.Contains("Bot")).MaxBy(guild => guild.MemberCount)!;
                var uniqueUsers = new HashSet<ulong>();
                MaxGuild = maxGuild.Name;
                MaxGuildCount = maxGuild.MemberCount;
                GuildCount = guilds.Count;

                foreach (var guild in guilds)
                    await guild.GetUsersAsync().ForEachAsync(users => uniqueUsers.UnionWith(users.Select(user => user.Id)));

                UniqueUserCount = uniqueUsers.Count;

                _logger.Info("Finished calculating stats.");

                IsReady = true;

                //SendStats(null);

                if (!Config.Instance.IsTest)
                    SendStats(client);

                _calculateStats.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            }
            catch (NullReferenceException)
            {

                _logger.Info("Error calculating stats. Rerunning in 300 seconds.");
                _calculateStats.Change(TimeSpan.FromSeconds(300), TimeSpan.FromHours(1));

            }

        }

        private void SendStats(DiscordShardedClient client)
        {

            _ = SendStatsToBotsOnDiscordXyz(client);
            _ = SendStatsToDiscordBotsGG(client);
            _ = SendStatsToTopGG(client);

        }

        private Task SendStatsToBotsOnDiscordXyz(DiscordShardedClient client)
        {

            try
            {

                // var payload = JsonConvert.SerializeObject(new { guildCount = client.Guilds.Count });
                //
                // Log($"Sending stats to bots.ondiscord.xyz...");
                // var response = await _web.Post(string.Format(Constants.BotsOnDiscordXyzUrl, _id), payload, authorization: Config.Instance.Tokens.BotList.Black);
                // Log($"Status: ({response.StatusCode}) Sent stats to bots.ondiscord.xyz");

            }
            catch
            {
                _logger.Info("Error sending stats to bots.ondiscord.xyz.");
            }

            return Task.CompletedTask;

        }

        // ReSharper disable once InconsistentNaming
        private async Task SendStatsToDiscordBotsGG(DiscordShardedClient client)
        {

            try
            {

                var payload = JsonSerializer.Serialize(new { guildCount = client.Guilds.Count });
                // var payload = JsonConvert.SerializeObject(new { guildCount = client.Guilds.Count });

                _logger.Info("Sending stats to discord.bots.gg...");
                var response = await _web.Post(string.Format(Constants.DiscordBotsGG, _id), payload, authorization: Config.Instance.Tokens.BotList.DiscordBotsGG);
                _logger.Info($"Status: ({response.StatusCode}) Sent stats to discord.bots.gg.");

            }
            catch
            {
                _logger.Info("Error sending stats to discord.bots.gg.");
            }

        }

        // ReSharper disable once InconsistentNaming
        private async Task SendStatsToTopGG(DiscordShardedClient client)
        {

            try
            {

                var bot = await _topGG.GetMeAsync();

                _logger.Info("Sending stats to top.gg...");
                await bot.UpdateStatsAsync(client.Guilds.Count);
                _logger.Info("Status: Sent stats to top.gg.");

            }
            catch
            {
                _logger.Info("Error sending stats to top.gg.");
            }

        }

    }
}