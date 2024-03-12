using System;
using System.Threading.Tasks;
using Dapper;
using Dommel;
using Npgsql;
using YuGiOh.Common.DatabaseMappers;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Common.Repositories
{
    public class GuildConfigRepository : IGuildConfigRepository
    {

        private readonly IGuildConfigConfiguration _config;

        static GuildConfigRepository()
        {

            // FluentMapper.Initialize(config =>
            // {
            //
            //     config
            //         .AddConvention<LowerCaseConvention>()
            //         .ForEntity<GuildConfigEntity>();
            //
            //     config.ForDommel();
            //
            //     DommelMapper.SetColumnNameResolver(new LowerCaseConvention());
            //     DommelMapper.AddSqlBuilder(typeof(NpgsqlConnection), new PostgresSqlBuilder());
            //
            // });

            DommelMapper.SetColumnNameResolver(LowerCaseConvention.Instance);
            DommelMapper.AddSqlBuilder(typeof(NpgsqlConnection), new PostgresSqlBuilder());

            AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);

        }

        public GuildConfigRepository(IGuildConfigConfiguration config)
        {
            _config = config;
        }

        public async Task<GuildConfigEntity> GetGuildConfigAsync(ulong id)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var guildConfig = await connection.QuerySingleAsync<GuildConfigEntity>("select * from configs where id = @id", new { id = (decimal)id }).ConfigureAwait(false);

            return guildConfig;

        }

        public async Task InsertGuildConfigAsync(GuildConfigEntity guildConfig)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.InsertAsync(guildConfig).ConfigureAwait(false);

        }

        public async Task UpdateGuildConfigAsync(GuildConfigEntity guildConfig)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.UpdateAsync(guildConfig).ConfigureAwait(false);

        }

        public async Task<bool> GuildConfigExistsAsync(ulong id)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var doesExist = await connection.ExecuteScalarAsync<bool>("select 1 from configs where id = @id", new { id = (decimal)id }).ConfigureAwait(false);

            return doesExist;

        }

    }
}