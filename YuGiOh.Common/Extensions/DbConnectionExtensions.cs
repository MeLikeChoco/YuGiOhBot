using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.Extensions
{
    public static class DbConnectionExtensions
    {

        private const string DefaultSplitOn = "archetypename,supportname,antisupportname";

        public static Task<IEnumerable<T>> QueryProcAsync<T>(this DbConnection connection, string proc, object @params = null)
            => connection.QueryAsync<T>(proc, @params, commandType: CommandType.StoredProcedure);

        public static Task<T> QuerySingleProcAsync<T>(this DbConnection connection, string proc, object @params = null)
            => connection.QuerySingleAsync<T>(proc, @params, commandType: CommandType.StoredProcedure);

        public static Task<IEnumerable<CardEntity>> QueryCardAsync(
            this DbConnection connection,
            string sql,
            object @params = null,
            string splitOn = DefaultSplitOn
        )
        {

            return connection.QueryAsync(
                sql,
                ProcessEntities(),
                param: @params,
                splitOn: splitOn
            ).ContinueWith(results => results.Result.Distinct());

        }

        public static Task<IEnumerable<CardEntity>> QueryCardProcAsync(
            this DbConnection connection,
            string proc,
            object @params = null,
            string splitOn = DefaultSplitOn)
        {

            return connection.QueryAsync(
                proc,
                ProcessEntities(),
                param: @params,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure
            ).ContinueWith(results => results.Result.Distinct());

        }

        public static Task<CardEntity> QuerySingleCardProcAsync(
            this DbConnection connection,
            string proc,
            object @params = null,
            string splitOn = DefaultSplitOn)
        {

            return connection.QueryAsync(
                proc,
                ProcessEntity(),
                param: @params,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure
            ).ContinueWith(results => results.Result.Distinct().FirstOrDefault());

        }

        private static Func<CardEntity, string, string, string, CardEntity> ProcessEntities()
        {

            var entities = new Dictionary<int, CardEntity>();

            CardEntity processEntity(
                CardEntity entity,
                string archetype,
                string support,
                string antisupport
            )
            {

                if (!entities.TryGetValue(entity.Id, out var cardEntity))
                {

                    cardEntity = entity;
                    cardEntity.Archetypes = new();
                    cardEntity.Supports = new();
                    cardEntity.AntiSupports = new();

                    entities.Add(cardEntity.Id, cardEntity);

                }

                if (!cardEntity.Archetypes.Contains(archetype) && !string.IsNullOrEmpty(archetype))
                    cardEntity.Archetypes.Add(archetype);

                if (!cardEntity.Supports.Contains(support) && !string.IsNullOrEmpty(support))
                    cardEntity.Supports.Add(support);

                if (!cardEntity.AntiSupports.Contains(antisupport) && !string.IsNullOrEmpty(antisupport))
                    cardEntity.AntiSupports.Add(antisupport);

                return cardEntity;

            }

            return processEntity;

        }

        private static Func<CardEntity, string, string, string, CardEntity> ProcessEntity()
        {

            CardEntity cardEntity = null;

            CardEntity processEntity(
                CardEntity entity,
                string archetype,
                string support,
                string antisupport
            )
            {

                if (cardEntity == null)
                {
                    cardEntity = entity;
                    cardEntity.Archetypes = new();
                    cardEntity.Supports = new();
                    cardEntity.AntiSupports = new();
                }

                if (!cardEntity.Archetypes.Contains(archetype) && !string.IsNullOrEmpty(archetype))
                    cardEntity.Archetypes.Add(archetype);

                if (!cardEntity.Supports.Contains(support) && !string.IsNullOrEmpty(support))
                    cardEntity.Supports.Add(support);

                if (!cardEntity.AntiSupports.Contains(antisupport) && !string.IsNullOrEmpty(antisupport))
                    cardEntity.AntiSupports.Add(antisupport);

                return cardEntity;

            }

            return processEntity;

        }

    }
}
