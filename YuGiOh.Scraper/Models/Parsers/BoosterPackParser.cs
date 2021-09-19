using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;

namespace YuGiOh.Scraper.Models.Parsers
{
    public class BoosterPackParser
    {
        private readonly string _name, _id;

        public BoosterPackParser(string name, string id)
        {
            _name = name;
            _id = id;
        }

        public async Task<BoosterPack> Parse()
        {

            var url = string.Format(ConstantString.MediaWikiParseIdUrl, _id);
            var dom = await GetDom(url);
            var parserOutput = dom.GetElementByClassName("mw-parser-output");
            var dates = GetReleaseDates(parserOutput);
            var table = dom.GetElementsByClassName("card-list").FirstOrDefault()?.FirstElementChild.Children;

            if (table is null)
                throw new NullReferenceException($"No card list exists for {_name}");

            var tableHead = table.First();
            var nameIndex = GetColumnIndex(tableHead, "name");
            var rarityIndex = GetColumnIndex(tableHead, "rarity");
            var cardTable = table.Skip(1);
            var cards = new List<BoosterPackCard>();

            foreach (var row in cardTable)
            {

                var name = TrimName(row.Children[nameIndex].TextContent.Trim().Trim('"'));
                List<string> rarities;

                if (rarityIndex == -1)
                    rarities = null;
                else
                    rarities = row
                        .Children[rarityIndex]
                        .Children.Select(element => element.TextContent.Trim())
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList();

                var card = new BoosterPackCard { Name = name, Rarities = rarities };

                cards.Add(card);

            }

            return new BoosterPack()
            {

                Id = int.Parse(_id),
                Name = _name,
                Dates = dates,
                Cards = cards,
                Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, _id)

            };

        }

        private static List<BoosterPackDate> GetReleaseDates(IElement parserOutput)
        {

            var dates = new List<BoosterPackDate>();
            var infobox = parserOutput.GetElementsByClassName("infobox").FirstOrDefault()?.FirstElementChild.Children;
            var releaseDateHeader = infobox.FirstOrDefault(element => !string.IsNullOrEmpty(element.TextContent) && element.TextContent.Contains("release dates", StringComparison.InvariantCultureIgnoreCase));

            if (releaseDateHeader is not null)
            {

                var startIndex = infobox.Index(releaseDateHeader) + 1;

                for (int i = startIndex; i < infobox.Length; i++)
                {

                    var dateInfo = infobox[i];

                    if (dateInfo.Children.Length == 2)
                    {

                        var region = dateInfo.FirstElementChild.TextContent.Trim();
                        var date = dateInfo.Children[1].TextContent.Trim();

                        if (date?.Contains('[') == true)
                        {

                            var openBracketIndex = date.IndexOf('[');
                            date = date.Substring(0, openBracketIndex);

                        }

                        dates.Add(new BoosterPackDate
                        {
                            Name = region,
                            Date = date
                        });

                    }
                    else
                        break;

                }

            }

            return dates;

        }

        private static int GetColumnIndex(IElement tableHead, string name)
        {

            var column = tableHead.Children.FirstOrDefault(element => element.TextContent.Contains(name, StringComparison.OrdinalIgnoreCase));

            return tableHead.Children.Index(column);

        }

        private static string TrimName(string name)
        {

            if (name.StartsWith('"') && name.EndsWith('"'))
            {

                if (name[name.Length - 1] == '"' && name[name.Length - 2] == '"')
                    name = name.TrimStart('"').Substring(0, name.Length - 2);
                else
                    name = name.Trim('"');

            }

            return name.Trim();

        }

        private static async Task<IDocument> GetDom(string url)
        {

            var parseResponse = await Constant.HttpClient.GetStringAsync(url);
            var parseJToken = JObject.Parse(parseResponse)["parse"];
            var html = parseJToken.Value<string>("text") ?? parseJToken["text"].Value<string>("*");

            return await Constant.HtmlParser.ParseDocumentAsync(html);

        }

    }

}
