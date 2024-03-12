using Npgsql;

namespace YuGiOh.Common.Interfaces;

public interface IBackupsRepositoryConfiguration
{

    NpgsqlConnection GetBackupsDbConnection();

}