using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhBot.Core;

namespace YuGiOhBot.Services
{
    public class DiscordBotsService
    {

        private Timer _postData;
        private HttpClient _http;
        private DiscordSocketClient _client;
        private const string PostDiscordBotsAddress = "https://bots.discord.pw/api/bots/293526797600030720/stats";
        private const string PostDiscordBotListAddress = "https://discordbots.org/api/bots/293526797600030720/stats";
        private readonly AuthenticationHeaderValue DiscordBotsAuth = new AuthenticationHeaderValue(File.ReadAllText("Tokens/DiscordBots.txt"));
        private readonly AuthenticationHeaderValue DiscordBotListAuth = new AuthenticationHeaderValue(File.ReadAllText("Tokens/DiscordBotList.txt"));

        public DiscordBotsService(DiscordSocketClient clientParams)
        {

            _client = clientParams;

            _http = new HttpClient();

            _postData = new Timer(async state =>
            {

                _http.DefaultRequestHeaders.Authorization = DiscordBotsAuth;
                await AltConsole.PrintAsync("Service", "Discord Bots", "Sending information payload to Discord Bots...");
                var payload = new StringContent($"{{\"server_count\": {_client.Guilds.Count}}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _http.PostAsync(PostDiscordBotsAddress, payload);
                await AltConsole.PrintAsync("Service", "Discord Bots", "Information payload sent.");
                await AltConsole.PrintAsync("Service", "Discord Bots", $"The status was: {response.StatusCode}");

                _http.DefaultRequestHeaders.Authorization = DiscordBotListAuth;
                await AltConsole.PrintAsync("Service", "Discord Bots", "Sending information payload to Discord Bot List...");
                payload = new StringContent($"{{\"server_count\": {_client.Guilds.Count}}}", Encoding.UTF8, "application/json");
                response = await _http.PostAsync(PostDiscordBotListAddress, payload);
                await AltConsole.PrintAsync("Service", "Discord Bots", "Information payload sent.");
                await AltConsole.PrintAsync("Service", "Discord Bots", $"The status was: {response.StatusCode}");

            }, null, 10000, 3600000);

        }

    }
}
