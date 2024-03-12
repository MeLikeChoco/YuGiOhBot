using Npgsql;

namespace YuGiOh.Common.Interfaces;

public interface IYuGiOhRepositoryConfiguration
{

    NpgsqlConnection GetYuGiOhDbConnection();

}