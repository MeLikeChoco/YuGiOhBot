using Npgsql;

namespace YuGiOh.Common.Interfaces
{
    public interface IGuildConfigConfiguration
    {

        NpgsqlConnection GetGuildConfigConnection();

    }
}