using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;
using YuGiOhScraper.Extensions;

namespace YuGiOhScraper.Parsers.YuGiOhWikia
{
    public class BoosterPackParser : MediaWikiParser<BoosterPack>
    {

        public BoosterPackParser(string name, string url)
            : base(name, url) { }

        public override BoosterPack Parse(HttpClient httpClient)
        {

            var dom = GetDom(httpClient);
            var dates = dom.GetElementByClassName("portable-infobox")?
                .GetElementsByTagName("section")
                .FirstOrDefault(element => element.FirstElementChild.TextContent.Contains("release date", StringComparison.OrdinalIgnoreCase))?
                .GetElementsByClassName("pi-data")
                .ToDictionary(element => element.FirstElementChild.TextContent.Trim(), element => element.LastElementChild.TextContent.Trim());
            var table = dom.GetElementByClassName("card-list") ?? dom.GetElementByClassName("wikitable");
            var tableHead = table.ClassList.Contains("card-list") ? table.GetElementsByTagName("tbody").FirstOrDefault().FirstElementChild : table.GetElementsByTagName("tbody").FirstOrDefault().Children[1];
            var nameIndex = GetColumnIndex(tableHead, "name");
            var rarityIndex = GetColumnIndex(tableHead, "rarity");
            var cardTable = table.ClassList.Contains("card-list") ? table.GetElementsByTagName("tbody").FirstOrDefault().Children.Skip(1) : table.GetElementsByTagName("tbody").FirstOrDefault().Children.Skip(2);
            var cards = new List<BoosterPackCard>();

            foreach (var cardRow in cardTable)
            {

                var name = TrimName(cardRow.Children[nameIndex].TextContent);
                var rarities = rarityIndex == -1 ? null : cardRow.Children[rarityIndex].Children.Select(element => element.TextContent.Trim()).Where(IsNotNullOrEmpty);
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

        private int GetColumnIndex(IElement tableHead, string name)
        {

            var column = tableHead.Children.FirstOrDefault(element => element.TextContent.Contains(name, StringComparison.OrdinalIgnoreCase));

            return tableHead.Children.Index(column);

        }

        private bool IsNotNullOrEmpty(string input)
            => !string.IsNullOrEmpty(input);

    }
}
