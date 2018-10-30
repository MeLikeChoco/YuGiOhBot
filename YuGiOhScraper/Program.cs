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
using System.Runtime.CompilerServices;
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
        private static bool _wasLaunchedByProgram = false;

        public static async Task Main(string[] args)
        {

            if (args.Any() && int.TryParse(args.FirstOrDefault(), out var result))
                _wasLaunchedByProgram = result == 1;
            //var links = await GetCardLinks(httpClient);
            IDictionary<string, string> links = (await GetCardLinks()).ToDictionary(kv => kv.Key, kv => kv.Value);

            Console.WriteLine($"There are {links.Count} cards to parse.");

            IEnumerable<Card> cards = new List<Card>();
            string retry = "";

            do
            {

                Console.WriteLine("Getting cards...");
                cards = cards.Concat(GetCards(links, out links));
                //var cards = GetCards(httpClient, links.ToList().GetRange(0, 100).ToDictionary(kv => kv.Key, kv => kv.Value));

                if (links.Any() && !_wasLaunchedByProgram)
                {

                    Console.WriteLine($"There were {links.Count} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting cards.");

            } while (links.Any() && retry == "y" && !_wasLaunchedByProgram);

            await CardsToSqlite(cards, links.Keys);

            if (!_wasLaunchedByProgram)
                Console.ReadKey();

        }

        //private static Task CardsToSqlite()
        //    => CardsToSqlite(null);

        private static async Task CardsToSqlite(IEnumerable<Card> cards, IEnumerable<string> errors)
        {

            if (File.Exists("ygo.db"))
                File.Delete("ygo.db");

            using (var db = new SqliteConnection("Data Source = ygo.db"))
            {

                await db.OpenAsync();

                SqliteCommand createCardTable, createCardErrorTable;
                createCardTable = createCardErrorTable = null;

                createCardTable = db.CreateCommand();
                createCardTable.CommandText = "CREATE TABLE 'Cards'(" +
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

                if (_wasLaunchedByProgram)
                {

                    createCardErrorTable = db.CreateCommand();
                    createCardErrorTable.CommandText = "CREATE TABLE 'CardErrors' (" +
                        "'Name' TEXT, " +
                        "'Exception' TEXT " +
                        ")";

                }

                Console.WriteLine("Saving to ygo.db...");
                await createCardTable.ExecuteNonQueryAsync();

                if (_wasLaunchedByProgram)
                    await createCardErrorTable.ExecuteNonQueryAsync();

                await db.InsertAsync(cards);

                if (_wasLaunchedByProgram)
                {

                    var errorToEntity = errors.Select(name => new CardError { Name = name });

                    await db.InsertAsync(errorToEntity);

                }

                Console.WriteLine("Finished saving to ygo.db.");

                db.Close();

            }

        }

        private static IEnumerable<Card> GetCards(IDictionary<string, string> links, out IDictionary<string, string> errors)
        {

            var cards = new ConcurrentBag<Card>();
            var total = links.Count;
            var current = 0;
            var pOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            var tempErrors = new ConcurrentDictionary<string, string>();

            Parallel.ForEach(links, pOptions, kv =>
            {

                try
                {

                    var card = new CardParser(kv.Key, $"http://yugioh.wikia.com{kv.Value}").Parse();

                    #region OCG TCG
                    //c# int default is 0, therefore, if only one of them is 1, that means it is an format exclusive card
                    //if both of them are 1, then it is in both formats
                    if (_tcg.ContainsKey(card.Name))
                        card.TcgExists = true;

                    if (_ocg.ContainsKey(card.Name))
                        card.OcgExists = true;
                    #endregion OCG TCG

                    cards.Add(card);

                }
                catch (Exception)
                {

                    tempErrors[kv.Key] = kv.Value;

                }

                var counter = Interlocked.Increment(ref current);

                if (!_wasLaunchedByProgram)
                    InlineWrite($"Progress: {counter}/{total} ({(counter / (double)total) * 100}%)");

            });

            //go to next line after loop
            Console.WriteLine();

            errors = tempErrors;

            return cards;

        }

        //private static object _inlineLock = new object();
        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void InlineWrite(string message)
        {

            //lock (_inlineLock)
            //{

            //    int currentLineCursor = Console.CursorTop;
            //    Console.SetCursorPosition(0, Console.CursorTop);
            //    Console.Write(new string(' ', Console.WindowWidth));
            //    Console.SetCursorPosition(0, currentLineCursor);
            //    //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            //    Console.Write(message);

            //}

            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            Console.Write(message);


        }

        //there are two ways we could have done this
        //1. assume the guy using this has bad internet
        //2. assume the guy using this doesn't care about it
        //I'll go with 2 to make my life easier
        //I also most likely don't need parallel foreach, but whatever
        private static async Task<IDictionary<string, string>> GetCardLinks()
        {

            string responseTcg, responseOcg;
            _tcg = new ConcurrentDictionary<string, string>();
            _ocg = new ConcurrentDictionary<string, string>();

            using (var httpClient = new HttpClient { BaseAddress = new Uri("http://yugioh.wikia.com/") })
            {

                Console.WriteLine("Retrieving TCG and OCG card list...");

                responseTcg = await httpClient.GetStringAsync("api/v1/Articles/List?category=TCG_cards&limit=1000000&namespaces=0"); //I have to use a really high number or else some cards won't show up, its so bizarre...
                responseOcg = await httpClient.GetStringAsync("api/v1/Articles/List?category=OCG_cards&limit=1000000&namespaces=0");

                Console.WriteLine("Retrieved TCG and OCG card list.");

            }

            Console.WriteLine("Parsing returned response...");

            var json = JObject.Parse(responseTcg);

            Parallel.ForEach(json["items"].ToObject<JArray>(), item => _tcg[item.Value<string>("title")] = item.Value<string>("url"));

            json = JObject.Parse(responseOcg);

            Parallel.ForEach(json["items"].ToObject<JArray>(), item => _ocg[item.Value<string>("title")] = item.Value<string>("url"));

            Console.WriteLine("Finished parsing returned response.");

            return _tcg.Concat(_ocg).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

    }
}
