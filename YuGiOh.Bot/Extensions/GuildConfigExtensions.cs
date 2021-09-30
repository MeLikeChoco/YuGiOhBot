using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;
using YuGiOh.Common.Models;

namespace YuGiOh.Bot.Extensions
{
    public static class GuildConfigExtensions
    {

        public static GuildConfig ToModel(this GuildConfigEntity entity)
        {

            return new GuildConfig
            {

                Id = ulong.Parse(entity.Id),
                AutoDelete = entity.AutoDelete,
                GuessTime = entity.GuessTime,
                HangmanTime = entity.HangmanTime,
                Inline = entity.Inline,
                Minimal = entity.Minimal,
                Prefix = entity.Prefix

            };

        }

        public static GuildConfigEntity ToEntity(this GuildConfig guildConfig)
        {

            return new GuildConfigEntity
            {

                Id = guildConfig.Id.ToString(),
                AutoDelete = guildConfig.AutoDelete,
                GuessTime = guildConfig.GuessTime,
                Inline = guildConfig.Inline,
                Minimal = guildConfig.Minimal,
                Prefix = guildConfig.Prefix

            };

        }

    }
}
