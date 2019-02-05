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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;
using YuGiOhScraper.Parsers.YuGiOhWikia;

namespace YuGiOhScraper.Modules
{
    public class YuGiOhWikia : ModuleBase
    {

        private readonly bool _wasLaunchedByProgram;

        public YuGiOhWikia(bool arg)
            : base("YuGiOh Wikia", "ygo.db", ScraperConstants.YuGiOhWikiaUrl)
        {

            //if (!string.IsNullOrEmpty(arg) && int.TryParse(arg, out var result))
            //    _wasLaunchedByProgram = result == 1;

            _wasLaunchedByProgram = arg;

        }

        //public async Task RunAsync()
        //{

        //    Console.WriteLine("YuGiOh Wikia module has began running...");

        //    IDictionary<string, string> cardLinks = (await GetCardLinks()).ToDictionary(kv => kv.Key, kv => kv.Value);
        //    IDictionary<string, string> boosterPackLinks = (await GetBoosterLinks()).ToDictionary(kv => kv.Key, kv => kv.Value);

        //    Console.WriteLine($"There are {cardLinks.Count} cards to parse.");

        //    var cards = ParseCards(cardLinks, out var errors);
        //    var boosterPacks = ParsePacks(boosterPackLinks, out var errors2);
        //    errors = errors.Concat(errors2);

        //    await CollectionToSqlite(cards, boosterPacks, errors);

        //    Console.WriteLine("YuGiOh Wikia module has finished running.");

        //}

