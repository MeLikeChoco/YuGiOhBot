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
using YuGiOhBot.Services.CardObjects;

namespace YuGiOhBot.Services
{
    public class YuGiOhServices
    {

        private const string DatabasePath = "Data Source=Databases/YgoSqliteDb/ygo.db;";
        private const string CardTable = "Card";
        private const string BasePricesUrl = "http://yugiohprices.com/api/get_card_prices/";
        private const string BaseImagesUrl = "http://yugiohprices.com/api/card_image/";

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

                //if(types.Contains("Link"))
                if (types.Contains("Xyz"))
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

                //if(types.Contains("Link"))
                if (types.Contains("Xyz"))
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
                    HasEffect = true,
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

            throw new NotImplementedException();

        }

        public async Task<List<string>> SearchCards(string cardName, bool IsArchetypeSearch)
        {

            var searchResults = new List<string>(15); //I think 15 is a good initial capacity

            using(var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand searchCommand = databaseConnection.CreateCommand())
            {

                var unintentionalSanitization = cardName.Replace(" ", "%");
                searchCommand.CommandText = IsArchetypeSearch ? $"select name from Card where (name like '%{unintentionalSanitization}%') or (lore like '%{unintentionalSanitization}%') or (realName like '%{unintentionalSanitization}%'" : $"select name from Card where (name like '%{unintentionalSanitization}%') or (realName like '%{unintentionalSanitization}%')";

                await databaseConnection.OpenAsync();

                using(SqliteDataReader dataReader = await searchCommand.ExecuteReaderAsync())
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

    }
}
