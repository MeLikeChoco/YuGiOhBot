using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
