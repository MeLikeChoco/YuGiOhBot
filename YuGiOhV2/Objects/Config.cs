using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects
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
        
        [JsonIgnore]
        public DateTime LastDatabaseUpdate
        {

            get
            {

                var dateStr = File.ReadAllText("LastDatabaseUpdate.txt");

                return DateTime.Parse(dateStr);

            }

            set
            {

                File.WriteAllText("LastDatabaseUpdate.txt", value.ToUniversalTime().ToString());

            }

        }

        [JsonIgnore]
        public static Config Instance
        {
            get
            {

                if(_instance == null)
                    _instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Files/Config.json"));

                return _instance;

            }
        }

        [JsonIgnore]
        private static Config _instance;

        public void Reload()
        {

            _instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Files/Config.json"));

            AltConsole.Write("Config", "Reload", "The new settings are:");

            foreach(var property in GetType().GetProperties())
                AltConsole.Write("Config", "Reload", $"{property}: {property.GetValue(this)}");

        }

    }
}
