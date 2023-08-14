using System;
using System.Threading.Tasks;
using Dommel;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models;

namespace YuGiOh.Common.Repositories
{
    public class BackupsRepository
    {

        private readonly IBackupsRepositoryConfiguration _config;

        public BackupsRepository(IBackupsRepositoryConfiguration config)
        {
            _config = config;
        }

        public async Task InsertYuGiOhBackupAsync(Guid guid)
        {

            await using var connection = _config.GetBackupsDbConnection();

            await connection.OpenAsync();
            await connection.InsertAsync(new Backup { Guid = guid });

        }

        public async Task InsertGuildsBackupAsync(Guid guid)
        {

            await using var connection = _config.GetBackupsDbConnection();

            await connection.OpenAsync();
            await connection.InsertAsync(new Backup { Guid = guid });

        }

    }
}