using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using Dommel;
using Newtonsoft.Json;
using Npgsql;
using YuGiOh.Common.DatabaseMappers;
using YuGiOh.Common.Extensions;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Common.Repositories
{
    public class YuGiOhRepository : IYuGiOhRepository
    {

        private const string EscapeSqlParameterRegex = @"([%_\[])";
        private const string ReplaceEscapeSqlParameterRegex = "[$1]";

        private readonly IYuGiOhRepositoryConfiguration _config;

        public YuGiOhRepository(IYuGiOhRepositoryConfiguration config)
        {

            _config = config;

            FluentMapper.Initialize(config =>
            {

                if (!FluentMapper.TypeConventions.ContainsKey(typeof(YuGiOhDatabaseResolver)))
                {

                    config
                        .AddConvention<YuGiOhDatabaseResolver>()
                        .ForEntitiesInCurrentAssembly();

                }

                config.ForDommel();

                var resolver = new YuGiOhDatabaseResolver();

                DommelMapper.SetColumnNameResolver(resolver);

            });

        }

        public async Task InsertCardAsync(Card card)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            //instead of insert on conflict update
            //this is done for readability and laziness
            //easier to let dapper handle the insertion than write out all the column names
            //and have them not clash with parameter names (if using function)
            var doesExistBit = await connection.ExecuteScalarAsync<int>("select count(1) from cards where id = @Id", new { card.Id });

            if (doesExistBit == 1)
                await connection.UpdateAsync(card);
            else
                await connection.InsertAsync(card);

            var cardArchetypesId = await connection.ExecuteScalarAsync("select archetypes from cards where id = @Id", new { card.Id });
            var cardSupportsId = await connection.ExecuteScalarAsync("select supports from cards where id = @Id", new { card.Id });
            var cardAntiSupportsId = await connection.ExecuteScalarAsync("select antisupports from cards where id = @Id", new { card.Id });

            if (card.Archetypes != null)
            {

                foreach (var archetype in card.Archetypes)
                {

                    var archetypeId = await connection.QuerySingleAsync<int>("insert_or_get_archetype", new { input = archetype }, commandType: CommandType.StoredProcedure);

                    //var archetypeId =
                    //    await connection.ExecuteScalarAsync("select id from archetypes where name = @archetype", new { archetype }) ??
                    //    await connection.ExecuteScalarAsync("insert into archetypes(name) values(@archetype) returning id", new { archetype });

                     await connection.ExecuteAsync(
                        "insert into card_to_archetypes values(@cardArchetypesId, @archetypeId) on conflict on constraint cardarchetypesid_archetypesid_pair_unique do nothing",
                        new
                        {
                            cardArchetypesId,
                            archetypeId
                        });

                }

            }

            if (card.Supports != null)
            {

                foreach (var support in card.Supports)
                {

                    var supportId = await connection.QuerySingleAsync<int>("insert_or_get_support", new { input = support }, commandType: CommandType.StoredProcedure);

                    //object supportId =
                    //    await connection.ExecuteScalarAsync("select id from supports where name = @support", new { support }) ??
                    //    await connection.ExecuteScalarAsync("insert into supports(name) values(@support) returning id", new { support });

                    await connection.ExecuteAsync(
                        "insert into card_to_supports values(@cardSupportsId, @supportId) on conflict on constraint cardsupportsid_supportsid_pair_unique do nothing",
                        new
                        {
                            cardSupportsId,
                            supportId
                        });

                }

            }

            if (card.AntiSupports != null)
            {

                foreach (var antiSupport in card.AntiSupports)
                {

                    var antiSupportId = await connection.QuerySingleAsync<int>("insert_or_get_antisupport", new { input = antiSupport }, commandType: CommandType.StoredProcedure);

                    //object antiSupportId =
                    //    await connection.ExecuteScalarAsync("select id from antisupports where name = @antiSupport", new { antiSupport }) ??
                    //    await connection.ExecuteScalarAsync("insert into antisupports(name) values(@antiSupport) returning id", new { antiSupport });

                    await connection.ExecuteAsync(
                        "insert into card_to_antisupports values(@cardAntiSupportsId, @antiSupportId) on conflict on constraint cardantisupportsid_antisupportsid_pair_unique do nothing",
                        new
                        {
                            cardAntiSupportsId,
                            antiSupportId
                        });

                }

            }

            await connection.CloseAsync();

        }

        public async Task InsertBoosterPack(BoosterPack boosterPack)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var doesExistBit = await connection.ExecuteScalarAsync<int>("select count(1) from boosterpacks where id = @Id", new { boosterPack.Id });

            if (doesExistBit == 1)
                await connection.UpdateAsync(boosterPack);
            else
                await connection.InsertAsync(boosterPack);

            using (var update = connection.CreateCommand())
            {

                update.CommandText = "update boosterpacks set dates = @dates, cards = @cards where id = @id";

                update.Parameters.AddWithValue("id", boosterPack.Id);
                update.Parameters.AddWithValue("dates", JsonConvert.SerializeObject(boosterPack.Dates));
                update.Parameters.AddWithValue("cards", JsonConvert.SerializeObject(boosterPack.Cards));

                await update.ExecuteNonQueryAsync();

            }

            await connection.CloseAsync();

        }

        public async Task InsertErrorAsync(Error error)
        {

            using (var connection = _config.GetYuGiOhDbConnection())
            {

                await connection.OpenAsync();
                await connection.InsertAsync(error);
                await connection.CloseAsync();

            }

        }

        public async Task<IEnumerable<string>> GetCardsAsync(string input)
        {

            IEnumerable<string> cards;

            using (var connection = _config.GetYuGiOhDbConnection())
            {

                await connection.OpenAsync();

                //search order (case insensitive):
                //exact full match
                //match if word contains all terms

                cards = await connection.QueryAsync<string>("get_card_exact", new { input }, commandType: CommandType.StoredProcedure);

                if (!cards.Any())
                    cards = await connection.QueryAsync<string>("get_card_contains", new { input }, commandType: CommandType.StoredProcedure);

                await connection.CloseAsync();

            }

            return cards;

        }

        public async Task<IEnumerable<string>> GetCardsContainsAllAsync(string input)
        {

            IEnumerable<string> cards;

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var inputWords = input.Split(' ');
            var results = await connection.SelectAsync<Card>(card =>
                inputWords.All(word => card.Name.ContainsIgnoreCase(word)
            ));

            await connection.CloseAsync();

            cards = results.Select(card => card.Name);

            return cards;

        }

        public async Task<Card> GetCardFuzzyAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var card = await connection.QuerySingleAsync<Card>("get_card_fuzzy", new { input }, commandType: CommandType.StoredProcedure);
            var archetypes = await connection.QueryAsync<string>("get_archetypes", new { Id = card.ArchetypesId }, commandType: CommandType.StoredProcedure);
            var supports = await connection.QueryAsync<string>("get_supports", new { Id = card.ArchetypesId }, commandType: CommandType.StoredProcedure);
            var antiSupports = await connection.QueryAsync<string>("get_antisupports", new { Id = card.ArchetypesId }, commandType: CommandType.StoredProcedure);

            await connection.CloseAsync();

            card.Archetypes = archetypes.ToList();
            card.Supports = supports.ToList();
            card.AntiSupports = antiSupports.ToList();

            return card;

        }

        public async Task<IEnumerable<string>> GetCardsFromArchetypeAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var cards = await connection.QueryAsync<string>("get_cards_from_archetype", new { input }, commandType: CommandType.StoredProcedure);

            await connection.CloseAsync();

            return cards;

        }

    }
}
