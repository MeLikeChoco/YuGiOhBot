using Discord;
using Discord.WebSocket;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Adapter.Discord.Net;
using DiscordBotsList.Api.Objects;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly Timer _calculateStats;
        private Timer _sendStats;
        private ulong _id;
        private Web _web;
        private readonly DiscordNetDblApi _discordBotListApi;
        private IAdapter _submissionAdapter;

        public Stats(DiscordShardedClient client, Web web)
        {
            
            _web = web;
            IsReady = false;
            _armedTimer = false;
            _id = client.CurrentUser.Id;
            _discordBotListApi = DiscordNetDblUtils.CreateDblApi(client, File.ReadAllText("Files/Bot List Tokens/Blue.txt"));
            _submissionAdapter = new SubmissionAdapter(_discordBotListApi, client, TimeSpan.FromMinutes(5));
            _calculateStats = new Timer(CalculateStats, client, 0, 3600000);

            //lmao, because the constructor is actually missing an assignment, i have to do this

            _submissionAdapter.Log += Log;

        }

        private void CalculateStats(object state)
        {

            if(state is DiscordShardedClient client && client.Shards.All(socket => socket.ConnectionState == ConnectionState.Connected))
            {

                Log("Calculating stats...");

                var guilds = client.Guilds;
                var maxGuild = guilds.Where(guild => !guild.Name.Contains("Bot")).MaxBy(guild => guild.MemberCount).FirstOrDefault();
                MaxGuild = maxGuild.Name;
                MaxGuildCount = maxGuild.MemberCount;
                UniqueUserCount = guilds.Sum(guild => guild.MemberCount);
                GuildCount = guilds.Count;

                Log("Finished calculating stats.");

                IsReady = true;

                //SendStats(null);

                if (!_armedTimer && Environment.GetCommandLineArgs().ElementAtOrDefault(1)?.ToLower() != "true")
                {

                    _sendStats = new Timer(SendStats, client, 0, 3600000);
                    _armedTimer = true;

                }

            }

        }

        private async void SendStats(object state)
        {

            if(state is DiscordShardedClient client && client.Shards.All(socket => socket.ConnectionState == ConnectionState.Connected))
            {

                var payload = JsonConvert.SerializeObject(new GuildCount(GuildCount));

                try
                {

                    Log("Sending stats to black discord bots...");
                    //var response = await _web.Post($"https://bots.discord.pw/api/bots/{_id}/stats", $"{{\"server_count\": {GuildCount}}}", await File.ReadAllTextAsync("Files/Bot List Tokens/Black.txt"));
                    var response = await _web.Post($"https://bots.discord.pw/api/bots/{_id}/stats", payload, await File.ReadAllTextAsync("Files/Bot List Tokens/Black.txt"));
                    Log($"Status: {response.StatusCode}");

                }
                catch
                {

                    Log("Error in sending stats to black discord bots.");

                }

                try
                {

                    Log("Sending stats to blue discord bots...");
                    //var response = await _web.Post($"https://discordbots.org/api/bots/{_id}/stats", $"{{\"server_count\": {GuildCount}}}", await File.ReadAllTextAsync("Files/Bot List Tokens/Blue.txt"));
                    //var response = await _web.Post($"https://discordbots.org/api/bots/{_id}/stats", payload, await File.ReadAllTextAsync("Files/Bot List Tokens/Blue.txt"));
                    _submissionAdapter.RunAsync().GetAwaiter().GetResult();
                    Log($"Status: Sent stats to blue discord bots.");

                }
                catch
                {

                    Log("Error in sending stats to blue discord bots.");

                }

            }

        }

        //this is what happens when you forget to assign in the constructor
        //delete this later
        public class SubmissionAdapter : IAdapter
        {
            protected AuthDiscordBotListApi api;
            protected IDiscordClient client;
            protected TimeSpan updateTime;

            protected DateTime lastTimeUpdated;

            public SubmissionAdapter(AuthDiscordBotListApi api, IDiscordClient client, TimeSpan updateTime)
            {
                this.api = api;
                this.client = client;
                this.updateTime = updateTime;
            }

            public event Action<string> Log;

            public virtual async Task RunAsync()
            {
                if (DateTime.Now > lastTimeUpdated + updateTime)
                {
                    await api.UpdateStats(
                        (await client.GetGuildsAsync()).Count
                    );

                    lastTimeUpdated = DateTime.Now;
                    SendLog("Submitted stats to DiscordBotsList.org");
                }
            }

            public virtual void Start()
            {

            }

            public virtual void Stop()
            {
                throw new NotImplementedException();
            }

            protected void SendLog(string msg)
                => Log?.Invoke(msg);
        }


        private void Log(string message)
            => AltConsole.Write("Info", "Stats", message);

    }
}
