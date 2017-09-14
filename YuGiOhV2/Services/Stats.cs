using Discord.WebSocket;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Objects.Serializers;

namespace YuGiOhV2.Services
{
    public class Stats
    {

        public int UniqueUserCount { get; set; }
        public string MaxGuild { get; set; }
        public int MaxGuildCount { get; set; }
        public int GuildCount { get; set; }
        public bool IsReady { get; set; }

        private bool _armedTimer;
        private Timer _calculateStats;
        private Timer _sendStats;
        private ulong _id;
        private Web _web;
        
        public Stats(DiscordSocketClient client, Web web)
        {

            _web = web;
            IsReady = false;
            _armedTimer = false;
            _id = client.CurrentUser.Id;
            _calculateStats = new Timer(CalculateStats, client, 0, 3600000);

        }
        
        private void CalculateStats(object state)
        {

            Log("Calculating stats...");

            var client = state as DiscordSocketClient;
            var guilds = client.Guilds;

            var userSet = new HashSet<ulong>(guilds.SelectMany(guild =>
            {

                guild.DownloadUsersAsync().GetAwaiter().GetResult();
                return guild.Users.Where(user => !user.IsBot);

            }).Select(user => user.Id));
                        
            var maxGuild = guilds.Where(guild => !guild.Name.Contains("Bot")).MaxBy(guild => guild.MemberCount);
            MaxGuild = maxGuild.Name;
            MaxGuildCount = maxGuild.MemberCount;
            UniqueUserCount = userSet.Count;
            GuildCount = guilds.Count;

            Log("Finished calculating stats.");

           IsReady = true;

            if (!_armedTimer)
            {

                _sendStats = new Timer(SendStats, null, 0, 3600000);
                _armedTimer = true;

            }

        }

        private async void SendStats(object state)
        {

            //indenting or not, no difference, but i like to think it helps :(
            var payload = JsonConvert.SerializeObject(new GuildCount(GuildCount), Formatting.Indented);

            try
            {

                Log("Sending stats to black discord bots...");
                await _web.Post($"https://bots.discord.pw/api/{_id}/stats", payload, await File.ReadAllTextAsync("Files/Bot List Tokens/Black.txt"));

            }
            catch
            {

                Log("Error in sending stats to black discord bots.");

            }

            try
            {

                Log("Sending stats to blue discord bots...");
                await _web.Post($"https://discordbots.org/api/{_id}/stats", payload, await File.ReadAllTextAsync("Files/Bot List Tokens/Blue.txt"));

            }
            catch
            {

                Log("Error in sending stats to blue discord bots.");

            }

        }

        private void Log(string message)
            => AltConsole.Print("Info", "Stats", message);

    }
}
