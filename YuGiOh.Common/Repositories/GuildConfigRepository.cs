using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync();

            var guildConfig = await connection.QuerySingleAsync<GuildConfigEntity>("select * from configs where id = @id", new { id });

            await connection.CloseAsync();

            return guildConfig;

        }

        public async Task InsertGuildConfigAsync(GuildConfigEntity guildConfig)
        {

            using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync();
            await connection.InsertAsync(guildConfig);
            await connection.CloseAsync();

        }

        public async Task UpdateGuildConfigAsync(GuildConfigEntity guildConfig)
        {

            using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync();
            await connection.UpdateAsync(guildConfig);
            await connection.CloseAsync();

        }

        public async Task<bool> GuildConfigExistsAsync(string id)
        {

            using var connection = _config.GetGuildConfigConnection();

            await connection.OpenAsync();

            var doesExist = await connection.ExecuteScalarAsync<bool>("select 1 from configs where id = @id", new { id });

            await connection.CloseAsync();

            return doesExist;

        }

    }
}
