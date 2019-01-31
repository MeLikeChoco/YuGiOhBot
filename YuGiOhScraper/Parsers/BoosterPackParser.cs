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

namespace YuGiOhScraper.Parsers
{
    public class BoosterPackParser
    {

        private string _name;
        private IElement _dom;

        public BoosterPackParser(string name, string link)
        {

            _name = name;
            _dom = ScraperConstants.Context.OpenAsync(link).Result.GetElementById("mw-content-text");

        }

        public BoosterPack Parse()
        {

            var dates = _dom.GetElementsByClassName("portable-infobox").First()
                .GetElementsByTagName("section")
                .FirstOrDefault(element => element.FirstElementChild.TextContent.Contains("release date", StringComparison.OrdinalIgnoreCase))?
                .GetElementsByClassName("pi-data")
                .ToDictionary(element => element.FirstElementChild.TextContent.Trim(), element => element.LastElementChild.TextContent.Trim());
            var table = _dom.GetElementsByClassName("card-list").FirstOrDefault();
            var tableHead = table.FirstElementChild;
            var nameIndex = GetColumnIndex(tableHead, "name");
            var rarityIndex = GetColumnIndex(tableHead, "rarity");
            var cardTable = table.Children.Skip(1);
            var cards = new List<BoosterPackCard>();

            foreach (var cardRow in cardTable)
            {

                var card = new BoosterPackCard() { Name = cardRow.Children[nameIndex].TextContent.Trim() };
                var rarities = cardRow.Children[rarityIndex].Children.Select(node => node.TextContent.Trim());
                card.Rarities = rarities;

            }

            var boosterPack = new BoosterPack()
            {

                Name = _name,
                Dates = dates == null ? null : JsonConvert.SerializeObject(dates),
                Cards = JsonConvert.SerializeObject(cards)

            };

            return boosterPack;

        }

        private int GetColumnIndex(IElement tableHead, string name)
        {

            var column = tableHead.FirstChild.ChildNodes.FirstOrDefault(element => element.TextContent.Equals(name, StringComparison.OrdinalIgnoreCase));

            return tableHead.ChildNodes.Index(column);

        }

    }
}
