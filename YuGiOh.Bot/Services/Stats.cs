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
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Serializers;

namespace YuGiOh.Bot.Services
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
        private readonly ulong _id;
        private Web _web;
        private readonly AuthDiscordBotListApi _topGG;

        public Stats(DiscordShardedClient client, Web web)
        {

            _web = web;
            IsReady = false;
            _armedTimer = false;
            _id = client.CurrentUser.Id;
            _topGG = new AuthDiscordBotListApi(_id, Config.Instance.Tokens.BotList.Blue);
            _calculateStats = new Timer(CalculateStats, client, TimeSpan.FromSeconds(300), TimeSpan.FromHours(1));

        }

        private void CalculateStats(object state)
        {

            try
            {

                if (state is DiscordShardedClient client && client.Shards.All(socket => socket.ConnectionState == ConnectionState.Connected))
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

                    if (!Environment.GetCommandLineArgs().Contains("test"))
                        SendStats(client);

                    _calculateStats.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

                }

            }
            catch (NullReferenceException)
            {

                AltConsole.Write("Stats", "Error", "Error calculating stats. Rerunning in 300 seconds.");
                _calculateStats.Change(TimeSpan.FromSeconds(300), TimeSpan.FromHours(1));

            }

        }

        private async void SendStats(DiscordShardedClient client)
        {

            var payload = JsonConvert.SerializeObject(new { guildCount = client.Guilds.Count });

            Log($"Sending stats to black discord bots...");
            //var response = await _web.Post($"https://bots.discord.pw/api/bots/{_id}/stats", $"{{\"server_count\": {GuildCount}}}", await File.ReadAllTextAsync("Files/Bot List Tokens/Black.txt"));
            var response = await _web.Post(string.Format(Constants.BlackDiscordBotUrl, _id), payload, authorization: Config.Instance.Tokens.BotList.Black);
            Log($"Status: {response.StatusCode}");

            try
            {

                var bot = await _topGG.GetMeAsync();

                Log("Sending stats to top.gg...");
                //var response = await _web.Post($"https://discordbots.org/api/bots/{_id}/stats", $"{{\"server_count\": {GuildCount}}}", await File.ReadAllTextAsync("Files/Bot List Tokens/Blue.txt"));
                //var response = await _web.Post($"https://discordbots.org/api/bots/{_id}/stats", payload, await File.ReadAllTextAsync("Files/Bot List Tokens/Blue.txt"));
                await bot.UpdateStatsAsync(client.Guilds.Count);
                Log("Status: Sent stats to top.gg.");

            }
            catch
            {

                Log("Error in sending stats to blue discord bots.");

            }

        }

        ////this is what happens when you forget to assign in the constructor
        ////delete this later
        //public class SubmissionAdapter : IAdapter
        //{
        //    protected AuthDiscordBotListApi api;
        //    protected IDiscordClient client;
        //    protected TimeSpan updateTime;

        //    protected DateTime lastTimeUpdated;

        //    public SubmissionAdapter(AuthDiscordBotListApi api, IDiscordClient client, TimeSpan updateTime)
        //    {
        //        this.api = api;
        //        this.client = client;
        //        this.updateTime = updateTime;
        //    }

        //    public event Action<string> Log;

        //    public virtual async Task RunAsync()
        //    {
        //        if (DateTime.Now > lastTimeUpdated + updateTime)
        //        {
        //            await api.UpdateStats(
        //                (await client.GetGuildsAsync()).Count
        //            );

        //            lastTimeUpdated = DateTime.Now;
        //            SendLog("Submitted stats to DiscordBotsList.org");
        //        }
        //    }

        //    public virtual void Start()
        //    {

        //    }

        //    public virtual void Stop()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    protected void SendLog(string msg)
        //        => Log?.Invoke(msg);
        //}


        private void Log(string message)
            => AltConsole.Write("Info", "Stats", message);

    }
}
