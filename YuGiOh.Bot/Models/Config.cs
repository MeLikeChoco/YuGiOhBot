using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;

namespace YuGiOh.Bot.Models
{
    public class Config
    {

        [JsonIgnore]
        [Option('s', "is_sub_proc", Default = false)]
        public bool IsSubProc { get; set; }

        [JsonPropertyName("Guild Invite")]
        public string GuildInvite { get; set; }

        [JsonPropertyName("Bot Invite")]
        public string BotInvite { get; set; }

        [JsonPropertyName("Feedback Channel")]
        public ulong FeedbackChannel { get; set; }

        [JsonPropertyName("Owner Id")]
        public ulong OwnerId { get; set; }

        [JsonPropertyName("Is Test")]
        public bool IsTest { get; set; }

        public Databases Databases { get; set; }
        public Tokens Tokens { get; set; }

        [JsonIgnore]
        public DateTime LastDatabaseUpdate
        {

            get
            {

                var dateStr = File.ReadAllText("Databases/LastScrape.txt");

                return DateTime.Parse(dateStr);

            }

            set => File.WriteAllText("Databases/LastScrape.txt", value.ToUniversalTime().ToString());

        }

        [JsonIgnore]
        public static Config Instance
        {
            get
            {

                if (_instance is not null)
                    return _instance;

                _instance = JsonSerializer.Deserialize<Config>(File.ReadAllText("Files/Config.json"));
                // _instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Files/Config.json"));

                Parser.Default
                    .ParseArguments<Config>(Environment.GetCommandLineArgs())
                    .WithParsed(config => _instance.IsSubProc = config.IsSubProc);

                return _instance;

            }
        }

        [JsonIgnore]
        private static Config _instance;

        public static void Reload()
        {
            
            _instance = JsonSerializer.Deserialize<Config>(File.ReadAllText("Files/Config.json"));
            // _instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Files/Config.json"));

            AltConsole.Write("Config", "Reload", "Config reloaded.");

        }

    }

    public class Databases
    {

        [JsonPropertyName("YuGiOh Staging")]
        public DatabaseConnectionConfig YuGiOhStaging { get; set; }

        [JsonPropertyName("Guilds Staging")]
        public DatabaseConnectionConfig GuildsStaging { get; set; }

        [JsonPropertyName("YuGiOh Prod")]
        public DatabaseConnectionConfig YuGiOhProd { get; set; }

        [JsonPropertyName("Guilds Prod")]
        public DatabaseConnectionConfig GuildsProd { get; set; }

    }

    public class DatabaseConnectionConfig
    {

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }

    }

    public class Tokens
    {

        public DiscordTokenConfig Discord { get; set; }

        [JsonPropertyName("Bot List")]
        public BotList BotList { get; set; }
        public string Bitly { get; set; }

    }

    public class DiscordTokenConfig
    {

        public string Legit { get; set; }
        public string Test { get; set; }

    }

    public class BotList
    {

        public string BotsOnDiscordXyz { get; set; }
        public string TopGG { get; set; }
        public string DiscordBotsGG { get; set; }

    }
}