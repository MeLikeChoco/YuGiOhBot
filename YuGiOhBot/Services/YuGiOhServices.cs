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
using Discord;
using Dapper;

namespace YuGiOhBot.Services
{
    public class YuGiOhServices
    {

        private const string DatabasePath = "Data Source=Databases/YgoSqliteDb/ygo.db;";
        private const string CardTable = "Card";
        private const string BasePricesUrl = "http://yugiohprices.com/api/get_card_prices/";
        private const string BaseImagesUrl = "http://yugiohprices.com/api/card_image/";
        private readonly HttpClient _http = new HttpClient();
        
        public ConcurrentDictionary<string, HashSet<string>> OcgBanList { get; private set; }
        public ConcurrentDictionary<string, HashSet<string>> TcgBanList { get; private set; }
        public ConcurrentDictionary<string, HashSet<string>> TrnBanList { get; private set; }

        public EmbedBuilder GetCard(string cardName)
        {

            return CacheService.CardCache.FirstOrDefault(card => card.Key == cardName).Value;

        }

        public EmbedBuilder LazyGetCard(string cardName)
        {

            var array = cardName.Split(' ');
            return CacheService.CardCache.FirstOrDefault(kv => kv.Key.Split(' ').All(str => array.Contains(str))).Value;

        }

        public EmbedBuilder GetRandomCard()
        {

            return CacheService.CardCache.RandomSubset(1).First().Value;

        }

        public List<string> SearchCards(string cardName)
        {

            var cards = CacheService.CardNames.Where(name => name.ToLower().Contains(cardName));
            return cards.ToList();

        }

        public List<string> LazySearchCards(string search)
        {

            var array = search.Split(' ');
            IEnumerable<string> cards = CacheService.CardNames.Where(name => name.ToLower().Split(' ').All(str => array.Contains(str)));

            return cards.ToList();

        }

        public async Task<List<string>> ArchetypeSearch(string search)
        {

            var cards = new List<string>();
            var query = "select name from Card where ";
            search.Split(' ').ForEach(str => query += $"archetype like '%{str}%' and ");
            query = query.Trim().Substring(0, query.Length - 4);

            //i will actually use the database here because i do not want to be parsing the embed description for the archetype
            using (var connection = new SqliteConnection(DatabasePath))
            {

                await connection.OpenAsync();

                var results = await connection.QueryAsync<string>(query);
                cards = results.ToList();

                connection.Close();

            }

            return cards;

        }

        public async Task<YuGiOhPriceSerializer> GetPrices(string cardName, string realName)
        {

            var json = await _http.GetStringAsync($"{BasePricesUrl}{Uri.EscapeUriString(cardName)}");

            if (json.StartsWith("{\"status\":\"fail\""))
            {

                //if card name fails, try with real name
                if (!string.IsNullOrEmpty(realName))
                {

                    json = await _http.GetStringAsync($"{BasePricesUrl}{Uri.EscapeUriString(realName)}");

                    if (json.StartsWith("{\"status\":\"fail\"")) return new YuGiOhPriceSerializer();

                }
                else return new YuGiOhPriceSerializer();

            }

            return JsonConvert.DeserializeObject<YuGiOhPriceSerializer>(json);

        }

        public async Task<string> GetImageUrl(string cardName, string realName)
        {

            HttpResponseMessage response = await _http.GetAsync($"{BaseImagesUrl}{Uri.EscapeUriString(cardName)}");

            if (!response.IsSuccessStatusCode)
            {

                try
                {

                    response = await _http.GetAsync($"{BaseImagesUrl}{realName}");
                    return response.RequestMessage.RequestUri.ToString();

                }
                catch { return response.RequestMessage.RequestUri.ToString(); }

            }

            return response.RequestMessage.RequestUri.ToString();

        }

