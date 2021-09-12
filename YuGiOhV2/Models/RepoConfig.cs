using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using YuGiOh.Common.Interfaces;
using YuGiOhV2.Models;

namespace YuGiOh.Bot.Models
{
    public class RepoConfig : IYuGiOhRepositoryConfiguration
    {

        private readonly Config _config;

        public RepoConfig(Config config)
            => _config = config;

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
