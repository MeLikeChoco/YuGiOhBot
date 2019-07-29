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
    [Module]
    public class YuGiPedia : MediaWikiBase
    {

        public YuGiPedia()
            : base("YugiPedia", "ygopedia", ScraperConstants.YuGiPediaUrl) { }

        protected override Task DoBeforeParse(IDictionary<string, string> cardLinks, IDictionary<string, string> boosterPackLinks)
            => Task.CompletedTask;

        protected override Task DoBeforeSaveToDb(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {

            cards.FirstOrDefault(card => card.Name.Contains("obelisk", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("tormentor", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000000");
            cards.FirstOrDefault(card => card.Name.Contains("slifer", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("sky dragon", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000010");
            cards.FirstOrDefault(card => card.Name.Contains("ra", StringComparison.OrdinalIgnoreCase) && card.Name.Contains("winged dragon", StringComparison.OrdinalIgnoreCase)).DoIf(card => card != null, card => card.Passcode = "10000020");

            return Task.CompletedTask;

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

                if (!Settings.IsSubProcess)
                    InlineWrite($"Page: {counter++}");

            } while (!string.IsNullOrEmpty(cmcontinue));

            return cards;

        }

        //protected override Task<IDictionary<string, string>> GetCardLinks()
        //    => AggregateInfo(ScraperConstants.MediaWikiAllCards);

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

                    var boosterPack = new BoosterPackParser(kv.Key, $"{ScraperConstants.YuGiPediaUrl}{ScraperConstants.MediaWikiParseUrl}{kv.Value}").Parse(HttpClient);

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

                    var card = new CardParser(kv.Key, $"{ScraperConstants.YuGiPediaUrl}{ScraperConstants.MediaWikiParseUrl}{kv.Value}").Parse(HttpClient);
                    card.Url = $"{ScraperConstants.YuGiPediaUrl}?curid={kv.Value}";

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

                    }.DoIf(error => exception.InnerException != null, error =>
                    {

                        error.InnerException = $"{exception.InnerException.Message}\n{exception.InnerException.StackTrace}";

                        return error;

                    }));

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

        protected override string GetCmContinue(JObject jObject)
            => jObject["continue"]?.Value<string>("cmcontinue");

        protected override JArray GetCategoryMembers(JObject jObject)
            => jObject["query"].Value<JArray>("categorymembers");

    }
}
