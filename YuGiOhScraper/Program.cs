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

        private static IDictionary<string, string> _tcgCards, _ocgCards, _tcgBoosters, _ocgBoosters;
        private static bool _wasLaunchedByProgram = false;

        public static async Task Main(string[] args)
        {

            if (args.Any() && int.TryParse(args.FirstOrDefault(), out var result))
                _wasLaunchedByProgram = result == 1;
            //var links = await GetCardLinks(httpClient);
            IDictionary<string, string> cardLinks = (await GetCardLinks()).Take(0).ToDictionary(kv => kv.Key, kv => kv.Value);
            IDictionary<string, string> boosterPackLinks = (await GetBoosterLinks()).ToDictionary(kv => kv.Key, kv => kv.Value);

            Console.WriteLine($"There are {cardLinks.Count} cards to parse.");

            var cards = ParseCards(cardLinks, out var errors);
            var boosterPacks = ParsePacks(boosterPackLinks, out var errors2);
            errors = errors.Concat(errors2);

            await CollectionToSqlite(cards, boosterPacks, errors);

            if (!_wasLaunchedByProgram)
                Console.ReadKey();

        }

        private static void PerformAdditionalAction(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks)
        {

            PasscodeForGodCards(cards);

        }

        private static void PasscodeForGodCards(IEnumerable<Card> cards)
        {

            cards.FirstOrDefault(card => card.Name.Contains("obelisk", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("tormentor", StringComparison.OrdinalIgnoreCase)).Passcode = "10000000";
            cards.FirstOrDefault(card => card.Name.Contains("slifer", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("sky dragon", StringComparison.OrdinalIgnoreCase)).Passcode = "10000010";
            cards.FirstOrDefault(card => card.Name.Contains("ra", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("winged dragon", StringComparison.OrdinalIgnoreCase)).Passcode = "10000020";

        }

        private static IEnumerable<BoosterPack> ParsePacks(IDictionary<string, string> links, out IEnumerable<CardError> errors)
        {

            string retry = "";
            IEnumerable<BoosterPack> boosterPacks = new List<BoosterPack>();

            do
            {

                Console.WriteLine("Getting cards...");
                boosterPacks = boosterPacks.Concat(GetBoosterPacks(links, out errors));
                links = errors.ToDictionary(error => error.Name, error => error.Url);
                //var cards = GetCards(httpClient, links.ToList().GetRange(0, 100).ToDictionary(kv => kv.Key, kv => kv.Value));

                if (links.Any() && !_wasLaunchedByProgram)
                {

                    Console.WriteLine($"There were {errors.Count()} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting cards.");

            } while (links.Any() && retry == "y" && !_wasLaunchedByProgram);

            return boosterPacks;

        }

        private static IEnumerable<Card> ParseCards(IDictionary<string, string> links, out IEnumerable<CardError> errors)
        {

            string retry = "";
            IEnumerable<Card> cards = new List<Card>();

            do
            {

                Console.WriteLine("Getting cards...");
                cards = cards.Concat(GetCards(links, out errors));
                links = errors.ToDictionary(error => error.Name, error => error.Url);
                //var cards = GetCards(httpClient, links.ToList().GetRange(0, 100).ToDictionary(kv => kv.Key, kv => kv.Value));

                if (links.Any() && !_wasLaunchedByProgram)
                {

                    Console.WriteLine($"There were {errors.Count()} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting cards.");

            } while (links.Any() && retry == "y" && !_wasLaunchedByProgram);

            return cards;

        }

        //private static Task CardsToSqlite()
        //    => CardsToSqlite(null);

        private static async Task CollectionToSqlite(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<CardError> errors)
        {

            if (File.Exists(ScraperConstants.DbPath))
                File.Delete(ScraperConstants.DbPath);

            using (var db = new SqliteConnection(ScraperConstants.ConnectionString))
            {

                await db.OpenAsync();

                SqliteCommand createCardTable, createCardErrorTable;
                createCardTable = createCardErrorTable = null;

                createCardTable = db.CreateCommand();
                createCardTable.CommandText = ScraperConstants.CreateCardTableSql;

                if (_wasLaunchedByProgram)
                {

                    createCardErrorTable = db.CreateCommand();
                    createCardErrorTable.CommandText = ScraperConstants.CreateCardErrorTableSql;

                }

                Console.WriteLine("Saving to ygo.db...");
                await createCardTable.ExecuteNonQueryAsync();

                if (_wasLaunchedByProgram)
                    await createCardErrorTable.ExecuteNonQueryAsync();

                await db.InsertAsync(cards);
                await db.InsertAsync(boosterPacks);
                await db.InsertAsync(errors);

                Console.WriteLine("Finished saving to ygo.db.");

                db.Close();

            }

        }

        private static IEnumerable<BoosterPack> GetBoosterPacks(IDictionary<string, string> links, out IEnumerable<CardError> errors)
        {

            var boosterPacks = new ConcurrentBag<BoosterPack>();
            var total = links.Count;
            var current = 0;
            var pOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            var tempErrors = new ConcurrentBag<CardError>();

            Parallel.ForEach(links, pOptions, kv =>
            {

                try
                {

                    var boosterPack = new BoosterPackParser(kv.Key, $"{ScraperConstants.Wikia.TrimEnd('/')}{kv.Value}").Parse();

                    #region OCG TCG
                    #endregion OCG TCG

                    boosterPacks.Add(boosterPack);

                }
                catch (Exception exception)
                {

                    tempErrors.Add(new CardError()
                    {

                        Name = kv.Key,
                        Exception = $"{exception.Message}\n{exception.StackTrace}",
                        Url = kv.Value

                    });

                }

                var counter = Interlocked.Increment(ref current);

                if (!_wasLaunchedByProgram)
                    InlineWrite($"Progress: {counter}/{total} ({(counter / (double)total) * 100}%)");

            });

            errors = tempErrors;

            return boosterPacks;

        }

        private static IEnumerable<Card> GetCards(IDictionary<string, string> links, out IEnumerable<CardError> errors)
        {

            var cards = new ConcurrentBag<Card>();
            var total = links.Count;
            var current = 0;
            var pOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            var tempErrors = new ConcurrentBag<CardError>();

            Parallel.ForEach(links, pOptions, kv =>
            {

                try
                {

                    var card = new CardParser(kv.Key, $"{ScraperConstants.Wikia.TrimEnd('/')}{kv.Value}").Parse();

                    #region OCG TCG
                    //c# int default is 0, therefore, if only one of them is 1, that means it is an format exclusive card
                    //if both of them are 1, then it is in both formats
                    if (_tcgCards.ContainsKey(card.Name))
                        card.TcgExists = true;

                    if (_ocgCards.ContainsKey(card.Name))
                        card.OcgExists = true;
                    #endregion OCG TCG

                    cards.Add(card);

                }
                catch (Exception exception)
                {

                    tempErrors.Add(new CardError()
                    {

                        Name = kv.Key,
                        Exception = $"{exception.Message}\n{exception.StackTrace}",
                        Url = kv.Value

                    });

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
            _tcgCards = new ConcurrentDictionary<string, string>();
            _ocgCards = new ConcurrentDictionary<string, string>();

            using (var httpClient = new HttpClient { BaseAddress = new Uri(ScraperConstants.Wikia) })
            {

                Console.WriteLine("Retrieving TCG and OCG card list...");

                responseTcg = await httpClient.GetStringAsync(ScraperConstants.TcgCards); //I have to use a really high number or else some cards won't show up, its so bizarre...
                responseOcg = await httpClient.GetStringAsync(ScraperConstants.OcgCards);

                Console.WriteLine("Retrieved TCG and OCG card list.");

            }

            Console.WriteLine("Parsing returned response...");

            var tcgJson = JObject.Parse(responseTcg);
            var ocgJson = JObject.Parse(responseOcg);

            Task.WaitAll(Task.Run(() => Parallel.ForEach(tcgJson["items"].ToObject<JArray>(), item => _tcgCards[item.Value<string>("title")] = item.Value<string>("url"))),
                Task.Run(() => Parallel.ForEach(ocgJson["items"].ToObject<JArray>(), item => _ocgCards[item.Value<string>("title")] = item.Value<string>("url"))));

            Console.WriteLine("Finished parsing returned response.");

            return _tcgCards.Concat(_ocgCards).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

        private static async Task<IDictionary<string, string>> GetBoosterLinks()
        {

            string responseTcg, responseOcg;
            _tcgBoosters = new ConcurrentDictionary<string, string>();
            _ocgBoosters = new ConcurrentDictionary<string, string>();

            using (var httpClient = new HttpClient() { BaseAddress = new Uri(ScraperConstants.Wikia) })
            {

                Console.WriteLine("Retrieving TCG and OCG booster pack list...");

                responseTcg = await httpClient.GetStringAsync(ScraperConstants.TcgPacks);
                responseOcg = await httpClient.GetStringAsync(ScraperConstants.OcgPacks);

                Console.WriteLine("Retrieved TCG and OCG booster pack list.");

            }

            Console.WriteLine("Parsing returned response...");

            var tcgJson = JObject.Parse(responseTcg);
            var ocgJson = JObject.Parse(responseOcg);

            Task.WaitAll(Task.Run(() => Parallel.ForEach(tcgJson["items"].ToObject<JArray>(), item => _tcgBoosters[item.Value<string>("title")] = item.Value<string>("url"))),
                Task.Run(() => Parallel.ForEach(ocgJson["items"].ToObject<JArray>(), item => _ocgBoosters[item.Value<string>("title")] = item.Value<string>("url"))));

            Console.WriteLine("Finished parsing returned response.");

            return _tcgBoosters.Concat(_ocgBoosters).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

    }
}
