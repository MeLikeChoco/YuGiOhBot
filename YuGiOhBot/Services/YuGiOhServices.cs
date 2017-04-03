using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using YuGiOhBot.Core;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using MoreLinq;
using YuGiOhBot.Services.CardObjects;
using Discord.WebSocket;

namespace YuGiOhBot.Services
{
    public class YuGiOhServices
    {

        private const string DatabasePath = "Data Source=Databases/YgoSqliteDb/ygo.db;";
        private const string CardTable = "Card";
        private const string BasePricesUrl = "http://yugiohprices.com/api/get_card_prices/";
        private const string BaseImagesUrl = "http://yugiohprices.com/api/card_image/";
        public List<string> CardList { get; private set; }
        public ConcurrentDictionary<string, List<string>> OcgBanList { get; private set; }
        public ConcurrentDictionary<string, List<string>> TcgBanList { get; private set; }
        public ConcurrentDictionary<string, List<string>> TrnBanList { get; private set; }

        public async Task<YuGiOhCard> GetCard(string cardName)
        {

            //til that you can do something like this
            string name, realName, attribute, types, cardType, level, atk, def, rank, pendScale,
                linkMarkers, link, property, lore, archetype, ocgStatus, tcgAdvStatus, tcgTrnStatus;
            bool ocgOnly = false, tcgOnly = false;
            name = realName = attribute = types = cardType = level = atk = def = rank = pendScale =
                linkMarkers = link = property = lore = archetype = ocgStatus = tcgAdvStatus = tcgTrnStatus = string.Empty;

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand getCardCommand = databaseConnection.CreateCommand())
            {

                getCardCommand.CommandText = $"select * from Card where (name like @NAME) or (realName like @NAME)";
                getCardCommand.Parameters.Add("@NAME", SqliteType.Text);
                getCardCommand.Parameters["@NAME"].Value = cardName;

                await databaseConnection.OpenAsync();

                using (SqliteDataReader dataReader = await getCardCommand.ExecuteReaderAsync())
                {

                    if (!dataReader.HasRows)
                    {

                        databaseConnection.Close();
                        return new YuGiOhCard();

                    }

                    await dataReader.ReadAsync();

                    //eh, i was stupid to not use ordinals, but whatever, this works too
                    name = dataReader["name"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("realName"))) realName = dataReader["realName"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("attribute"))) attribute = dataReader["attribute"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("cardType"))) cardType = dataReader["cardType"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("types"))) types = dataReader["types"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("level"))) level = dataReader["level"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("atk"))) atk = dataReader["atk"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("def"))) def = dataReader["def"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("rank"))) rank = dataReader["rank"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("pendulumScale"))) pendScale = dataReader["pendulumScale"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("link"))) link = dataReader["link"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("linkMarkers"))) linkMarkers = dataReader["linkMarkers"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("property"))) property = dataReader["property"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("lore"))) lore = dataReader["lore"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("archetype"))) archetype = dataReader["archetype"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgStatus"))) ocgStatus = dataReader["ocgStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgAdvStatus"))) tcgAdvStatus = dataReader["tcgAdvStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgTrnStatus"))) tcgTrnStatus = dataReader["tcgTrnStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgOnly"))) ocgOnly = dataReader["ocgOnly"].ToString().Equals("1");
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgOnly"))) tcgOnly = dataReader["tcgOnly"].ToString().Equals("1");

                }

                databaseConnection.Close();

            }

            if (cardType.Equals("Monster"))
            {

                if (types.Contains("Link"))
                {

                    var card = new LinkMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Links = link,
                        LinkMarkers = linkMarkers,
                        Atk = atk,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }
                else if (types.Contains("Xyz"))
                {

                    var card = new XyzMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Rank = rank,
                        Atk = atk,
                        Def = def,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }else if (types.Contains("Pendulum"))
                {

                    var card = new PendulumMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Level = level,
                        PendulumScale = pendScale,
                        Atk = atk,
                        Def = def,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }else if (types.Contains("Link"))
                {

                    var card = new LinkMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Links = link,
                        LinkMarkers = linkMarkers,
                        Atk = atk,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                }
                else
                {

                    var card = new RegularMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Level = level,
                        Lore = lore,
                        Atk = atk,
                        Def = def,
                        HasEffect = types.Contains("Effect"),
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }

            }
            else
            {

                var card = new SpellTrapCard()
                {
                    Name = name,
                    RealName = realName,
                    CardType = cardType,
                    Property = property,
                    Lore = lore,
                    Archetype = archetype,
                    TcgOnly = tcgOnly,
                    OcgOnly = ocgOnly,
                    ImageUrl = await GetImageUrl(name, realName),
                    Prices = await GetPrices(name, realName),
                };

                return card;

            }

            return new YuGiOhCard();

        }

        public async Task<YuGiOhCard> LazyGetCard(string cardName)
        {

            var searchTerms = cardName.Split(' ');

            //til that you can do something like this
            string name, realName, attribute, types, cardType, level, atk, def, rank, pendScale,
               linkMarkers, link, property, lore, archetype, ocgStatus, tcgAdvStatus, tcgTrnStatus;
            bool ocgOnly = false, tcgOnly = false;
            name = realName = attribute = types = cardType = level = atk = def = rank = pendScale =
                linkMarkers = link = property = lore = archetype = ocgStatus = tcgAdvStatus = tcgTrnStatus = string.Empty;

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand getCardCommand = databaseConnection.CreateCommand())
            {

                var buildCommand = new StringBuilder("select * from Card where ");

                var lastTerm = searchTerms.Last();
                foreach (var term in searchTerms)
                {

                    if (!term.Equals(lastTerm)) buildCommand = buildCommand.Append($"(name like '%{term}%') and ");
                    else buildCommand = buildCommand.Append($"(name like '%{term}%')");

                }

                getCardCommand.CommandText = buildCommand.ToString();

                await databaseConnection.OpenAsync();

                using (SqliteDataReader dataReader = await getCardCommand.ExecuteReaderAsync())
                {

                    if (!dataReader.HasRows)
                    {

                        databaseConnection.Close();
                        return new YuGiOhCard();

                    }

                    await dataReader.ReadAsync();

                    //eh, i was stupid to not use ordinals, but whatever, this works too
                    name = dataReader["name"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("realName"))) realName = dataReader["realName"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("attribute"))) attribute = dataReader["attribute"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("cardType"))) cardType = dataReader["cardType"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("types"))) types = dataReader["types"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("level"))) level = dataReader["level"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("atk"))) atk = dataReader["atk"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("def"))) def = dataReader["def"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("rank"))) rank = dataReader["rank"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("pendulumScale"))) pendScale = dataReader["pendulumScale"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("link"))) link = dataReader["link"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("linkMarkers"))) linkMarkers = dataReader["linkMarkers"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("property"))) property = dataReader["property"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("lore"))) lore = dataReader["lore"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("archetype"))) archetype = dataReader["archetype"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgStatus"))) ocgStatus = dataReader["ocgStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgAdvStatus"))) tcgAdvStatus = dataReader["tcgAdvStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgTrnStatus"))) tcgTrnStatus = dataReader["tcgTrnStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgOnly"))) ocgOnly = dataReader["ocgOnly"].ToString().Equals("1");
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgOnly"))) tcgOnly = dataReader["tcgOnly"].ToString().Equals("1");

                }

                databaseConnection.Close();

            }

            if (cardType.Equals("Monster"))
            {

                if (types.Contains("Link"))
                {

                    var card = new LinkMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Links = link,
                        LinkMarkers = linkMarkers,
                        Atk = atk,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }
                else if (types.Contains("Xyz"))
                {

                    var card = new XyzMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Rank = rank,
                        Atk = atk,
                        Def = def,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }
                else if (types.Contains("Pendulum"))
                {

                    var card = new PendulumMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Level = level,
                        PendulumScale = pendScale,
                        Atk = atk,
                        Def = def,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }
                else if (types.Contains("Link"))
                {

                    var card = new LinkMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Links = link,
                        LinkMarkers = linkMarkers,
                        Atk = atk,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                }
                else
                {

                    var card = new RegularMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Level = level,
                        Lore = lore,
                        Atk = atk,
                        Def = def,
                        HasEffect = types.Contains("Effect"),
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }

            }
            else
            {

                var card = new SpellTrapCard()
                {
                    Name = name,
                    RealName = realName,
                    CardType = cardType,
                    Property = property,
                    Lore = lore,
                    Archetype = archetype,
                    TcgOnly = tcgOnly,
                    OcgOnly = ocgOnly,
                    ImageUrl = await GetImageUrl(name, realName),
                    Prices = await GetPrices(name, realName),
                };

                return card;

            }

            return new YuGiOhCard();

        }

        public async Task<YuGiOhCard> GetRandomCard()
        {

            string name, realName, attribute, types, cardType, level, atk, def, rank, pendScale,
               linkMarkers, link, property, lore, archetype, ocgStatus, tcgAdvStatus, tcgTrnStatus;
            bool ocgOnly = false, tcgOnly = false;
            name = realName = attribute = types = cardType = level = atk = def = rank = pendScale =
                linkMarkers = link = property = lore = archetype = ocgStatus = tcgAdvStatus = tcgTrnStatus = string.Empty;

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand randomCommand = databaseConnection.CreateCommand())
            {

                randomCommand.CommandText = "select * from Card order by Random() limit 1";

                await databaseConnection.OpenAsync();

                using (SqliteDataReader dataReader = await randomCommand.ExecuteReaderAsync())
                {

                    await dataReader.ReadAsync();

                    name = dataReader["name"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("realName"))) realName = dataReader["realName"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("attribute"))) attribute = dataReader["attribute"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("cardType"))) cardType = dataReader["cardType"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("types"))) types = dataReader["types"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("level"))) level = dataReader["level"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("atk"))) atk = dataReader["atk"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("def"))) def = dataReader["def"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("rank"))) rank = dataReader["rank"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("pendulumScale"))) pendScale = dataReader["pendulumScale"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("link"))) link = dataReader["link"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("linkMarkers"))) linkMarkers = dataReader["linkMarkers"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("property"))) property = dataReader["property"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("lore"))) lore = dataReader["lore"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("archetype"))) archetype = dataReader["archetype"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgStatus"))) ocgStatus = dataReader["ocgStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgAdvStatus"))) tcgAdvStatus = dataReader["tcgAdvStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgTrnStatus"))) tcgTrnStatus = dataReader["tcgTrnStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgOnly"))) ocgOnly = dataReader["ocgOnly"].ToString().Equals("1");
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgOnly"))) tcgOnly = dataReader["tcgOnly"].ToString().Equals("1");

                }

                databaseConnection.Close();

            }

            if (cardType.Equals("Monster"))
            {

                if (types.Contains("Link"))
                {

                    var card = new LinkMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Links = link,
                        LinkMarkers = linkMarkers,
                        Atk = atk,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }
                else if (types.Contains("Xyz"))
                {

                    var card = new XyzMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Rank = rank,
                        Atk = atk,
                        Def = def,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }
                else if (types.Contains("Pendulum"))
                {

                    var card = new PendulumMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Level = level,
                        PendulumScale = pendScale,
                        Atk = atk,
                        Def = def,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }
                else if (types.Contains("Link"))
                {

                    var card = new LinkMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Links = link,
                        LinkMarkers = linkMarkers,
                        Atk = atk,
                        Lore = lore,
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        HasEffect = types.Contains("Effect"),
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                }
                else
                {

                    var card = new RegularMonster()
                    {
                        Name = name,
                        RealName = realName,
                        Attribute = attribute,
                        CardType = cardType,
                        Types = types,
                        Level = level,
                        Lore = lore,
                        Atk = atk,
                        Def = def,
                        HasEffect = types.Contains("Effect"),
                        TcgOnly = tcgOnly,
                        OcgOnly = ocgOnly,
                        Archetype = archetype,
                        ImageUrl = await GetImageUrl(name, realName),
                        Prices = await GetPrices(name, realName),
                    };

                    return card;

                }

            }
            else
            {

                var card = new SpellTrapCard()
                {
                    Name = name,
                    RealName = realName,
                    CardType = cardType,
                    Property = property,
                    Lore = lore,
                    Archetype = archetype,
                    TcgOnly = tcgOnly,
                    OcgOnly = ocgOnly,
                    ImageUrl = await GetImageUrl(name, realName),
                    Prices = await GetPrices(name, realName),
                };

                return card;

            }

            return new YuGiOhCard();

        }

        public async Task<string> LazyGetCardName(string cardName)
        {

            var searchTerms = cardName.Split(' ');

            using (var dbConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand searchCmd = dbConnection.CreateCommand())
            {

                var buildCommand = new StringBuilder("select name from Card where ");

                var lastTerm = searchTerms.Last();
                foreach (var term in searchTerms)
                {

                    if (!term.Equals(lastTerm)) buildCommand = buildCommand.Append($"(name like '%{term}%') and ");
                    else buildCommand = buildCommand.Append($"(name like '%{term}%') ");

                }

                buildCommand.Append($"order by name limit 1");

                searchCmd.CommandText = buildCommand.ToString();

                await dbConnection.OpenAsync();
                string closestCard = (await searchCmd.ExecuteScalarAsync())?.ToString();

                dbConnection.Close();

                return closestCard;

            }

        }

        public async Task<List<string>> SearchCards(string cardName)
        {

            var searchResults = new List<string>(15); //I think 15 is a good initial capacity

            using(var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand searchCommand = databaseConnection.CreateCommand())
            {

                var unintentionalSanitization = cardName.Replace(" ", "%");
                searchCommand.CommandText = $"select name from Card where (name like '%{unintentionalSanitization}%') or (realName like '%{unintentionalSanitization}%')";

                await databaseConnection.OpenAsync();

                using (SqliteDataReader dataReader = await searchCommand.ExecuteReaderAsync())
                {

                    if (dataReader.HasRows)
                    {

                        int nameOrdinal = dataReader.GetOrdinal("name");

                        while (await dataReader.ReadAsync())
                        {

                            searchResults.Add(dataReader.GetString(nameOrdinal)); 

                        }

                    }

                }

                databaseConnection.Close();

            }
            
            return searchResults;

        }

        public async Task<List<string>> LazySearchCards(string search)
        {

            var searchTerms = search.Split(' ');
            var searchResults = new List<string>();

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand searchCommand = databaseConnection.CreateCommand())
            {

                var buildCommand = new StringBuilder("select name from Card where ");

                var lastTerm = searchTerms.Last();
                foreach(var term in searchTerms)
                {

                    if(!term.Equals(lastTerm)) buildCommand = buildCommand.Append($"(name like '%{term}%') and ");
                    else buildCommand = buildCommand.Append($"(name like '%{term}%')");

                }

                searchCommand.CommandText = buildCommand.ToString();

                await databaseConnection.OpenAsync();

                using (SqliteDataReader dataReader = await searchCommand.ExecuteReaderAsync())
                {

                    if (dataReader.HasRows)
                    {

                        int nameOrdinal = dataReader.GetOrdinal("name");

                        while(await dataReader.ReadAsync())
                        {

                            searchResults.Add(dataReader.GetString(nameOrdinal));

                        }

                    }

                }

                databaseConnection.Close();

            }

            return searchResults;

        }

        public async Task<List<string>> ArchetypeSearch(string search)
        {

            var results = new List<string>(15);

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand archetypeCommand = databaseConnection.CreateCommand())
            {

                archetypeCommand.CommandText = $"select name from Card where archetype like '%{search}%'";

                await databaseConnection.OpenAsync();

                using (SqliteDataReader dataReader = await archetypeCommand.ExecuteReaderAsync())
                {

                    if (!dataReader.HasRows) return results;

                    int ordinal = dataReader.GetOrdinal("name");

                    while (await dataReader.ReadAsync())
                    {

                        results.Add(dataReader.GetString(ordinal));

                    }

                }

                databaseConnection.Close();

            }

            return results;

        }

        private async Task<YuGiOhPriceSerializer> GetPrices(string cardName, string realName)
        {

            using (var http = new HttpClient())
            {

                var json = await http.GetStringAsync($"{BasePricesUrl}{Uri.EscapeUriString(cardName)}");

                if (json.StartsWith("{\"status\":\"fail\""))
                {
                    
                    //if card name fails, try with real name
                    if (!string.IsNullOrEmpty(realName))
                    {

                        json = await http.GetStringAsync($"{BasePricesUrl}{Uri.EscapeUriString(realName)}");

                        if (json.StartsWith("{\"status\":\"fail\"")) return new YuGiOhPriceSerializer();

                    }else return new YuGiOhPriceSerializer();

                }

                return JsonConvert.DeserializeObject<YuGiOhPriceSerializer>(json);

            }

        }

        private async Task<string> GetImageUrl(string cardName, string realName)
        {

            //redirects are annoying eh?
            using (var http = new HttpClient())
            {

                HttpResponseMessage response = await http.GetAsync($"{BaseImagesUrl}{cardName}");

                if (!response.IsSuccessStatusCode)
                {

                    try
                    {

                        response = await http.GetAsync($"{BaseImagesUrl}{realName}");
                        return response.RequestMessage.RequestUri.ToString();

                    }
                    catch { return response.RequestMessage.RequestUri.ToString(); }

                }

                return response.RequestMessage.RequestUri.ToString();

            }

        }

        public async Task InitializeBanList()
        {

            OcgBanList = new ConcurrentDictionary<string, List<string>>();
            TcgBanList = new ConcurrentDictionary<string, List<string>>();
            TrnBanList = new ConcurrentDictionary<string, List<string>>();
            var forbidden = "Forbidden";
            var limited = "Limited";
            var semiLimited = "Semi-Limited";

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            {

                using (SqliteCommand ocgBanCommand = databaseConnection.CreateCommand())
                {

                    //first read the ocg ban list
                    ocgBanCommand.CommandText = "select name,ocgStatus from Card where (not ocgStatus='U') and (not ocgStatus='Not yet released') and (not ocgStatus='Illegal') " +
                        "and (not ocgStatus='Legal') and (not ocgStatus='')";

                    OcgBanList.TryAdd(forbidden, new List<string>());
                    OcgBanList.TryAdd(limited, new List<string>());
                    OcgBanList.TryAdd(semiLimited, new List<string>());

                    //open the database here
                    await databaseConnection.OpenAsync();

                    using (SqliteDataReader dataReader = await ocgBanCommand.ExecuteReaderAsync())
                    {

                        int nameO = dataReader.GetOrdinal("name");
                        int ocgStatusO = dataReader.GetOrdinal("ocgStatus");

                        while (await dataReader.ReadAsync())
                        {

                            if (dataReader.GetString(ocgStatusO).Equals(forbidden))
                            {

                                OcgBanList.TryGetValue(forbidden, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else if (dataReader.GetString(ocgStatusO).Equals(limited))
                            {

                                OcgBanList.TryGetValue(limited, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else
                            {

                                OcgBanList.TryGetValue(semiLimited, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }

                        }

                    }

                }

                using (SqliteCommand tcgBanCommand = databaseConnection.CreateCommand())
                {

                    //first read the ocg ban list
                    tcgBanCommand.CommandText = "select name,tcgAdvStatus from Card where (not tcgAdvStatus='U') and (not tcgAdvStatus='Not yet released') and (not tcgAdvStatus='Illegal') " +
                        "and (not tcgAdvStatus='Legal') and (not tcgAdvStatus='')";

                    TcgBanList.TryAdd(forbidden, new List<string>());
                    TcgBanList.TryAdd(limited, new List<string>());
                    TcgBanList.TryAdd(semiLimited, new List<string>());

                    using (SqliteDataReader dataReader = await tcgBanCommand.ExecuteReaderAsync())
                    {

                        int nameO = dataReader.GetOrdinal("name");
                        int tcgStatusO = dataReader.GetOrdinal("tcgAdvStatus");

                        while (await dataReader.ReadAsync())
                        {

                            if (dataReader.GetString(tcgStatusO).Equals(forbidden))
                            {

                                TcgBanList.TryGetValue(forbidden, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else if (dataReader.GetString(tcgStatusO).Equals(limited))
                            {

                                TcgBanList.TryGetValue(limited, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else
                            {

                                TcgBanList.TryGetValue(semiLimited, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }

                        }

                    }

                }

                using (SqliteCommand trnBanCommand = databaseConnection.CreateCommand())
                {

                    //first read the ocg ban list
                    trnBanCommand.CommandText = "select name,tcgTrnStatus from Card where (not tcgTrnStatus='U') and (not tcgTrnStatus='Not yet released') and (not tcgTrnStatus='Illegal') " +
                        "and (not tcgTrnStatus='Legal') and (not tcgTrnStatus='')";

                    TrnBanList.TryAdd(forbidden, new List<string>());
                    TrnBanList.TryAdd(limited, new List<string>());
                    TrnBanList.TryAdd(semiLimited, new List<string>());

                    using (SqliteDataReader dataReader = await trnBanCommand.ExecuteReaderAsync())
                    {

                        int nameO = dataReader.GetOrdinal("name");
                        int tcgStatusO = dataReader.GetOrdinal("tcgTrnStatus");

                        while (await dataReader.ReadAsync())
                        {

                            if (dataReader.GetString(tcgStatusO).Equals(forbidden))
                            {

                                TrnBanList.TryGetValue(forbidden, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else if (dataReader.GetString(tcgStatusO).Equals(limited))
                            {

                                TrnBanList.TryGetValue(limited, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else
                            {

                                TrnBanList.TryGetValue(semiLimited, out List<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }

                        }

                    }

                }

                databaseConnection.Close();

            }

        }

        public async Task InitializeCardList()
        {

            CardList = new List<string>();

            using (var dbConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand addCardCommand = dbConnection.CreateCommand())
            {

                addCardCommand.CommandText = "select name from Card";

                await dbConnection.OpenAsync();

                using (SqliteDataReader dataReader = await addCardCommand.ExecuteReaderAsync())
                {

                    int nameOrd = dataReader.GetOrdinal("name");

                    while(await dataReader.ReadAsync())
                        CardList.Add(dataReader.GetString(nameOrd).ToLower());

                }

                dbConnection.Close();

            }

            await AltConsole.PrintAsync("Service", "YuGiOh", $"There are {CardList.Count} cards!");

        }

    }
}
