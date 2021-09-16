using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.FluentMap.Mapping;
using YuGiOh.Common.Models;

namespace YuGiOh.Common.DatabaseMappers
{
    internal class GuildConfigEntityMapper : EntityMap<GuildConfigEntity>
    {

        public GuildConfigEntityMapper()
        {

            Map(entity => entity.GuessTime)
                .ToColumn("guess_time");

        }

    }
}
