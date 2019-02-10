using MoreLinq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Extensions;

namespace YuGiOhScraper.Modules
{
    public abstract class MediaWikiBase : ModuleBase
    {

        protected MediaWikiBase(string name, string filename, string url)
            : base(name, filename, url) { }

        protected override async Task<IDictionary<string, string>> GetCardLinks()
        {

            Console.WriteLine("Retrieving TCG and OCG card list...");

            TcgCards = await AggregateInfo(ScraperConstants.MediaWikiTcgCards);
            OcgCards = await AggregateInfo(ScraperConstants.MediaWikiOcgCards);

            Console.WriteLine("Finished retrieving TCG and OCG cards.");

            return TcgCards.Concat(OcgCards).DistinctBy(kv => kv.Key).DoIf(list => Settings.CardAmount != -1, list => list.Take(Settings.CardAmount)).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

        protected override async Task<IDictionary<string, string>> GetBoosterPackLinks()
        {
            
            Console.WriteLine("Retrieving TCG and OCG booster pack list...");

            TcgBoosters = await AggregateInfo(ScraperConstants.MediaWikiTcgPacks);
            OcgBoosters = await AggregateInfo(ScraperConstants.MediaWikiOcgPacks);

            Console.WriteLine("Retrieved TCG and OCG booster pack list.");
            Console.WriteLine("Parsing returned response...");

            Console.WriteLine("Finished parsing returned response.");

            return TcgBoosters.Concat(OcgBoosters).DistinctBy(kv => kv.Key).DoIf(list => Settings.BoosterPackAmount != -1, list => list.Take(Settings.BoosterPackAmount)).ToDictionary(kv => kv.Key, kv => kv.Value);

        }

        protected async Task<IDictionary<string, string>> AggregateInfo(string baseUrl)
        {

            var counter = 1;
            var cmcontinue = "";
            var entities = new Dictionary<string, string>();
            string response, url;
            JObject json;

            do
            {

                url = baseUrl + $"&cmcontinue={Uri.EscapeDataString(cmcontinue)}";
                response = await HttpClient.GetStringAsync(url);
                json = JObject.Parse(response);
                cmcontinue = GetCmContinue(json);

                foreach (var entity in GetCategoryMembers(json).ToObject<JArray>())
                    entities[entity.Value<string>("title")] = entity.Value<string>("pageid");

                InlineWrite($"Page: {counter++}");

            } while (!string.IsNullOrEmpty(cmcontinue));

            return entities;

        }

        protected abstract string GetCmContinue(JObject jObject);
        protected abstract JArray GetCategoryMembers(JObject jObject);

    }
}
