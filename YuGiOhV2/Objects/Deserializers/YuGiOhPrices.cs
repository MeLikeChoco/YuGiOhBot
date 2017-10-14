using Newtonsoft.Json;
using System.Collections.Generic;

namespace YuGiOhV2.Objects.Deserializers
{
    public class Prices
    {

        [JsonProperty("high")]
        public double High { get; set; }
        [JsonProperty("low")]
        public double Low { get; set; }
        [JsonProperty("average")]
        public double Average { get; set; }

    }

    public class Data
    {

        [JsonProperty("prices")]
        public Prices Prices { get; set; }

    }

    public class PriceData
    {
        
        [JsonProperty("data")]
        public Data Data { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

    }

    public class Datum
    {

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("print_tag")]
        public string PrintTag { get; set; }
        [JsonProperty("rarity")]
        public string Rarity { get; set; }
        [JsonProperty("price_data")]
        public PriceData PriceData { get; set; }

    }

    public class YuGiOhPrices
    {

        [JsonProperty("data")]
        public List<Datum> Data { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }

    }
}
