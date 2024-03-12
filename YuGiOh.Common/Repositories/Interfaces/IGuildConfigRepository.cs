using System.Threading.Tasks;
using YuGiOh.Common.Models;

namespace YuGiOh.Common.Repositories.Interfaces;

public interface IGuildConfigRepository
{

    Task InsertGuildConfigAsync(GuildConfigEntity guildConfig);
    Task UpdateGuildConfigAsync(GuildConfigEntity guildConfig);
    Task<GuildConfigEntity> GetGuildConfigAsync(ulong id);
    Task<bool> GuildConfigExistsAsync(ulong id);

}