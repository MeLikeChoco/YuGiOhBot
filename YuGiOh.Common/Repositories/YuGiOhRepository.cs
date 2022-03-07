using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using Dommel;
using Npgsql;
using YuGiOh.Common.DatabaseMappers;
using YuGiOh.Common.Extensions;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Common.Repositories.Interfaces;

// Although Dapper auto-opens/closes connections that call it when it wasn't opened when called upon, I rather open it myself for consistency
// Not using CloseAsync() because Dispose() automatically calls it
// Theoretically.....
// Hopefully........
// Please close it..... don't make me look stupid

namespace YuGiOh.Common.Repositories
{
    public class YuGiOhRepository : IYuGiOhRepository
    {

        // private const string EscapeSqlParameterRegex = @"([%_\[])";
        // private const string ReplaceEscapeSqlParameterRegex = "[$1]";

        private readonly IYuGiOhRepositoryConfiguration _config;

        static YuGiOhRepository()
        {

            FluentMapper.Initialize(config =>
            {

                config
                    .AddConvention<LowerCaseConvention>()
                    .ForEntity<CardEntity>()
                    .ForEntity<BoosterPackEntity>();

                config.AddMap(new CardEntityMapper());
                config.AddMap(new BoosterPackEntityMapper());
                config.ForDommel();

                var resolver = new LowerCaseConvention();

                DommelMapper.SetColumnNameResolver(resolver);
                DommelMapper.AddSqlBuilder(typeof(NpgsqlConnection), new PostgresSqlBuilder());

            });

        }

        public YuGiOhRepository(IYuGiOhRepositoryConfiguration config)
        {
            _config = config;
        }

        #region Insertions

        public async Task InsertCardAsync(CardEntity card)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var doesExistBit = await connection.ExecuteScalarAsync<int>("select count(1) from cards where id = @Id", new { card.Id }).ConfigureAwait(false);

            if (doesExistBit == 1)
                await connection.UpdateAsync(card).ConfigureAwait(false);
            else
                await connection.InsertAsync(card).ConfigureAwait(false);

            // var (cardArchetypesId, cardSupportsId, cardAntiSupportsId) = await connection.ExecuteScalarAsync<(int, int, int)>("select archetypes, supports, antisupports from cards where id = @id", new { id = card.Id });

            if (card.Translations is not null && card.Translations.Any())
            {

                foreach (var translation in card.Translations)
                {

                    translation.CardId = card.Id;

                    await connection.ExecuteAsync(
                            "upsert_translation",
                            new
                            {
                                cardid = translation.CardId,
                                language = translation.Language,
                                name = translation.Name,
                                lore = translation.Lore
                            },
                            commandType: CommandType.StoredProcedure
                        )
                        .ConfigureAwait(false);

                }

            }

            if (card.Archetypes is not null && card.Archetypes.Any())
            {

                var archetypes = card.Archetypes.Where(archetype => !string.IsNullOrEmpty(archetype));
                var archetypesInDb = await connection
                    .QueryAsync<string>("select distinct archetypename from joined_cards where id = @id", new { id = card.Id })
                    .ContinueWith(result => result.Result.ToArray())
                    .ConfigureAwait(false);
                var archetypesToDelete = archetypesInDb.Where(archetype => !archetypes.Contains(archetype));
                var archetypesToInsert = archetypes.Where(archetype => !archetypesInDb.Contains(archetype));

                foreach (var archetype in archetypesToDelete)
                    await connection.ExecuteAsync("call delete_archetype_relation(@cardname, @archetype)", new { cardname = card.Name, archetype }).ConfigureAwait(false);

                foreach (var archetype in archetypesToInsert)
                    await connection.ExecuteAsync("call insert_archetype_relation(@cardname, @archetype)", new { cardname = card.Name, archetype }).ConfigureAwait(false);

            }

            if (card.Supports is not null && card.Supports.Any())
            {

                var supports = card.Supports.Where(support => !string.IsNullOrEmpty(support));
                var supportsInDb = await connection
                    .QueryAsync<string>("select distinct supportname from joined_cards where id = @id", new { id = card.Id })
                    .ContinueWith(result => result.Result.ToArray())
                    .ConfigureAwait(false);
                var supportsToDelete = supportsInDb.Where(support => !supports.Contains(support));
                var supportsToInsert = supports.Where(support => !supportsInDb.Contains(support));

                foreach (var support in supportsToDelete)
                    await connection.ExecuteAsync("call delete_support_relation(@cardname, @support)", new { cardname = card.Name, support }).ConfigureAwait(false);

                foreach (var support in supportsToInsert)
                    await connection.ExecuteAsync("call insert_support_relation(@cardname, @support)", new { cardname = card.Name, support }).ConfigureAwait(false);

            }

