using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;

namespace YuGiOh.Bot.Services.Interfaces
{
    public interface IGuildConfigDbService
    {

        Task<GuildConfig> GetGuildConfigAsync(ulong id);
        Task InsertGuildConfigAsync(GuildConfig guildConfig);
        Task UpdateGuildConfigAsync(GuildConfig guildConfig);
        Task<bool> GuildConfigDoesExistAsync(ulong id);

    }
}
