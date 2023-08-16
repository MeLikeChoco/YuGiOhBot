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

                Id = (ulong) entity.Id,
                AutoDelete = entity.AutoDelete,
                GuessTime = entity.GuessTime,
                HangmanTime = entity.HangmanTime,
                Inline = entity.Inline,
                Minimal = entity.Minimal,
                Prefix = entity.Prefix,
                HangmanAllowWords = entity.HangmanAllowWords,

            };

        }

        public static GuildConfigEntity ToEntity(this GuildConfig guildConfig)
        {

            return new GuildConfigEntity
            {

                Id = guildConfig.Id,
                AutoDelete = guildConfig.AutoDelete,
                GuessTime = guildConfig.GuessTime,
                HangmanTime = guildConfig.HangmanTime,
                Inline = guildConfig.Inline,
                Minimal = guildConfig.Minimal,
                Prefix = guildConfig.Prefix,
                HangmanAllowWords = guildConfig.HangmanAllowWords,

            };

        }

    }
}