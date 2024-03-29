﻿using System.Collections.Concurrent;
using System.Threading.Tasks;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Bot.Services
{
    public class GuildConfigDbService : IGuildConfigDbService
    {

        private static readonly ConcurrentDictionary<ulong, GuildConfig> Cache = new();

        private readonly IGuildConfigRepository _repo;

        public GuildConfigDbService(IGuildConfigRepository repo)
            => _repo = repo;

        public async Task<GuildConfig> GetGuildConfigAsync(ulong id)
            => await GetGuildConfigValueTaskAsync(id);

        private async ValueTask<GuildConfig> GetGuildConfigValueTaskAsync(ulong id)
        {

            if (Cache.TryGetValue(id, out var guildConfig))
                return guildConfig;

            var entity = await _repo.GetGuildConfigAsync(id);
            guildConfig = entity.ToModel();
            Cache[id] = guildConfig;

            return guildConfig;

        }

        public Task InsertGuildConfigAsync(GuildConfig guildConfig)
            => _repo.InsertGuildConfigAsync(guildConfig.ToEntity());

        public Task UpdateGuildConfigAsync(GuildConfig guildConfig)
            => _repo.UpdateGuildConfigAsync(guildConfig.ToEntity());

        public Task<bool> GuildConfigDoesExistAsync(ulong id)
            => _repo.GuildConfigExistsAsync(id);

    }
}