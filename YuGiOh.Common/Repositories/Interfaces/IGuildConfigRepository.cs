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

        Task InsertGuildConfigAsync(ulong id);
        Task<GuildConfig> GetGuildConfigAsync(ulong id);

    }
}
