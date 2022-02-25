using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using YuGiOh.Common.Interfaces;
using YuGiOh.Bot.Models;

namespace YuGiOh.Bot.Models
{
    public class RepoConfig : IYuGiOhRepositoryConfiguration, IGuildConfigConfiguration
    {

        private readonly Config _config;

        public RepoConfig(Config config)
            => _config = config;

        public NpgsqlConnection GetGuildConfigConnection()
        {

            var guildsDbConfig = Constants.IsDebug ? _config.Databases.GuildsStaging : _config.Databases.GuildsProd;

            var connectionStr = new NpgsqlConnectionStringBuilder
            {

                Host = guildsDbConfig.Host,
                Port = guildsDbConfig.Port,
                Username = guildsDbConfig.Username,
                Password = guildsDbConfig.Password,
                Database = guildsDbConfig.Database

            }.ToString();

            return new NpgsqlConnection(connectionStr);

        }

        public NpgsqlConnection GetYuGiOhDbConnection()
        {

            var yugiohDbConfig = Constants.IsDebug ? _config.Databases.YuGiOhStaging : _config.Databases.YuGiOhProd;

            var connectionStr = new NpgsqlConnectionStringBuilder
            {

                Host = yugiohDbConfig.Host,
                Port = yugiohDbConfig.Port,
                Username = yugiohDbConfig.Username,
                Password = yugiohDbConfig.Password,
                Database = yugiohDbConfig.Database

            }.ToString();

            return new NpgsqlConnection(connectionStr);

        }

    }
}
