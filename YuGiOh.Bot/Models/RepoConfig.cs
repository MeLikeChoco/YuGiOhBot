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

            var guildsDbConfig = _config.Databases.Guilds;

            var connectionStr = new NpgsqlConnectionStringBuilder
            {

                Host = guildsDbConfig.Host,
                Port = guildsDbConfig.Port,
                Username = guildsDbConfig.Username,
                Password = guildsDbConfig.Password,
                Database = "guilds"

            }.ToString();

            return new NpgsqlConnection(connectionStr);

        }

        public NpgsqlConnection GetYuGiOhDbConnection()
        {

            var yugiohDbConfig = _config.Databases.YuGiOh;

            var connectionStr = new NpgsqlConnectionStringBuilder
            {

                Host = yugiohDbConfig.Host,
                Port = yugiohDbConfig.Port,
                Username = yugiohDbConfig.Username,
                Password = yugiohDbConfig.Password,
                Database = "yugioh"

            }.ToString();

            return new NpgsqlConnection(connectionStr);

        }

    }
}
