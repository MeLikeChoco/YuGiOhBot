using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace YuGiOh.Common.Interfaces
{
    public interface IBackupsRepositoryConfiguration
    {

        NpgsqlConnection GetBackupsDbConnection();

    }
}
