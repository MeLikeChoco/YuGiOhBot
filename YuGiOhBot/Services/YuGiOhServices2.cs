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

namespace YuGiOhBot.Services
{
    public class YuGiOhServices2
    {

        private const string DatabasePath = "Data Source=Databases/YgoSqliteDbygo.db;";
        private const string CardTable = "texts";
        private const string DataTable = "datas";
        private const string RegexString = "<[^>]*>";
        private const string BasePricesUrl = "http://yugiohprices.com/api/get_card_prices/";
        private const string BaseImagesUrl = "http://yugiohprices.com/api/card_image/";

        public async Task<YuGiOhCard> GetCard(string cardName)
        {

            var card = new YuGiOhCard();

            //til that you can do something like this
            string name, realName, attribute, cardType, level, atk, def, rank, pendScale,
                linkMarkers, link, property, lore, ocgStatus, tcgAdvStatus, tcgTrnStatus;
            bool ocgOnly = false, tcgOnly = false;
            List<string> types = new List<string>();
            name = realName = attribute = cardType = level = atk = def = rank = pendScale =
                linkMarkers = link = property = lore = ocgStatus = tcgAdvStatus = tcgTrnStatus = string.Empty;

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand getCardCommand = databaseConnection.CreateCommand())
            {

                getCardCommand.CommandText = $"select * from Card where name like @NAME";
                getCardCommand.Parameters.Add("@NAME", SqliteType.Text);
                getCardCommand.Parameters["@NAME"].Value = cardName;

                await databaseConnection.OpenAsync();

                using (SqliteDataReader dataReader = await getCardCommand.ExecuteReaderAsync())
                {

                    if (!dataReader.HasRows)
                    {

                        databaseConnection.Close();
                        return card;

                    }

                    await dataReader.ReadAsync();

                    //eh, i was stupid to not use ordinals, but whatever, this works too
                    name = dataReader["name"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("realName"))) realName = dataReader["realName"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("attribute"))) attribute = dataReader["attribute"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("cardType"))) cardType = dataReader["cardType"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("types"))) types = dataReader["types"].ToString().Split('/').Select(aType => aType.Trim()).ToList();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("level"))) level = dataReader["level"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("atk"))) atk = dataReader["atk"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("def"))) def = dataReader["def"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("rank"))) rank = dataReader["rank"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("pendulumScale"))) pendScale = dataReader["pendulumScale"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("link"))) link = dataReader["link"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("property"))) property = dataReader["rank"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("lore"))) lore = dataReader["lore"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgStatus"))) rank = dataReader["ocgStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgAdvStatus"))) tcgAdvStatus = dataReader["tcgAdvStatus"].ToString();
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("ocgOnly"))) ocgOnly = true; //both values will always have a 1 if true, therefore no need to check
                    if (!await dataReader.IsDBNullAsync(dataReader.GetOrdinal("tcgTrnStatus"))) tcgOnly = true; //if the column in the current row has a value, it will ALWAYS be 1

                }

                databaseConnection.Close();

            }

            if (tcgOnly) card.Format = "TCG";
            else if (ocgOnly) card.Format = "OCG";
            else card.Format = "TCG/OCG";

            card.Name = name;
            var rgx = new Regex(RegexString);
            card.Description = rgx.Replace(lore, string.Empty);
            card.IsEffect = cardType.Equals("Trap") || cardType.Equals("Spell") || types.Contains("Effect") || types.Contains("Pendulum") ? true : false;
            card.Level = level;
            card.LeftPend = pendScale;
            card.RightPend = pendScale;
            card.Types = types;
            card.Atk = atk;
            card.Def = def;
            card.Race = types.Count > 0 ? types.FirstOrDefault() : string.Empty; //it will always be the first thing declared in types
            card.Attribute = attribute;
            card.Prices = await GetPrices(name, realName);
            card.ImageUrl = await GetImageUrl(name, realName);

            return card;

        }

        public async Task<List<string>> SearchCards(string cardName, bool IsArchetypeSearch)
        {

            var searchResults = new List<string>();

            using(var databaseConnection = new SqliteConnection(DatabasePath))
            using (SqliteCommand searchCommand = databaseConnection.CreateCommand())
            {

                var unintentionalSanitization = cardName.Replace(" ", "%");
                searchCommand.CommandText = IsArchetypeSearch ? $"select name from Card where (name like {unintentionalSanitization}) or (lore like {unintentionalSanitization})" : "select name from Card where name like @NAME";

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

                    if(!term.Equals(lastTerm)) buildCommand = buildCommand.Append($"(name like %{term}%) ");
                    else buildCommand = buildCommand.Append($"(name like %{term}%)");

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
