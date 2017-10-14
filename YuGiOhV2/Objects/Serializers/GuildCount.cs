using Newtonsoft.Json;

namespace YuGiOhV2.Objects.Serializers
{
    public class GuildCount
    {

        [JsonProperty("server_count")]
        public int ServerCount { get; set; }

        public GuildCount(int count)
            => ServerCount = count;

    }
}
