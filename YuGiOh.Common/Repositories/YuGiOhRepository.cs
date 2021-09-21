using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using Dommel;
using Newtonsoft.Json;
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

        static YuGiOhRepository()
        {

            FluentMapper.Initialize(config =>
            {

                config
                    .AddConvention<LowerCaseConvention>()
                    .ForEntity<CardEntity>()
                    .ForEntity<BoosterPack>();

                config.AddMap(new CardEntityMapper());
                config.ForDommel();

                var resolver = new LowerCaseConvention();

                DommelMapper.SetColumnNameResolver(resolver);

            });

        }

        public YuGiOhRepository(IYuGiOhRepositoryConfiguration config)
        {
            _config = config;
        }

        public async Task InsertCardAsync(CardEntity card)
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

            if (card.Archetypes is not null)
            {

                foreach (var archetype in card.Archetypes)
                {

                    var archetypeId = await connection.QuerySingleProcAsync<int>("insert_or_get_archetype", new { input = archetype });

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

            if (card.Supports is not null)
            {

                foreach (var support in card.Supports)
                {

                    var supportId = await connection.QuerySingleProcAsync<int>("insert_or_get_support", new { input = support });

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

            if (card.AntiSupports is not null)
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

        public async Task<CardEntity> GetCardAsync(string input)
        {

            var connection = _config.GetYuGiOhDbConnection();
            CardEntity card;

            await connection.OpenAsync();

            try
            {

                card = await connection.QuerySingleProcAsync<CardEntity>("get_card_exact", new { input });
                var archetypes = await connection.QueryProcAsync<string>("get_card_archetypes", new { id = card.ArchetypesId });
                var supports = await connection.QueryProcAsync<string>("get_card_supports", new { id = card.ArchetypesId });
                var antiSupports = await connection.QueryProcAsync<string>("get_card_antisupports", new { id = card.ArchetypesId });

                card.Archetypes = archetypes.ToList();
                card.Supports = supports.ToList();
                card.AntiSupports = antiSupports.ToList();

            }
            catch (Exception)
            {
                card = null;
            }
            finally
            {
                await connection.CloseAsync();
            }

            return card;

        }

        public async Task<IEnumerable<CardEntity>> SearchCardsAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var parameterizedInput = $"%{input}%";
            var parameters = new DynamicParameters();

            parameters.Add("input", input);
            parameters.Add("parameterized_input", parameterizedInput);

            var cards = await connection.QueryProcAsync<CardEntity>("search_cards", parameters);

            await connection.CloseAsync();

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsContainsAllAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var inputWords = input.Split(' ');
            //var results = await connection.SelectAsync<CardEntity>(card =>
            //    inputWords.All(word => card.Name.ContainsIgnoreCase(word))
            //);

            var sqlBuilder = new SqlBuilder();
            var selector = sqlBuilder.AddTemplate("select * from cards /**where**/ order by char_length(name)");
            var parameters = new DynamicParameters();

            for (var i = 0; i < inputWords.Length; i++)
            {

                var key = $"@{i}";

                sqlBuilder.Where($"name ~~* {key}");
                parameters.Add(key, $"%{inputWords[i]}%");

            }

            var cards = await connection.QueryAsync<CardEntity>(selector.RawSql, parameters);

            await connection.CloseAsync();

            //cards = results.Select(card => card.Name);

            return cards;

        }

        public async Task<CardEntity> GetCardFuzzyAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var card = await connection.QuerySingleProcAsync<CardEntity>("get_card_fuzzy", new { input });
            var archetypes = await connection.QueryProcAsync<string>("get_card_archetypes", new { id = card.ArchetypesId });
            var supports = await connection.QueryProcAsync<string>("get_card_supports", new { id = card.ArchetypesId });
            var antiSupports = await connection.QueryProcAsync<string>("get_card_antisupports", new { id = card.ArchetypesId });

            await connection.CloseAsync();

            card.Archetypes = archetypes.ToList();
            card.Supports = supports.ToList();
            card.AntiSupports = antiSupports.ToList();

            return card;

        }

        public async Task<CardEntity> GetRandomCardAsync()
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var card = await connection.QuerySingleProcAsync<CardEntity>("get_random_card");
            var archetypes = await connection.QueryProcAsync<string>("get_card_archetypes", new { id = card.ArchetypesId });
            var supports = await connection.QueryProcAsync<string>("get_card_supports", new { id = card.SupportsId });
            var antisupports = await connection.QueryProcAsync<string>("get_card_antisupports", new { id = card.AntiSupportsId });

            await connection.CloseAsync();

            card.Archetypes = archetypes.ToList();
            card.Supports = supports.ToList();
            card.AntiSupports = antisupports.ToList();

            return card;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInArchetypeAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var cards = await connection.QueryProcAsync<CardEntity>("get_cards_in_archetype", new { input });

            await connection.CloseAsync();

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInSupportAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var cards = await connection.QueryProcAsync<CardEntity>("get_cards_in_support", new { input });

            await connection.CloseAsync();

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInAntisupportAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var cards = await connection.QueryProcAsync<CardEntity>("get_cards_in_antisupport", new { input });

            await connection.CloseAsync();

            return cards;

        }

        public async Task<string> GetNameWithPasscodeAsync(string passcode)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var name = await connection.ExecuteScalarAsync<string>("select name from cards where passcode = @passcode", new { passcode });

            await connection.CloseAsync();

            return name;

        }

        public async Task<string> GetImageLinkAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var imgLink = await connection.QuerySingleAsync<string>("select img from cards where name ~~* @input", new { input });

            await connection.CloseAsync();

            return imgLink;

        }

        public async Task<IEnumerable<string>> GetBanlistCards(CardEntityFormats format)
        {

            var formatStr = format switch
            {

                CardEntityFormats.OCG => "ocgstatus",
                CardEntityFormats.TCG => "tcgstatus",
                CardEntityFormats.TCGTRAD => "tcgtrnstatus",
                _ => null

            };

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var cards = await connection.QueryAsync<string>("select name from cards where @formatStr in ('Forbidden', 'Semi-Limited', 'Limited')");

            await connection.CloseAsync();

            return cards;

        }

    }
}
