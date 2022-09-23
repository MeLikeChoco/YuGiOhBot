using Npgsql;
using YuGiOh.Common.Interfaces;

namespace YuGiOh.Bot.Models
{
    public class RepoConfig : IYuGiOhRepositoryConfiguration, IGuildConfigConfiguration
    {

        private readonly Config _config;

        public RepoConfig(Config config)
            => _config = config;

        public NpgsqlConnection GetGuildConfigConnection()
        {

            var connectionStr = _config.GetDbConnectionStrings().Guilds;

            return new NpgsqlConnection(connectionStr);

        }

        public NpgsqlConnection GetYuGiOhDbConnection()
        {

            var connectionStr = _config.GetDbConnectionStrings().YuGiOh;

            return new NpgsqlConnection(connectionStr);

        }

    }
}