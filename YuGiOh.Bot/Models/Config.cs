using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using Serilog;
using LoggerExtensions = YuGiOh.Bot.Extensions.LoggerExtensions;

namespace YuGiOh.Bot.Models
{
    public class Config
    {

        private static readonly ILogger Logger = LoggerExtensions.CreateStaticLogger(nameof(Config));

        [JsonIgnore]
        [Option('s', "is_sub_proc", Default = false)]
        public bool IsSubProc { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Logging Templates")]
        public LoggingTemplates LoggingTemplates { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Log File Path")]
        public string LogFilePath { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Guild Invite")]
        public string GuildInvite { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Bot Invite")]
        public string BotInvite { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Feedback Channel")]
        public ulong FeedbackChannel { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Owner Id")]
        public ulong OwnerId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Is Test")]
        public bool IsTest { get; private set; }

        [JsonInclude]
        public Databases Databases { get; private set; }

        [JsonInclude]
        public Tokens Tokens { get; private set; }

        private static Config _instance;

        [JsonIgnore]
        public static Config Instance
        {
            get
            {

                if (_instance is not null)
                    return _instance;

                _instance = JsonSerializer.Deserialize<Config>(File.ReadAllText("Files/Config.json")) ?? throw new ConfigurationErrorsException();

                Parser.Default
                    .ParseArguments<Config>(Environment.GetCommandLineArgs())
                    .WithParsed(config => _instance.IsSubProc = config.IsSubProc);

                return _instance;

            }
        }

        public static void Reload()
        {

            _instance = JsonSerializer.Deserialize<Config>(File.ReadAllText("Files/Config.json"));
            // _instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Files/Config.json"));

            Logger.Information("Config reloaded.");

        }

    }

    public class LoggingTemplates
    {

        [JsonInclude]
        public string Console { get; private set; }

        [JsonInclude]
        public string File { get; private set; }

    }

    public class Databases
    {

        [JsonInclude]
        [JsonPropertyName("YuGiOh Staging")]
        public string YuGiOhStaging { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Guilds Staging")]
        public string GuildsStaging { get; private set; }

        [JsonInclude]
        [JsonPropertyName("YuGiOh Prod")]
        public string YuGiOhProd { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Guilds Prod")]
        public string GuildsProd { get; private set; }

    }

    public class Tokens
    {

        [JsonInclude]
        public DiscordTokenConfig Discord { get; private set; }

        [JsonInclude]
        [JsonPropertyName("Bot List")]
        public BotList BotList { get; private set; }

        [JsonInclude]
        public string Bitly { get; private set; }

    }

    public class DiscordTokenConfig
    {

        [JsonInclude]
        public string Legit { get; private set; }

        [JsonInclude]
        public string Test { get; private set; }

    }

    public class BotList
    {

        [JsonInclude]
        public string BotsOnDiscordXyz { get; private set; }

        [JsonInclude]
        public string TopGG { get; private set; }

        [JsonInclude]
        public string DiscordBotsGG { get; private set; }

    }
}