            if (card.AntiSupports is not null && card.AntiSupports.Any())
            {

                var antisupports = card.AntiSupports.Where(antisupport => !string.IsNullOrEmpty(antisupport));
                var antisupportsInDb = await connection
                    .QueryAsync<string>("select distinct antisupportname from joined_cards where id = @id", new { id = card.Id })
                    .ContinueWith(result => result.Result.ToArray())
                    .ConfigureAwait(false);
                var antisupportsToDelete = antisupportsInDb.Where(antisupport => !antisupports.Contains(antisupport));
                var antisupportsToInsert = antisupports.Where(antisupport => !antisupportsInDb.Contains(antisupport));

                foreach (var antisupport in antisupportsToDelete)
                    await connection.ExecuteAsync("call delete_antisupport_relation(@cardname, @antisupport)", new { cardname = card.Name, antisupport }).ConfigureAwait(false);

                foreach (var antisupport in antisupportsToInsert)
                    await connection.ExecuteAsync("call insert_antisupport_relation(@cardname, @antisupport)", new { cardname = card.Name, antisupport }).ConfigureAwait(false);

            }

        }

        public async Task InsertCardHashAsync(int id, string hash)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.ExecuteAsync("insert into card_hashes values(@id, @hash) on conflict on constraint card_hashes_pkey do update set hash = @hash", new { id, hash }).ConfigureAwait(false);

        }

        public async Task InsertBoosterPack(BoosterPackEntity boosterPack)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var doesExistBit = await connection.ExecuteScalarAsync<int>("select count(1) from boosterpacks where id = @Id", new { boosterPack.Id }).ConfigureAwait(false);

            if (doesExistBit == 1)
                await connection.UpdateAsync(boosterPack).ConfigureAwait(false);
            else
                await connection.InsertAsync(boosterPack).ConfigureAwait(false);

            var (datesId, cardsId) = await connection.QuerySingleAsync<(int, int)>("select dates, cards from boosterpacks where id = @id", new { id = boosterPack.Id }).ConfigureAwait(false);
            // var datesId = await connection.QuerySingleAsync<int>("select dates from boosterpacks where id = @id", new { id = boosterPack.Id }).ConfigureAwait(false);
            // var cardsId = await connection.QuerySingleAsync<int>("select cards from boosterpacks where id = @id", new { id = boosterPack.Id }).ConfigureAwait(false);

            foreach (var date in boosterPack.Dates)
            {

                date.BoosterPackDatesId = datesId;

                await connection.ExecuteAsync(
                        "upsert_boosterpack_date",
                        new
                        {
                            boosterpackdatesid = date.BoosterPackDatesId,
                            name = date.Name,
                            date = date.Date
                        },
                        commandType: CommandType.StoredProcedure)
                    .ConfigureAwait(false);

            }

            foreach (var card in boosterPack.Cards)
            {

                card.BoosterPackCardsId = cardsId;

                card.Id = await connection.QuerySingleAsync<int>(
                        "insert_or_get_boosterpack_card",
                        new
                        {
                            _boosterpackcardsid = card.BoosterPackCardsId,
                            _name = card.Name
                        },
                        commandType: CommandType.StoredProcedure)
                    .ConfigureAwait(false);

                var raritiesId = await connection.QuerySingleAsync<int>("select rarities from boosterpack_cards where id = @id", new { id = card.Id }).ConfigureAwait(false);

                foreach (var rarity in card.Rarities)
                {

                    await connection.ExecuteAsync(
                            "insert into boosterpack_rarities(boosterpackraritiesid, name) values(@boosterpackraritiesid, @name) on conflict on constraint boosterpack_rarities_boosterpackraritiesid_name_unique_pair do nothing",
                            new
                            {
                                boosterpackraritiesid = raritiesId,
                                name = rarity
                            })
                        .ConfigureAwait(false);

                }

            }

        }

        public async Task InsertErrorAsync(Error error)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.InsertAsync(error).ConfigureAwait(false);

        }

        #endregion Insertions

        public async Task<string> GetCardHashAsync(int id)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var serverHash = await connection.QueryFirstOrDefaultAsync<string>("select hash from card_hashes where id = @id", new { id }).ConfigureAwait(false);

            return serverHash?.Trim();

        }

        #region CardEntity Queries

        public async Task<CardEntity> GetCardAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var card = await connection.QuerySingleCardProcAsync("get_card_exact", new { input }).ConfigureAwait(false);

            return card;

        }

        public async Task<IEnumerable<CardEntity>> SearchCardsAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryCardsProcAsync("search_cards", new { input }).ConfigureAwait(false);

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsContainsAllAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

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

            var cards = await connection.QueryCardsAsync(selector.RawSql, parameters).ConfigureAwait(false);

            return cards;

        }

        public async Task<CardEntity> GetCardFuzzyAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var card = await connection.QuerySingleCardProcAsync("get_card_fuzzy", new { input }).ConfigureAwait(false);

            return card;

        }

        public async Task<CardEntity> GetRandomCardAsync()
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var card = await connection.QuerySingleCardProcAsync("get_random_card").ConfigureAwait(false);

            return card;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInArchetypeAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryCardsProcAsync("get_cards_in_archetype", new { input }).ConfigureAwait(false);

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInSupportAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryCardsProcAsync("get_cards_in_support", new { input }).ConfigureAwait(false);

            return cards;

        }

        public async Task<IEnumerable<CardEntity>> GetCardsInAntisupportAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryCardsProcAsync("get_cards_in_antisupport", new { input }).ConfigureAwait(false);

            return cards;

        }

        #endregion CardEntity Queries

        #region Autocomplete Queries

        public async Task<IEnumerable<string>> GetCardsAutocompleteAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var cards = await connection.QueryProcAsync<string>("get_cards_autocomplete", new { input });

            return cards;

        }

        public async Task<IEnumerable<string>> GetArchetypesAutocompleteAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var archetypes = await connection.QueryProcAsync<string>("get_archetypes_autocomplete", new { input }).ConfigureAwait(false);

            return archetypes;

        }

        public async Task<IEnumerable<string>> GetSupportsAutocompleteAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var supports = await connection.QueryProcAsync<string>("get_supports_autocomplete", new { input }).ConfigureAwait(false);

            return supports;

        }

        public async Task<IEnumerable<string>> GetAntisupportsAutocompleteAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var antisupports = await connection.QueryProcAsync<string>("get_antisupports_autocomplete", new { input }).ConfigureAwait(false);

            return antisupports;

        }

        #endregion Autocomplete Queries

        public async Task<string> GetNameWithPasscodeAsync(string passcode)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var name = await connection.ExecuteScalarAsync<string>("select name from cards where passcode = @passcode", new { passcode }).ConfigureAwait(false);

            return name;

        }

        public async Task<string> GetImageLinkAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var imgLink = await connection.QuerySingleAsync<string>("select img from cards where name ~~* @input", new { input }).ConfigureAwait(false);

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

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            var banlist = new Banlist
            {
                Forbidden = await connection.QueryAsync<string>($"select name from cards where {formatStr} ilike 'forbidden' order by name asc").ConfigureAwait(false),
                Limited = await connection.QueryAsync<string>($"select name from cards where {formatStr} ilike 'limited' order by name asc").ConfigureAwait(false),
                SemiLimited = await connection.QueryAsync<string>($"select name from cards where {formatStr} ilike 'semi-limited' order by name asc").ConfigureAwait(false)
            };

            return banlist;

        }

        public async Task<BoosterPackEntity> GetBoosterPackAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync().ConfigureAwait(false);

            BoosterPackEntity entity = null;
            var dateEntityDict = new Dictionary<string, BoosterPackDateEntity>();
            var cardEntityDict = new Dictionary<string, BoosterPackCardEntity>();

            await connection.QueryAsync<BoosterPackEntity, BoosterPackDateEntity, BoosterPackCardEntity, string, BoosterPackEntity>(
                    "select * from joined_boosterpacks where name ilike @input",
                    (
                        boosterpackEntity,
                        dateEntity,
                        cardEntity,
                        rarity
                    ) =>
                    {

                        if (entity is null)
                        {

                            entity = boosterpackEntity;
                            entity.Cards = new List<BoosterPackCardEntity>();
                            entity.Dates = new List<BoosterPackDateEntity>();

                        }

                        if (!dateEntityDict.TryGetValue(dateEntity.Name, out var dateBpEntity))
                        {

                            dateBpEntity = dateEntity;

                            dateEntityDict[dateEntity.Name] = dateBpEntity;

                        }

                        if (!cardEntityDict.TryGetValue(cardEntity.Name, out var cardBpEntity))
                        {

                            cardBpEntity = cardEntity;
                            cardBpEntity.Rarities = new List<string>();
                            cardEntityDict[cardBpEntity.Name] = cardBpEntity;

                        }

                        if (!entity.Dates.Contains(dateBpEntity))
                            entity.Dates.Add(dateBpEntity);

                        if (!entity.Cards.Contains(cardBpEntity))
                            entity.Cards.Add(cardBpEntity);

                        if (!cardBpEntity.Rarities.Contains(rarity))
                            cardBpEntity.Rarities.Add(rarity);

                        return entity;

                    },
                    new { input },
                    splitOn: "boosterpackdatename,boosterpackcardname,boosterpackrarity"
                )
                .ConfigureAwait(false);

            return entity;

        }

        public async Task<IEnumerable<string>> GetBoosterPacksAutocompleteAsync(string input)
        {

            await using var connection = _config.GetYuGiOhDbConnection();

            await connection.OpenAsync();

            var boosterpacks = await connection.QueryAsync<string>("get_boosterpacks_autocomplete", new { input }, commandType: CommandType.StoredProcedure);

            return boosterpacks;

        }

    }
}