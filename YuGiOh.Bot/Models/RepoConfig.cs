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

            var connectionStr = Constants.IsDebug ? _config.Databases.GuildsStaging : _config.Databases.GuildsProd;

            return new NpgsqlConnection(connectionStr);

        }

        public NpgsqlConnection GetYuGiOhDbConnection()
        {

            var connectionStr = Constants.IsDebug ? _config.Databases.YuGiOhStaging : _config.Databases.YuGiOhProd;

            return new NpgsqlConnection(connectionStr);

        }

    }
}