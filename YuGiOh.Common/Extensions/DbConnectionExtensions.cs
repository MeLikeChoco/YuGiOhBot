using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.Extensions
{
    public static class DbConnectionExtensions
    {

        private const string DefaultSplitOn = "archetypename,supportname,antisupportname,translationid";

        public static Task<IEnumerable<T>> QueryProcAsync<T>(this DbConnection connection, string proc, object @params = null)
            => connection.QueryAsync<T>(proc, @params, commandType: CommandType.StoredProcedure);

        public static Task<T> QuerySingleProcAsync<T>(this DbConnection connection, string proc, object @params = null)
            => connection.QuerySingleAsync<T>(proc, @params, commandType: CommandType.StoredProcedure);

        public static Task<IEnumerable<CardEntity>> QueryCardsAsync(
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

        public static Task<IEnumerable<CardEntity>> QueryCardsProcAsync(
            this DbConnection connection,
            string proc,
            object @params = null,
            string splitOn = DefaultSplitOn
        )
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
            string splitOn = DefaultSplitOn
        )
        {

            return connection.QueryAsync(
                proc,
                ProcessEntity(),
                param: @params,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure
            ).ContinueWith(results => results.Result.FirstOrDefault());

        }

        private static Func<CardEntity, string, string, string, TranslationEntity, CardEntity> ProcessEntities()
        {

            var entities = new Dictionary<int, CardEntity>();

            CardEntity ProcessEntityFunction(
                CardEntity entity,
                string archetype,
                string support,
                string antisupport,
                TranslationEntity translation
            )
            {

                if (!entities.TryGetValue(entity.Id, out var cardEntity))
                {

                    cardEntity = entity;
                    cardEntity.Archetypes = new List<string>();
                    cardEntity.Supports = new List<string>();
                    cardEntity.AntiSupports = new List<string>();
                    cardEntity.Translations = new List<TranslationEntity>();

                    entities.Add(cardEntity.Id, cardEntity);

                }

                if (!cardEntity.Archetypes.Contains(archetype) && !string.IsNullOrEmpty(archetype))
                    cardEntity.Archetypes.Add(archetype);

                if (!cardEntity.Supports.Contains(support) && !string.IsNullOrEmpty(support))
                    cardEntity.Supports.Add(support);

                if (!cardEntity.AntiSupports.Contains(antisupport) && !string.IsNullOrEmpty(antisupport))
                    cardEntity.AntiSupports.Add(antisupport);
                
                cardEntity.Translations.Add(translation);

                return cardEntity;

            }

            return ProcessEntityFunction;

        }

        private static Func<CardEntity, string, string, string, TranslationEntity, CardEntity> ProcessEntity()
        {

            CardEntity cardEntity = null;

            CardEntity ProcessEntityFunction(
                CardEntity entity,
                string archetype,
                string support,
                string antisupport,
                TranslationEntity translation
            )
            {

                if (cardEntity == null)
                {
                    cardEntity = entity;
                    cardEntity.Archetypes = new List<string>();
                    cardEntity.Supports = new List<string>();
                    cardEntity.AntiSupports = new List<string>();
                    cardEntity.Translations = new List<TranslationEntity>();
                }

                if (!cardEntity.Archetypes.Contains(archetype) && !string.IsNullOrEmpty(archetype))
                    cardEntity.Archetypes.Add(archetype);

                if (!cardEntity.Supports.Contains(support) && !string.IsNullOrEmpty(support))
                    cardEntity.Supports.Add(support);

                if (!cardEntity.AntiSupports.Contains(antisupport) && !string.IsNullOrEmpty(antisupport))
                    cardEntity.AntiSupports.Add(antisupport);

                cardEntity.Translations.Add(translation);

                return cardEntity;

            }

            return ProcessEntityFunction;

        }

    }
}