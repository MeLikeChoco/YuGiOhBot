using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YuGiOh.Scraper
{
    public class Config
    {

        public Databases Databases { get; set; }
        [JsonPropertyName("Retry Delay")]
        public TimeSpan RetryDelay { get; set; }
        [JsonPropertyName("Max Retry")]
        public int MaxRetry { get; set; }
        [JsonPropertyName("Hash Seed")]
        public uint HashSeed { get; set; }

    }

    public class Databases
    {

        public DatabaseInfo Staging { get; set; }
        public DatabaseInfo Production { get; set; }

    }

    public class DatabaseInfo
    {

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }


    }
}
