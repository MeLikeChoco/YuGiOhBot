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
        private readonly Dictionary<int, CardEntity> _entities;

        static YuGiOhRepository()
        {

            FluentMapper.Initialize(config =>
            {

                config
                    .AddConvention<LowerCaseConvention>()
                    .ForEntity<CardEntity>()
                    .ForEntity<BoosterPackEntity>();

                config.AddMap(new CardEntityMapper());
                config.ForDommel();

                var resolver = new LowerCaseConvention();

                DommelMapper.SetColumnNameResolver(resolver);

            });

        }

        public YuGiOhRepository(IYuGiOhRepositoryConfiguration config)
        {
            _config = config;
            _entities = new();
        }

        public async Task InsertCardAsync(CardEntity card)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            //instead of insert on conflict update
            //this is done for readability and laziness
            //easier to let dapper handle the insertion than write out all the column names
            //and have them not clash with parameter names (if using function)
            var doesExistBit = await connection.ExecuteScalarAsync<int>("select count(1) from cards where id = @Id", new { card.Id }).ConfigureAwait(false);

            if (doesExistBit == 1)
                await connection.UpdateAsync(card).ConfigureAwait(false);
            else
                await connection.InsertAsync(card).ConfigureAwait(false);

            var cardArchetypesId = await connection.ExecuteScalarAsync("select archetypes from cards where id = @Id", new { card.Id }).ConfigureAwait(false);
            var cardSupportsId = await connection.ExecuteScalarAsync("select supports from cards where id = @Id", new { card.Id }).ConfigureAwait(false);
            var cardAntiSupportsId = await connection.ExecuteScalarAsync("select antisupports from cards where id = @Id", new { card.Id }).ConfigureAwait(false);

            if (card.Archetypes is not null)
            {

                foreach (var archetype in card.Archetypes)
                {

                    if (string.IsNullOrEmpty(archetype))
                        continue;

                    var archetypeId = await connection.QuerySingleProcAsync<int>("insert_or_get_archetype", new { input = archetype }).ConfigureAwait(false);

                    //var archetypeId =
                    //    await connection.ExecuteScalarAsync("select id from archetypes where name = @archetype", new { archetype }) ??
                    //    await connection.ExecuteScalarAsync("insert into archetypes(name) values(@archetype) returning id", new { archetype });

                    await connection.ExecuteAsync(
                       "insert into card_to_archetypes values(@cardArchetypesId, @archetypeId) on conflict on constraint cardarchetypesid_archetypesid_pair_unique do nothing",
                       new
                       {
                           cardArchetypesId,
                           archetypeId
                       }).ConfigureAwait(false);

                }

            }

            if (card.Supports is not null)
            {

                foreach (var support in card.Supports)
                {

                    if (string.IsNullOrEmpty(support))
                        continue;

                    var supportId = await connection.QuerySingleProcAsync<int>("insert_or_get_support", new { input = support }).ConfigureAwait(false);

                    //object supportId =
                    //    await connection.ExecuteScalarAsync("select id from supports where name = @support", new { support }) ??
                    //    await connection.ExecuteScalarAsync("insert into supports(name) values(@support) returning id", new { support });

                    await connection.ExecuteAsync(
                        "insert into card_to_supports values(@cardSupportsId, @supportId) on conflict on constraint cardsupportsid_supportsid_pair_unique do nothing",
                        new
                        {
                            cardSupportsId,
                            supportId
                        }).ConfigureAwait(false);

                }

            }

            if (card.AntiSupports is not null)
            {

                foreach (var antiSupport in card.AntiSupports)
                {

                    if (string.IsNullOrEmpty(antiSupport))
                        continue;

                    var antiSupportId = await connection.QuerySingleAsync<int>("insert_or_get_antisupport", new { input = antiSupport }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

                    //object antiSupportId =
                    //    await connection.ExecuteScalarAsync("select id from antisupports where name = @antiSupport", new { antiSupport }) ??
                    //    await connection.ExecuteScalarAsync("insert into antisupports(name) values(@antiSupport) returning id", new { antiSupport });

                    await connection.ExecuteAsync(
                        "insert into card_to_antisupports values(@cardAntiSupportsId, @antiSupportId) on conflict on constraint cardantisupportsid_antisupportsid_pair_unique do nothing",
                        new
                        {
                            cardAntiSupportsId,
                            antiSupportId
                        }).ConfigureAwait(false);

                }

            }

            await connection.CloseAsync().ConfigureAwait(false);

        }

        public async Task InsertCardHashAsync(int id, string hash)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.ExecuteAsync("insert into card_hashes values(@id, @hash) on conflict on constraint card_hashes_pkey do update set hash = @hash", new { id, hash }).ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);

        }

        public async Task InsertBoosterPack(BoosterPackEntity boosterPack)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var doesExistBit = await connection.ExecuteScalarAsync<int>("select count(1) from boosterpacks where id = @Id", new { boosterPack.Id }).ConfigureAwait(false);

            if (doesExistBit == 1)
                await connection.UpdateAsync(boosterPack).ConfigureAwait(false);
            else
                await connection.InsertAsync(boosterPack).ConfigureAwait(false);

            using (var update = connection.CreateCommand())
            {

                update.CommandText = "update boosterpacks set dates = @dates, cards = @cards where id = @id";

                update.Parameters.AddWithValue("id", boosterPack.Id);
                update.Parameters.AddWithValue("dates", JsonConvert.SerializeObject(boosterPack.Dates));
                update.Parameters.AddWithValue("cards", JsonConvert.SerializeObject(boosterPack.Cards));

                await update.ExecuteNonQueryAsync().ConfigureAwait(false);

            }

            await connection.CloseAsync().ConfigureAwait(false);

        }

        public async Task InsertErrorAsync(Error error)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.InsertAsync(error).ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);

        }

        public async Task<CardEntity> GetCardAsync(string input)
        {

            var connection = _config.GetYuGiOhDbConnection();
            CardEntity card;

            await connection.OpenAsync().ConfigureAwait(false);

            try
            {
                card = await connection.QuerySingleCardProcAsync("get_card_exact", new { input }).ConfigureAwait(false);
            }
            catch (Exception)
            {
                card = null;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }

            return card;

        }

        public async Task<IEnumerable<CardEntity>> SearchCardsAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            //input = $"%{input}%";

            var cards = await connection.QueryCardProcAsync("search_cards", new { input }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsAutocomplete(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryCardProcAsync("get_cards_autocomplete", new { input });

            await connection.CloseAsync().ConfigureAwait(false);

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsContainsAllAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var inputWords = input.Split(' ');
            //var results = await connection.SelectAsync<CardEntity>(card =>
            //    inputWords.All(word => card.Name.ContainsIgnoreCase(word))
            //);

            var sqlBuilder = new SqlBuilder();
            var selector = sqlBuilder.AddTemplate("select * from joined_cards /**where**/ order by char_length(name)");
            var parameters = new DynamicParameters();

            for (var i = 0; i < inputWords.Length; i++)
            {

                var key = $"@{i}";

                sqlBuilder.Where($"name ilike {key}");
                parameters.Add(key, $"%{inputWords[i]}%");

            }

            var cards = await connection.QueryCardAsync(selector.RawSql, parameters).ConfigureAwait(false);

            //foreach (var card in cards)
            //    await FillCardEntity(connection, card).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            //cards = results.Select(card => card.Name);

            return cards;

        }

        public async Task<CardEntity> GetCardFuzzyAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var card = await connection.QuerySingleProcAsync<CardEntity>("get_card_fuzzy", new { input }).ConfigureAwait(false);
            card = await FillCardEntity(connection, card).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return card;

        }

        public async Task<CardEntity> GetRandomCardAsync()
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var card = await connection.QuerySingleProcAsync<CardEntity>("get_random_card").ConfigureAwait(false);
            card = await FillCardEntity(connection, card).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return card;

        }

        public async Task<string> GetCardHashAsync(int id)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var serverHash = await connection.QueryFirstOrDefaultAsync<string>("select hash from card_hashes where id = @id", new { id }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return serverHash?.Trim();

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInArchetypeAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryCardProcAsync("get_cards_in_archetype", new { input }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return cards;

        }

        public async Task<IEnumerable<string>> GetArchetypesAutocomplete(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var archetypes = await connection.QueryProcAsync<string>("get_archetypes_autocomplete", new { input }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return archetypes;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInSupportAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryProcAsync<CardEntity>("get_cards_in_support", new { input }).ConfigureAwait(false);

            foreach (var card in cards)
                await FillCardEntity(connection, card).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInAntisupportAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryProcAsync<CardEntity>("get_cards_in_antisupport", new { input }).ConfigureAwait(false);

            foreach (var card in cards)
                await FillCardEntity(connection, card).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return cards;

        }

        public async Task<string> GetNameWithPasscodeAsync(string passcode)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var name = await connection.ExecuteScalarAsync<string>("select name from cards where passcode = @passcode", new { passcode }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return name;

        }

        public async Task<string> GetImageLinkAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var imgLink = await connection.QuerySingleAsync<string>("select img from cards where name ~~* @input", new { input }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return imgLink;

        }

        public async Task<Banlist> GetBanlistAsync(BanlistFormats format)
        {

            var formatStr = format switch
            {

                BanlistFormats.OCG => "ocgstatus",
                BanlistFormats.TCG => "tcgadvstatus",
                BanlistFormats.TRAD => "tcgtrnstatus",
                _ => null

            };

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var banlist = new Banlist();
            banlist.Forbidden = await connection.QueryAsync<string>($"select name from cards where {formatStr} ~~* 'forbidden' order by name asc").ConfigureAwait(false);
            banlist.Limited = await connection.QueryAsync<string>($"select name from cards where {formatStr} ~~* 'limited' order by name asc").ConfigureAwait(false);
            banlist.SemiLimited = await connection.QueryAsync<string>($"select name from cards where {formatStr} ~~* 'semi-limited' order by name asc").ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return banlist;

        }

        public async Task<BoosterPackEntity> GetBoosterPackAsync(string input)
        {

            using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var entity = await connection.QueryFirstAsync<BoosterPackEntity>("select * from boosterpacks where name ~~* @input", new { input }).ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            return entity;

        }

        private async Task<CardEntity> FillCardEntity(NpgsqlConnection connection, CardEntity entity)
        {

            var archetypes = await connection.QueryProcAsync<string>("get_card_archetypes", new { id = entity.ArchetypesId }).ConfigureAwait(false);
            var supports = await connection.QueryProcAsync<string>("get_card_supports", new { id = entity.SupportsId }).ConfigureAwait(false);
            var antisupports = await connection.QueryProcAsync<string>("get_card_antisupports", new { id = entity.AntiSupportsId }).ConfigureAwait(false);

            entity.Archetypes = archetypes.ToList();
            entity.Supports = supports.ToList();
            entity.AntiSupports = antisupports.ToList();

            return entity;

        }

    }
}
