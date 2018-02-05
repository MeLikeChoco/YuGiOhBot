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

            AltConsole.Print("Config", "Reload", "The new settings are:");

            foreach(var property in GetType().GetProperties())
                AltConsole.Print("Config", "Reload", $"{property}: {property.GetValue(this)}");

        }

    }
}
