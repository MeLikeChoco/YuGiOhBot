using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;

namespace YuGiOhScraper.Parsers.YuGiPedia
{
    public class BoosterPackParser : MediaWikiParser<BoosterPack>
    {

        public BoosterPackParser(string name, string url)
            : base(name, url) { }

        public override BoosterPack Parse(HttpClient httpClient)
        {

            var html = GetHtml(httpClient);
            var response = ScraperConstants.HtmlParser.ParseDocument(html);
            var dom = response.GetElementsByClassName("mw-parser-output").First();
            var dates = GetReleaseDates(dom);
            var table = dom.GetElementsByClassName("card-list").FirstOrDefault().FirstElementChild.Children;

            if (table == null)
                throw new NullReferenceException($"No card list exists for {Name}");

            var tableHead = table.First();
            var nameIndex = GetColumnIndex(tableHead, "name");
            var rarityIndex = GetColumnIndex(tableHead, "rarity");
            var cardTable = table.Skip(1);
            var cards = new List<BoosterPackCard>();

            foreach (var row in cardTable)
            {

                var name = TrimName(row.Children[nameIndex].TextContent.Trim().Trim('"'));
                var rarities = rarityIndex == -1 ? null : row.Children[rarityIndex].Children.Select(element => element.TextContent.Trim()).Where(IsNotNullOrEmpty);
                var card = new BoosterPackCard { Name = name, Rarities = rarities };

                cards.Add(card);

            }

            var boosterPack = new BoosterPack()
            {

                Name = Name,
                Dates = dates == null ? null : JsonConvert.SerializeObject(dates),
                Cards = JsonConvert.SerializeObject(cards),
                Url = Url

            };

            return boosterPack;

        }

        private IDictionary<string, string> GetReleaseDates(IElement dom)
        {

            var dates = new Dictionary<string, string>();
            var infobox = dom.GetElementsByClassName("infobox").FirstOrDefault().FirstElementChild.Children;
            var releaseDateHeader = infobox.FirstOrDefault(element => !string.IsNullOrEmpty(element.TextContent) && element.TextContent.Contains("release dates", StringComparison.InvariantCultureIgnoreCase));

            if (releaseDateHeader != null)
            {

                var startIndex = infobox.Index(releaseDateHeader) + 1;

                for (int i = startIndex; i < infobox.Length; i++)
                {

                    var dateInfo = infobox[i];

                    if (dateInfo.Children.Length == 2)
                    {

                        var region = dateInfo.FirstElementChild.TextContent.Trim();
                        var date = dateInfo.Children[1].TextContent.Trim();

                        dates[region] = date;

                    }

                }

            }

            return dates;

        }

        private int GetColumnIndex(IElement tableHead, string name)
        {

            var column = tableHead.Children.FirstOrDefault(element => element.TextContent.Contains(name, StringComparison.OrdinalIgnoreCase));

            return tableHead.Children.Index(column);

        }

        private bool IsNotNullOrEmpty(string input)
            => !string.IsNullOrEmpty(input);

    }
}
