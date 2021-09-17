using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Bot.Services
{
    public class GuildConfigDbService : IGuildConfigDbService
    {

        private readonly IGuildConfigRepository _repo;

        public GuildConfigDbService(IGuildConfigRepository repo)
            => _repo = repo;

        public async Task<GuildConfig> GetGuildConfigAsync(ulong id)
        {

            var guildConfig = await _repo.GetGuildConfigAsync(id.ToString());

            return guildConfig.ToModel();

        }

        public Task InsertGuildConfigAsync(GuildConfig guildConfig)
            => _repo.InsertGuildConfigAsync(guildConfig.ToEntity());

        public Task UpdateGuildConfigAsync(GuildConfig guildConfig)
            => _repo.UpdateGuildConfigAsync(guildConfig.ToEntity());

        public Task<bool> GuildConfigDoesExistAsync(ulong id)
            => _repo.GuildConfigExistsAsync(id.ToString());

    }
}
