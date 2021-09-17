using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Common.Models;

namespace YuGiOh.Common.Repositories.Interfaces
{
    public interface IGuildConfigRepository
    {

        Task InsertGuildConfigAsync(GuildConfigEntity guildConfig);
        Task UpdateGuildConfigAsync(GuildConfigEntity guildConfig);
        Task<GuildConfigEntity> GetGuildConfigAsync(string id);
        Task<bool> GuildConfigExistsAsync(string id);

    }
}
