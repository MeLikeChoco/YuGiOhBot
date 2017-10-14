using Newtonsoft.Json;

namespace YuGiOhV2.Objects.Deserializers
{
    public class YgoCardArtPost
    {
        [JsonProperty("summary")]
        public string Name { get; set; }

        [JsonProperty("photos")]
        public Photos[] Photos { get; set; }
    }

    public class Photos
    {
        [JsonProperty("original_size")]
        public PhotoType OriginalSize { get; set; }
    }

    public class PhotoType
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}