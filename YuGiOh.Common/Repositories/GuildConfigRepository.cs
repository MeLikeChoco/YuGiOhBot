using System.Threading.Tasks;
using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using Dommel;
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

            FluentMapper.Initialize(config =>
            {

                config
                    .AddConvention<LowerCaseConvention>()
                    .ForEntity<GuildConfigEntity>();

                config.ForDommel();

                DommelMapper.SetColumnNameResolver(new LowerCaseConvention());

            });

        }

        public GuildConfigRepository(IGuildConfigConfiguration config)
        {
            _config = config;
        }

        public async Task<GuildConfigEntity> GetGuildConfigAsync(string id)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var guildConfig = await connection.QuerySingleAsync<GuildConfigEntity>("select * from configs where id = @id", new { id }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return guildConfig;

        }

        public async Task InsertGuildConfigAsync(GuildConfigEntity guildConfig)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.InsertAsync(guildConfig).ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);

        }

        public async Task UpdateGuildConfigAsync(GuildConfigEntity guildConfig)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.UpdateAsync(guildConfig).ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);

        }

        public async Task<bool> GuildConfigExistsAsync(string id)
        {

            await using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var doesExist = await connection.ExecuteScalarAsync<bool>("select 1 from configs where id = @id", new { id }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return doesExist;

        }

    }
}