using AngleSharp.Parser.Html;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MoreLinq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;
using YuGiOhScraper.Parsers;

namespace YuGiOhScraper
{
    public class Program
    {

        private static IDictionary<string, string> _tcg, _ocg;

        public static async Task Main(string[] args)
        {

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://yugioh.wikia.com/") };
            //var links = await GetCardLinks(httpClient);
            IDictionary<string, string> links = (await GetCardLinks(httpClient)).ToDictionary(kv => kv.Key, kv => kv.Value);
            IEnumerable<Card> cards = new List<Card>();
            string retry = "";

            do
            {

                Console.WriteLine("Getting cards...");
                cards = cards.Concat(GetCards(httpClient, links, out links));
                //var cards = GetCards(httpClient, links.ToList().GetRange(0, 100).ToDictionary(kv => kv.Key, kv => kv.Value));

                if (links.Any())
                {

                    Console.WriteLine($"There were {links.Count} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting cards.");

            } while (links.Any() && retry == "y");

            await CardsToSqlite(cards);

            Console.ReadKey();

        }

        //private static Task CardsToSqlite()
        //    => CardsToSqlite(null);

        private static async Task CardsToSqlite(IEnumerable<Card> cards)
        {

            if (File.Exists("ygo.db"))
                File.Delete("ygo.db");

            using (var db = new SqliteConnection("Data Source = ygo.db"))
            {

                await db.OpenAsync();

                var createTable = db.CreateCommand();
                createTable.CommandText = "CREATE TABLE 'Cards'(" +
                    "'Name' TEXT, " +
                    "'RealName' TEXT, " +
                    "'Passcode' TEXT, " +
                    "'CardType' TEXT, " +
                    "'Property' TEXT, " +
                    "'Level' INTEGER, " +
                    "'PendulumScale' INTEGER, " +
                    "'Rank' INTEGER, " +
                    "'Link' INTEGER, " +
                    "'LinkArrows' TEXT, " +
                    "'Types' TEXT, " +
                    "'Attribute' TEXT, " +
                    "'Materials' TEXT, " +
                    "'Lore' TEXT, " +
                    "'Atk' TEXT, " +
                    "'Def' TEXT, " +
                    "'Archetype' TEXT, " +
                    "'Supports' TEXT, " +
                    "'AntiSupports' TEXT, " +
                    "'OcgExists' INTEGER, " +
                    "'TcgExists' INTEGER, " +
                    "'OcgStatus' TEXT, " +
                    "'TcgAdvStatus' TEXT, " +
                    "'TcgTrnStatus' TEXT, " +
                    "'Img' TEXT, " +
                    "'Url' TEXT " +
                    ")";

                Console.WriteLine("Saving to ygo.db...");
                await createTable.ExecuteNonQueryAsync();
                await db.InsertAsync(cards);
                Console.WriteLine("Done saving to ygo.db.");

                db.Close();

            }

        }

        private static IEnumerable<Card> GetCards(HttpClient httpClient, IDictionary<string, string> links, out IDictionary<string, string> errors)
        {

            var cards = new ConcurrentBag<Card>();
            var total = links.Count;
            var current = 0;
            var pOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            var tempErrors = new ConcurrentDictionary<string,string>();

            Parallel.ForEach(links, pOptions, link =>
            {

                var response = httpClient.GetStringAsync(link.Value).Result;

                try
                {

                    var card = new CardParser(link.Key, response).Parse();

                    #region OCG TCG
                    //c# int default is 0, therefore, if only one of them is 1, that means it is an format exclusive card
                    //if both of them are 1, then it is in both formats
                    if (_tcg.ContainsKey(card.Name))
                        card.TcgExists = true;

                    if (_ocg.ContainsKey(card.Name))
                        card.OcgExists = true;
                    #endregion OCG TCG

                    card.Url = $"http://yugioh.wikia.com{link.Value}";

                    cards.Add(card);

                }
                catch (Exception)
                {

                    tempErrors[link.Key] = link.Value;

                }

                var counter = Interlocked.Increment(ref current);

                Console.Write($"Progress: {counter}/{total} ({(counter / (double)total) * 100}%)\r");

            });

            errors = tempErrors;

            return cards;

        }

        //there are two ways we could have done this
        //1. assume the guy using this has bad internet
        //2. assume the guy using this doesn't care about it
        //I'll go with 2 to make my life easier
        //I also most likely don't need parallel foreach, but whatever
        private static async Task<IDictionary<string, string>> GetCardLinks(HttpClient httpClient)
        {

            _tcg = new ConcurrentDictionary<string, string>();
            _ocg = new ConcurrentDictionary<string, string>();

            var responseTcg = await httpClient.GetStringAsync("api/v1/Articles/List?category=TCG_cards&limit=1000000&namespaces=0"); //I have to use a really high number or else some cards won't show up, its so bizarre...
            var responseOcg = await httpClient.GetStringAsync("api/v1/Articles/List?category=OCG_cards&limit=1000000&namespaces=0");
            var json = JObject.Parse(responseTcg);

            Parallel.ForEach(json["items"].ToObject<JArray>(), item => _tcg[item.Value<string>("title")] = item.Value<string>("url"));

            json = JObject.Parse(responseOcg);

            Parallel.ForEach(json["items"].ToObject<JArray>(), item => _ocg[item.Value<string>("title")] = item.Value<string>("url"));

            return _tcg.Concat(_ocg).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

    }
}
