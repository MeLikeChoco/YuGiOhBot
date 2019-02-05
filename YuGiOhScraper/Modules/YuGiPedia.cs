using MoreLinq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;

namespace YuGiOhScraper.Modules
{
    public class YuGiPedia : ModuleBase
    {

        private readonly bool _wasLaunchedByProgram;

        public YuGiPedia(bool arg)
            : base("YugiPedia", "ygopedia.db")
        {

            //if (!string.IsNullOrEmpty(arg) && int.TryParse(arg, out var result))
            //    _wasLaunchedByProgram = result == 1;

            _wasLaunchedByProgram = arg;

        }

        protected override Task DoBeforeParse(IDictionary<string, string> cardLinks, IDictionary<string, string> boosterPackLinks)
        {
            throw new NotImplementedException();
        }

        protected override Task DoBeforeSaveToDb(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {
            throw new NotImplementedException();
        }

        protected override async Task<IDictionary<string, string>> GetBoosterPackLinks()
        {

            string responseTcg, responseOcg;
            TcgBoosters = new ConcurrentDictionary<string, string>();
            OcgBoosters = new ConcurrentDictionary<string, string>();

            using (var httpClient = new HttpClient() { BaseAddress = new Uri(ScraperConstants.YuGiPediaUrl) })
            {

                Console.WriteLine("Retrieving TCG and OCG booster pack list...");

                responseTcg = await httpClient.GetStringAsync(ScraperConstants.YuGiPediaTcgPacks);
                responseOcg = await httpClient.GetStringAsync(ScraperConstants.YuGiPediaOcgPacks);

                Console.WriteLine("Retrieved TCG and OCG booster pack list.");

            }

            Console.WriteLine("Parsing returned response...");

            var tcgJson = JObject.Parse(responseTcg)["query"]["categorymembers"].ToObject<JArray>();
            var ocgJson = JObject.Parse(responseOcg)["query"]["categorymembers"].ToObject<JArray>();

            Task.WaitAll(
                Task.Run(() => Parallel.ForEach(tcgJson, item => TcgBoosters[item.Value<string>("title")] = item.Value<string>("pageid"))),
                Task.Run(() => Parallel.ForEach(ocgJson, item => OcgBoosters[item.Value<string>("title")] = item.Value<string>("pageid")))
                );

            Console.WriteLine("Finished parsing returned response.");

            return TcgBoosters.Concat(OcgBoosters).DistinctBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

        protected override Task<IDictionary<string, string>> GetCardLinks()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<BoosterPack> ParseBoosterPacks(IDictionary<string, string> boosterPackLinks, out IEnumerable<Error> errors)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Card> ParseCards(IDictionary<string, string> cardLinks, out IEnumerable<Error> errors)
        {
            throw new NotImplementedException();
        }

        protected override Task SaveToDatabase(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {
            throw new NotImplementedException();
        }

    }
}
