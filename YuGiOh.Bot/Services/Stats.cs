using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBotsList.Api;
using Newtonsoft.Json;
using YuGiOh.Bot.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace YuGiOh.Bot.Services
{
    public class Stats
    {

        public int UniqueUserCount { get; set; }
        public string MaxGuild { get; set; }
        public int MaxGuildCount { get; set; }
        public int GuildCount { get; set; }
        public bool IsReady { get; set; }

        private readonly Timer _calculateStats;
        private readonly ulong _id;
        private Web _web;
        private readonly AuthDiscordBotListApi _topGG;

        public Stats(DiscordShardedClient client, Web web)
        {

            _web = web;
            IsReady = false;
            _id = client.CurrentUser.Id;
            _topGG = new AuthDiscordBotListApi(_id, Config.Instance.Tokens.BotList.TopGG);
            _calculateStats = new Timer(CalculateStats, client, TimeSpan.FromSeconds(300), TimeSpan.FromHours(1));

        }

        private void CalculateStats(object state)
        {

            try
            {

                if (state is not DiscordShardedClient client || client.Shards.Any(socket => socket.ConnectionState != ConnectionState.Connected))
                    return;

                Log("Calculating stats...");

                var guilds = client.Guilds;
                var maxGuild = guilds.Where(guild => !guild.Name.Contains("Bot")).MaxBy(guild => guild.MemberCount);
                MaxGuild = maxGuild.Name;
                MaxGuildCount = maxGuild.MemberCount;
                UniqueUserCount = guilds.Sum(guild => guild.MemberCount);
                GuildCount = guilds.Count;

                Log("Finished calculating stats.");

                IsReady = true;

                //SendStats(null);

                if (!Config.Instance.IsTest)
                    SendStats(client);

                _calculateStats.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            }
            catch (NullReferenceException)
            {

                AltConsole.Write("Stats", "Error", "Error calculating stats. Rerunning in 300 seconds.");
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
                Log("Error sending stats to bots.ondiscord.xyz.");
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

                Log("Sending stats to discord.bots.gg...");
                var response = await _web.Post(string.Format(Constants.DiscordBotsGG, _id), payload, authorization: Config.Instance.Tokens.BotList.DiscordBotsGG);
                Log($"Status: ({response.StatusCode}) Sent stats to discord.bots.gg.");

            }
            catch
            {
                Log("Error sending stats to discord.bots.gg.");
            }

        }

        // ReSharper disable once InconsistentNaming
        private async Task SendStatsToTopGG(DiscordShardedClient client)
        {

            try
            {

                var bot = await _topGG.GetMeAsync();

                Log("Sending stats to top.gg...");
                await bot.UpdateStatsAsync(client.Guilds.Count);
                Log("Status: Sent stats to top.gg.");

            }
            catch
            {
                Log("Error sending stats to top.gg.");
            }

        }


        private static void Log(string message)
            => AltConsole.Write("Info", "Stats", message);

    }
}