﻿using System;
using System.IO;
using CommandLine;
using Newtonsoft.Json;

namespace YuGiOh.Bot.Models
{
    public class Config
    {

        [JsonProperty("Guild Invite")]
        public string GuildInvite { get; set; }
        [JsonProperty("Bot Invite")]
        public string BotInvite { get; set; }
        [JsonProperty("Feedback Channel")]
        public ulong FeedbackChannel { get; set; }
        [JsonProperty("Owner Id")]
        public ulong OwnerId { get; set; }
        [JsonProperty("Is Test")]
        public bool IsTest { get; set; }
        [JsonIgnore]
        [Option('s', "is_sub_proc", Default = false)]
        public bool IsSubProc { get; set; }
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

            set
            {

                File.WriteAllText("Databases/LastScrape.txt", value.ToUniversalTime().ToString());

            }

        }

        [JsonIgnore]
        public static Config Instance
        {
            get
            {

                if (_instance is null)
                {

                    _instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Files/Config.json"));

                    Parser.Default
                        .ParseArguments<Config>(Environment.GetCommandLineArgs())
                        .WithParsed(config => _instance.IsSubProc = config.IsSubProc);

                }

                return _instance;

            }
        }

        [JsonIgnore]
        private static Config _instance;

        public static void Reload()
        {

            _instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Files/Config.json"));

            AltConsole.Write("Config", "Reload", "Config reloaded.");

        }

    }

    public class Databases
    {

        public Database YuGiOh { get; set; }
        public Database Guilds { get; set; }

    }

    public class Database
    {

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }

    public class Tokens
    {

        public Discord Discord { get; set; }
        [JsonProperty("Bot List")]
        public BotList BotList { get; set; }

    }

    public class Discord
    {

        public string Legit { get; set; }
        public string Test { get; set; }

    }

    public class BotList
    {

        public string Black { get; set; }
        public string Blue { get; set; }

    }
}