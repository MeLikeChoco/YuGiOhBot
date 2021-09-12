using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dommel;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models;

namespace YuGiOh.Common.Repositories
{
    public class BackupsRepository
    {

        private IBackupsRepositoryConfiguration _config;

        public BackupsRepository(IBackupsRepositoryConfiguration config)
        {
            _config = config;
        }

        public async Task InsertBackupAsync(Guid guid)
        {

            using var connection = _config.GetBackupsDbConnection();

            await connection.OpenAsync();
            await connection.InsertAsync(new Backup { Guid = guid });
            await connection.CloseAsync();

        }

    }
}
