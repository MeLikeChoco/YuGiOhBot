using Newtonsoft.Json;

namespace YuGiOhV2.Objects.Serializers
{
    public class GuildCount
    {
        public GuildCount(int count)
        {
            ServerCount = count;
        }

        [JsonProperty("server_count")]
        public int ServerCount { get; set; }
    }
}