        public async Task InitializeBanList()
        {

            OcgBanList = new ConcurrentDictionary<string, HashSet<string>>();
            TcgBanList = new ConcurrentDictionary<string, HashSet<string>>();
            TrnBanList = new ConcurrentDictionary<string, HashSet<string>>();
            var forbidden = "Forbidden";
            var limited = "Limited";
            var semiLimited = "Semi-Limited";

            using (var databaseConnection = new SqliteConnection(DatabasePath))
            {

                using (SqliteCommand ocgBanCommand = databaseConnection.CreateCommand())
                {
                    
                    ocgBanCommand.CommandText = "select name,ocgStatus from Card where (not ocgStatus='U') and (not ocgStatus='Not yet released') and (not ocgStatus='Illegal') " +
                        "and (not ocgStatus='Legal') and (not ocgStatus='')";

                    OcgBanList.TryAdd(forbidden, new HashSet<string>());
                    OcgBanList.TryAdd(limited, new HashSet<string>());
                    OcgBanList.TryAdd(semiLimited, new HashSet<string>());

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

                                OcgBanList.TryGetValue(forbidden, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else if (dataReader.GetString(ocgStatusO).Equals(limited))
                            {

                                OcgBanList.TryGetValue(limited, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else
                            {

                                OcgBanList.TryGetValue(semiLimited, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }

                        }

                    }

                }

                using (SqliteCommand tcgBanCommand = databaseConnection.CreateCommand())
                {
                    
                    tcgBanCommand.CommandText = "select name,tcgAdvStatus from Card where (not tcgAdvStatus='U') and (not tcgAdvStatus='Not yet released') and (not tcgAdvStatus='Illegal') " +
                        "and (not tcgAdvStatus='Legal') and (not tcgAdvStatus='')";

                    TcgBanList.TryAdd(forbidden, new HashSet<string>());
                    TcgBanList.TryAdd(limited, new HashSet<string>());
                    TcgBanList.TryAdd(semiLimited, new HashSet<string>());

                    using (SqliteDataReader dataReader = await tcgBanCommand.ExecuteReaderAsync())
                    {

                        int nameO = dataReader.GetOrdinal("name");
                        int tcgStatusO = dataReader.GetOrdinal("tcgAdvStatus");

                        while (await dataReader.ReadAsync())
                        {

                            if (dataReader.GetString(tcgStatusO).Equals(forbidden))
                            {

                                TcgBanList.TryGetValue(forbidden, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else if (dataReader.GetString(tcgStatusO).Equals(limited))
                            {

                                TcgBanList.TryGetValue(limited, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else
                            {

                                TcgBanList.TryGetValue(semiLimited, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }

                        }

                    }

                }

                using (SqliteCommand trnBanCommand = databaseConnection.CreateCommand())
                {

                    trnBanCommand.CommandText = "select name,tcgTrnStatus from Card where (not tcgTrnStatus='U') and (not tcgTrnStatus='Not yet released') and (not tcgTrnStatus='Illegal') " +
                        "and (not tcgTrnStatus='Legal') and (not tcgTrnStatus='')";

                    TrnBanList.TryAdd(forbidden, new HashSet<string>());
                    TrnBanList.TryAdd(limited, new HashSet<string>());
                    TrnBanList.TryAdd(semiLimited, new HashSet<string>());

                    using (SqliteDataReader dataReader = await trnBanCommand.ExecuteReaderAsync())
                    {

                        int nameO = dataReader.GetOrdinal("name");
                        int tcgStatusO = dataReader.GetOrdinal("tcgTrnStatus");

                        while (await dataReader.ReadAsync())
                        {

                            if (dataReader.GetString(tcgStatusO).Equals(forbidden))
                            {

                                TrnBanList.TryGetValue(forbidden, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else if (dataReader.GetString(tcgStatusO).Equals(limited))
                            {

                                TrnBanList.TryGetValue(limited, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }
                            else
                            {

                                TrnBanList.TryGetValue(semiLimited, out HashSet<string> banlist);
                                banlist.Add(dataReader.GetString(nameO));

                            }

                        }

                    }

                }

                databaseConnection.Close();

            }

        }

    }
}
