using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;

namespace YuGiOhScraper.Parsers.YuGiOhWikia
{
    public class BoosterPackParser
    {

        private readonly string _name;
        private IElement _dom;

        public BoosterPackParser(string name, string link)
        {

            _name = name;
            _dom = ScraperConstants.Context.OpenAsync(link).Result.GetElementById("mw-content-text");

        }

        public BoosterPack Parse()
        {

            var dates = _dom.GetElementsByClassName("portable-infobox").FirstOrDefault()?
                .GetElementsByTagName("section")
                .FirstOrDefault(element => element.FirstElementChild.TextContent.Contains("release date", StringComparison.OrdinalIgnoreCase))?
                .GetElementsByClassName("pi-data")
                .ToDictionary(element => element.FirstElementChild.TextContent.Trim(), element => element.LastElementChild.TextContent.Trim());
            var table = _dom.GetElementsByClassName("card-list").FirstOrDefault() ?? _dom.GetElementsByClassName("wikitable").FirstOrDefault();
            var tableHead = table.ClassList.Contains("card-list") ? table.GetElementsByTagName("tbody").FirstOrDefault().FirstElementChild : table.GetElementsByTagName("tbody").FirstOrDefault().Children[1];
            var nameIndex = GetColumnIndex(tableHead, "name");
            var rarityIndex = GetColumnIndex(tableHead, "rarity");
            var cardTable = table.ClassList.Contains("card-list") ? table.GetElementsByTagName("tbody").FirstOrDefault().Children.Skip(1) : table.GetElementsByTagName("tbody").FirstOrDefault().Children.Skip(2);
            var cards = new List<BoosterPackCard>();

            foreach (var cardRow in cardTable)
            {

                var name = cardRow.Children[nameIndex].TextContent.Trim().Trim('"');
                var rarities = rarityIndex == -1 ? null : cardRow.Children[rarityIndex].Children.Select(element => element.TextContent.Trim()).Where(IsNotNullOrEmpty);
                var card = new BoosterPackCard { Name = name, Rarities = rarities };

                cards.Add(card);

            }

            var boosterPack = new BoosterPack()
            {

                Name = _name,
                Dates = dates == null ? null : JsonConvert.SerializeObject(dates),
                Cards = JsonConvert.SerializeObject(cards),
                Url = _dom.BaseUri

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
