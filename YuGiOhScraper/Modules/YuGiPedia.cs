using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MoreLinq;
using Newtonsoft.Json;
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
using YuGiOhScraper.Extensions;
using YuGiOhScraper.Parsers.YuGiPedia;

namespace YuGiOhScraper.Modules
{
    public class YuGiPedia : ModuleBase
    {

        private readonly bool _wasLaunchedByProgram;

        public YuGiPedia(bool arg)
            : base("YugiPedia", "ygopedia.db", ScraperConstants.YuGiPediaBaseUrl)
        {

            //if (!string.IsNullOrEmpty(arg) && int.TryParse(arg, out var result))
            //    _wasLaunchedByProgram = result == 1;

            _wasLaunchedByProgram = arg;

        }

        protected override Task DoBeforeParse(IDictionary<string, string> cardLinks, IDictionary<string, string> boosterPackLinks)
            => Task.CompletedTask;

        protected override Task DoBeforeSaveToDb(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {

            cards.FirstOrDefault(card => card.Name.Contains("obelisk", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("tormentor", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000000");
            cards.FirstOrDefault(card => card.Name.Contains("slifer", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("sky dragon", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000010");
            cards.FirstOrDefault(card => card.Name.Contains("ra", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("winged dragon", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000020");

            return Task.CompletedTask;

        }

        protected override async Task<IDictionary<string, string>> GetBoosterPackLinks()
        {

            string responseTcg, responseOcg;
            TcgBoosters = new ConcurrentDictionary<string, string>();
            OcgBoosters = new ConcurrentDictionary<string, string>();

            Console.WriteLine("Retrieving TCG and OCG booster pack list...");

            responseTcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiPediaTcgPacks);
            responseOcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiPediaOcgPacks);

            Console.WriteLine("Retrieved TCG and OCG booster pack list.");
            Console.WriteLine("Parsing returned response...");

            var tcgJson = JObject.Parse(responseTcg)["query"]["categorymembers"].ToObject<JArray>();
            var ocgJson = JObject.Parse(responseOcg)["query"]["categorymembers"].ToObject<JArray>();

            Task.WaitAll(
                Task.Run(() => Parallel.ForEach(tcgJson, item => TcgBoosters[item.Value<string>("title")] = item.Value<string>("pageid"))),
                Task.Run(() => Parallel.ForEach(ocgJson, item => OcgBoosters[item.Value<string>("title")] = item.Value<string>("pageid")))
                );

            Console.WriteLine("Finished parsing returned response.");

            return TcgBoosters.Concat(OcgBoosters).Take(0).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => $"{ScraperConstants.YuGiPediaUrl}{Uri.EscapeDataString(kv.Key)}");

        }

        protected override async Task<IDictionary<string, string>> GetCardLinks()
        {

            Console.WriteLine("Retrieving TCG and OCG card list...");

            TcgCards = await AggregateCards(ScraperConstants.YuGiPediaTcgCards);
            OcgCards = await AggregateCards(ScraperConstants.YuGiPediaOcgCards);

            Console.WriteLine("Finished retrieving TCG and OCG cards.");

            return TcgCards.Concat(OcgCards).DistinctBy(kv => kv.Key).Take(100).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

        private async Task<IDictionary<string, string>> AggregateCards(string baseUrl)
        {

            var counter = 1;
            var cmcontinue = "";
            var cards = new Dictionary<string, string>();
            string response, url;
            JObject json;

            do
            {

                url = baseUrl + $"&cmcontinue={Uri.EscapeDataString(cmcontinue)}";
                response = await HttpClient.GetStringAsync(url);
                json = JObject.Parse(response);
                cmcontinue = json["continue"]?["cmcontinue"]?.ToObject<string>();

                foreach (var card in json["query"]["categorymembers"].ToObject<JArray>())
                    cards[card.Value<string>("title")] = card.Value<string>("pageid");

                InlineWrite($"Page: {counter++}");

            } while (!string.IsNullOrEmpty(cmcontinue));

            return cards;

        }

        #region Booster Packs
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
        #endregion Booster Packs

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

            Parallel.ForEach(links, kv =>
            {

                try
                {

                    var card = new CardParser(kv.Key, ScraperConstants.YuGiPediaBaseUrl + $"?curid={kv.Value}").Parse();

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

                        createCardTable.CommandText = ScraperConstants.CreateCardTableSql;
                        createboosterPackTable.CommandText = ScraperConstants.CreateBoosterPackTableSql;
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

                    Console.WriteLine($"IOException occured. Most likely cause is {DatabaseName} being held open by another program. Hit enter when resolved...");

                    while (Console.ReadKey().Key != ConsoleKey.Enter) { }

                }

            } while (exception != null);

        }

    }
}