        protected override Task DoBeforeSaveToDb(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {

            PasscodeForGodCards(cards);

            return Task.CompletedTask;

        }

        protected override Task DoBeforeParse(IDictionary<string, string> cardLinks, IDictionary<string, string> boosterPackLinks)
            => Task.CompletedTask;

        private void PasscodeForGodCards(IEnumerable<Card> cards)
        {

            cards.FirstOrDefault(card => card.Name.Contains("obelisk", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("tormentor", StringComparison.OrdinalIgnoreCase)).Passcode = "10000000";
            cards.FirstOrDefault(card => card.Name.Contains("slifer", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("sky dragon", StringComparison.OrdinalIgnoreCase)).Passcode = "10000010";
            cards.FirstOrDefault(card => card.Name.Contains("ra", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("winged dragon", StringComparison.OrdinalIgnoreCase)).Passcode = "10000020";

        }

        #region BoosterPacks
        protected override IEnumerable<BoosterPack> ParseBoosterPacks(IDictionary<string, string> boosterPackLinks, out IEnumerable<Error> errors)
        {

            string retry = "";
            IEnumerable<BoosterPack> boosterPacks = new List<BoosterPack>();

            do
            {

                Console.WriteLine("Getting booster packs...");
                boosterPacks = boosterPacks.Concat(GetBoosterPacks(boosterPackLinks, out errors));
                boosterPackLinks = errors.ToDictionary(error => error.Name, error => error.Url);
                //var cards = GetCards(httpClient, links.ToList().GetRange(0, 100).ToDictionary(kv => kv.Key, kv => kv.Value));

                if (boosterPackLinks.Any() && !_wasLaunchedByProgram)
                {

                    Console.WriteLine($"There were {errors.Count()} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting booster packs.");

            } while (boosterPackLinks.Any() && retry == "y" && !_wasLaunchedByProgram);

            return boosterPacks;

        }

        private IEnumerable<BoosterPack> GetBoosterPacks(IDictionary<string, string> links, out IEnumerable<Error> errors)
        {

            var boosterPacks = new ConcurrentBag<BoosterPack>();
            var total = links.Count;
            var current = 0;
            var tempErrors = new ConcurrentBag<Error>();

            Parallel.ForEach(links, ScraperConstants.ParallelOptions, kv =>
            {

                try
                {

                    var boosterPack = new BoosterPackParser(kv.Key, $"{ScraperConstants.YuGiOhWikiaUrl.TrimEnd('/')}{kv.Value}").Parse();

                    #region OCG TCG
                    boosterPack.TcgExists = TcgBoosters.ContainsKey(boosterPack.Name);
                    boosterPack.OcgExists = OcgBoosters.ContainsKey(boosterPack.Name);
                    #endregion OCG TCG

                    boosterPacks.Add(boosterPack);

                }
                catch (Exception exception)
                {

                    tempErrors.Add(new Error()
                    {

                        Name = kv.Key,
                        Exception = $"{exception.Message}\t{exception.StackTrace}",
                        InnerException = exception.InnerException == null ? null : $"{exception.InnerException}\t{exception.InnerException.StackTrace}",
                        Url = kv.Value,
                        Type = "Booster Pack"

                    });

                }

                var counter = Interlocked.Increment(ref current);

                if (!_wasLaunchedByProgram)
                    InlineWrite($"Progress: {counter}/{total} ({(counter / (double)total) * 100}%)");

            });

            errors = tempErrors;

            return boosterPacks;

        }
        #endregion BoosterPacks

        #region Cards
        protected override IEnumerable<Card> ParseCards(IDictionary<string, string> cardLinks, out IEnumerable<Error> errors)
        {

            string retry = "";
            IEnumerable<Card> cards = new List<Card>();

            do
            {

                Console.WriteLine("Getting cards...");
                cards = cards.Concat(GetCards(cardLinks, out errors));
                cardLinks = errors.ToDictionary(error => error.Name, error => error.Url);
                //var cards = GetCards(httpClient, links.ToList().GetRange(0, 100).ToDictionary(kv => kv.Key, kv => kv.Value));

                if (cardLinks.Any() && !_wasLaunchedByProgram)
                {

                    Console.WriteLine($"There were {errors.Count()} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting cards.");

            } while (cardLinks.Any() && retry == "y" && !_wasLaunchedByProgram);

            return cards;

        }

        private IEnumerable<Card> GetCards(IDictionary<string, string> links, out IEnumerable<Error> errors)
        {

            var cards = new ConcurrentBag<Card>();
            var total = links.Count;
            var current = 0;
            var tempErrors = new ConcurrentBag<Error>();

            Parallel.ForEach(links, ScraperConstants.ParallelOptions, kv =>
            {

                try
                {

                    var card = new CardParser(kv.Key, $"{ScraperConstants.YuGiOhWikiaUrl.TrimEnd('/')}{kv.Value}").Parse();

                    #region OCG TCG
                    card.TcgExists = TcgCards.ContainsKey(card.Name);
                    card.OcgExists = OcgCards.ContainsKey(card.Name);
                    #endregion OCG TCG

                    cards.Add(card);

                }
                catch (Exception exception)
                {

                    tempErrors.Add(new Error()
                    {

                        Name = kv.Key,
                        Exception = $"{exception.Message}\n{exception.StackTrace}",
                        Url = kv.Value,
                        Type = "Card"

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
        #endregion Cards

        //private  Task CardsToSqlite()
        //    => CardsToSqlite(null);

        protected override async Task SaveToDatabase(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {

            Exception exception = null;

            do
            {

                try
                {

                    if (File.Exists(DatabasePath))
                        File.Delete(DatabasePath);

                    using (var db = new SqliteConnection(ConnectionString))
                    {

                        await db.OpenAsync();

                        SqliteCommand createCardTable, createboosterPackTable, createErrorTable;
                        createCardTable = createboosterPackTable = createErrorTable = null;

                        createCardTable = db.CreateCommand();
                        createboosterPackTable = db.CreateCommand();
                        createErrorTable = db.CreateCommand();
                        createErrorTable = db.CreateCommand();
                        createCardTable.CommandText = ScraperConstants.CreateCardTableSql;
                        createboosterPackTable.CommandText = ScraperConstants.CreateBoosterPackTableSql;
                        createErrorTable.CommandText = ScraperConstants.CreateErrorTable;
                        createErrorTable.CommandText = ScraperConstants.CreateErrorTable;

                        Console.WriteLine($"Saving to {DatabaseName}...");
                        await createCardTable.ExecuteNonQueryAsync();
                        await createboosterPackTable.ExecuteNonQueryAsync();
                        await createErrorTable.ExecuteNonQueryAsync();

                        await db.InsertAsync(cards);
                        await db.InsertAsync(boosterPacks);
                        await db.InsertAsync(errors);

                        Console.WriteLine($"Finished saving to {DatabaseName}.");

                        db.Close();

                    }

                    exception = null;

                }
                catch (IOException ioexception)
                {

                    exception = ioexception;

                    Console.WriteLine("IOException occured. Most likely cause is ygo.db held open by another program. Hit enter when resolved...");

                    while (Console.ReadKey().Key != ConsoleKey.Enter) { }

                }

            } while (exception != null);

        }

        //there are two ways we could have done this
        //1. assume the guy using this has bad internet
        //2. assume the guy using this doesn't care about it
        //I'll go with 2 to make my life easier
        //I also most likely don't need parallel foreach, but whatever
        protected override async Task<IDictionary<string, string>> GetCardLinks()
        {

            string responseTcg, responseOcg;
            TcgCards = new ConcurrentDictionary<string, string>();
            OcgCards = new ConcurrentDictionary<string, string>();

            Console.WriteLine("Retrieving TCG and OCG card list...");
            
            responseTcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaTcgCards); //I have to use a really high number or else some cards won't show up, its so bizarre...
            responseOcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaOcgCards);

            Console.WriteLine("Retrieved TCG and OCG card list.");
            Console.WriteLine("Parsing returned response...");

            var tcgJson = JObject.Parse(responseTcg);
            var ocgJson = JObject.Parse(responseOcg);

            Task.WaitAll(Task.Run(() => Parallel.ForEach(tcgJson["items"].ToObject<JArray>(), item => TcgCards[item.Value<string>("title")] = item.Value<string>("url"))),
                Task.Run(() => Parallel.ForEach(ocgJson["items"].ToObject<JArray>(), item => OcgCards[item.Value<string>("title")] = item.Value<string>("url"))));

            Console.WriteLine("Finished parsing returned response.");

            return TcgCards.Concat(OcgCards).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

        protected override async Task<IDictionary<string, string>> GetBoosterPackLinks()
        {

            string responseTcg, responseOcg;
            TcgBoosters = new ConcurrentDictionary<string, string>();
            OcgBoosters = new ConcurrentDictionary<string, string>();

            Console.WriteLine("Retrieving TCG and OCG booster pack list...");
            
            responseTcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaTcgPacks);
            responseOcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaOcgPacks);

            Console.WriteLine("Retrieved TCG and OCG booster pack list.");
            Console.WriteLine("Parsing returned response...");

            var tcgJson = JObject.Parse(responseTcg);
            var ocgJson = JObject.Parse(responseOcg);

            Task.WaitAll(
                Task.Run(() => Parallel.ForEach(tcgJson["items"].ToObject<JArray>(), item => TcgBoosters[item.Value<string>("title")] = item.Value<string>("url"))),
                Task.Run(() => Parallel.ForEach(ocgJson["items"].ToObject<JArray>(), item => OcgBoosters[item.Value<string>("title")] = item.Value<string>("url")))
                );

            Console.WriteLine("Finished parsing returned response.");

            return TcgBoosters.Concat(OcgBoosters).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

    }
}
