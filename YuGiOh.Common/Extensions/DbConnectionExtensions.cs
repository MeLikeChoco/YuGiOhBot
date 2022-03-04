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

        public static async Task<IEnumerable<CardEntity>> QueryCardsAsync(
            this DbConnection connection,
            string sql,
            object @params = null,
            string splitOn = DefaultSplitOn
        )
        {

            var results = await connection.QueryAsync(
                sql,
                ProcessEntities(),
                @params,
                splitOn: splitOn
            );

            return results.Distinct();

        }

        public static async Task<CardEntity> QuerySingleCardProcAsync(
            this DbConnection connection,
            string proc,
            object @params = null,
            string splitOn = DefaultSplitOn
        )
        {

            var result = await connection.QueryAsync(
                proc,
                ProcessEntity(),
                @params,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();


        }

        public static async Task<IEnumerable<CardEntity>> QueryCardsProcAsync(
            this DbConnection connection,
            string proc,
            object @params = null,
            string splitOn = DefaultSplitOn
        )
        {

            var result = await connection.QueryAsync(
                proc,
                ProcessEntities(),
                @params,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure
            );

            return result.Distinct();

        }

        private static Func<CardEntity, string, string, string, TranslationEntity, CardEntity> ProcessEntity()
        {

            CardEntity cardEntity = null;
            var translations = new HashSet<int>();

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

                if (translation is null || translations.Contains(translation.Id))
                    return cardEntity;

                cardEntity.Translations.Add(translation);
                translations.Add(translation.Id);

                return cardEntity;

            }

            return ProcessEntityFunction;

        }

        private static Func<CardEntity, string, string, string, TranslationEntity, CardEntity> ProcessEntities()
        {

            var entities = new Dictionary<int, CardEntity>();
            var entityToTranslations = new Dictionary<int, HashSet<int>>();

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

                if (!entityToTranslations.TryGetValue(entity.Id, out var translations))
                {
                    translations = new HashSet<int>();
                    entityToTranslations[entity.Id] = translations;
                }

                if (translation is null || translations.Contains(translation.Id))
                    return cardEntity;

                cardEntity.Translations.Add(translation);
                translations.Add(translation.Id);

                return cardEntity;

            }

            return ProcessEntityFunction;

        }

    }
}