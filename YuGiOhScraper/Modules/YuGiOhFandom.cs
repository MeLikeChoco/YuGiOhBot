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
using YuGiOhScraper.Extensions;
using YuGiOhScraper.Parsers.YuGiOhWikia;

namespace YuGiOhScraper.Modules
{
    [Module]
    public class YuGiOhFandom : MediaWikiBase
    {

        public YuGiOhFandom()
            : base("YuGiOh Wikia", "ygofandom", ScraperConstants.YuGiOhWikiaUrl) { }

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

            cards.FirstOrDefault(card => card.Name.Contains("obelisk", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("tormentor", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000000");
            cards.FirstOrDefault(card => card.Name.Contains("slifer", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("sky dragon", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000010");
            cards.FirstOrDefault(card => card.Name.Contains("ra", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("winged dragon", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000020");

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

                if (boosterPackLinks.Any() && !Settings.IsSubProcess)
                {

                    Console.WriteLine($"There were {errors.Count()} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting booster packs.");

            } while (boosterPackLinks.Any() && retry == "y" && !Settings.IsSubProcess);

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

                    var boosterPack = new BoosterPackParser(kv.Key, $"{ScraperConstants.YuGiOhWikiaUrl}{ScraperConstants.MediaWikiParseUrl}{kv.Value}").Parse(HttpClient);

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

                if (!Settings.IsSubProcess)
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

                if (cardLinks.Any() && !Settings.IsSubProcess)
                {

                    Console.WriteLine($"There were {errors.Count()} errors. Retry? (y/n): ");
                    retry = Console.ReadLine();

                }
                else
                    Console.WriteLine("Finished getting cards.");

            } while (cardLinks.Any() && retry == "y" && !Settings.IsSubProcess);

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

                    var card = new CardParser(kv.Key, $"{ScraperConstants.YuGiOhWikiaUrl}{ScraperConstants.MediaWikiParseUrl}{kv.Value}").Parse(HttpClient);
                    card.Url = $"{ScraperConstants.YuGiOhWikiaUrl}?curid={kv.Value}";

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

                if (!Settings.IsSubProcess)
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

        ////there are two ways we could have done this
        ////1. assume the guy using this has bad internet
        ////2. assume the guy using this doesn't care about it
        ////I'll go with 2 to make my life easier
        ////I also most likely don't need parallel foreach, but whatever
        //protected override async Task<IDictionary<string, string>> GetCardLinks()
        //{

        //    string responseTcg, responseOcg;
        //    TcgCards = new ConcurrentDictionary<string, string>();
        //    OcgCards = new ConcurrentDictionary<string, string>();

        //    Console.WriteLine("Retrieving TCG and OCG card list...");

        //    responseTcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaTcgCards); //I have to use a really high number or else some cards won't show up, its so bizarre...
        //    responseOcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaOcgCards);

        //    Console.WriteLine("Retrieved TCG and OCG card list.");
        //    Console.WriteLine("Parsing returned response...");

        //    var tcgJson = JObject.Parse(responseTcg);
        //    var ocgJson = JObject.Parse(responseOcg);

        //    Task.WaitAll(Task.Run(() => Parallel.ForEach(tcgJson["items"].ToObject<JArray>(), item => TcgCards[item.Value<string>("title")] = item.Value<string>("url"))),
        //        Task.Run(() => Parallel.ForEach(ocgJson["items"].ToObject<JArray>(), item => OcgCards[item.Value<string>("title")] = item.Value<string>("url"))));

        //    Console.WriteLine("Finished parsing returned response.");

        //    return TcgCards.Concat(OcgCards).DistinctBy(kv => kv.Key).DoIf(list => Settings.CardAmount != -1, list => list.Take(Settings.CardAmount)).ToDictionary(kv => kv.Key, kv => kv.Value);

        //}

        //protected override async Task<IDictionary<string, string>> GetBoosterPackLinks()
        //{

        //    string responseTcg, responseOcg;
        //    TcgBoosters = new ConcurrentDictionary<string, string>();
        //    OcgBoosters = new ConcurrentDictionary<string, string>();

        //    Console.WriteLine("Retrieving TCG and OCG booster pack list...");

        //    responseTcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaTcgPacks);
        //    responseOcg = await HttpClient.GetStringAsync(ScraperConstants.YuGiOhWikiaOcgPacks);

        //    Console.WriteLine("Retrieved TCG and OCG booster pack list.");
        //    Console.WriteLine("Parsing returned response...");

        //    var tcgJson = JObject.Parse(responseTcg);
        //    var ocgJson = JObject.Parse(responseOcg);

        //    Task.WaitAll(
        //        Task.Run(() => Parallel.ForEach(tcgJson["items"].ToObject<JArray>(), item => TcgBoosters[item.Value<string>("title")] = item.Value<string>("url"))),
        //        Task.Run(() => Parallel.ForEach(ocgJson["items"].ToObject<JArray>(), item => OcgBoosters[item.Value<string>("title")] = item.Value<string>("url")))
        //        );

        //    Console.WriteLine("Finished parsing returned response.");

        //    return TcgBoosters.Concat(OcgBoosters).DistinctBy(kv => kv.Key).DoIf(list => Settings.BoosterPackAmount != -1, list => list.Take(Settings.BoosterPackAmount)).ToDictionary(kv => kv.Key, kv => kv.Value);

        //}

        protected override string GetCmContinue(JObject jObject)
            => jObject["query-continue"]?["categorymembers"].Value<string>("cmcontinue");

        protected override JArray GetCategoryMembers(JObject jObject)
            => jObject["query"].Value<JArray>("categorymembers");

    }
}
