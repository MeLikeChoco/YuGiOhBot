﻿using Newtonsoft.Json;

namespace YuGiOh.Bot.Models.Serializers
{
    public class GuildCount
    {

        [JsonProperty("shard_id")]
        public int ShardId { get; set; }

        [JsonProperty("shard_count")]
        public int ShardCount { get; set; }

        [JsonProperty("server_count")]
        public int ServerCount { get; set; }


        public GuildCount(int shardId, int shardCount, int count)
            => ServerCount = count;

    }
}