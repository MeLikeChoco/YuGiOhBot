using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;

namespace YuGiOh.Scraper.Models.Parsers.Yugipedia;

[ParserModule(ConstantString.YugipediaModuleName)]
public class AnimeCardParser : ICanParse<AnimeCardEntity>
{

    private readonly string _id, _name;

    public AnimeCardParser(string id, string name)
    {
        _id = id;
        _name = name;
    }

    public async Task<AnimeCardEntity> ParseAsync()
    {

        var url = ConstantString.YugipediaUrl + string.Format(ConstantString.MediaWikiParseIdUrl, _id);
        var dom = await GetDom(url);
        var parserOutput = dom.GetElementByClassName("mw-parser-output");
        var cardTable = parserOutput.GetElementByClassName("card-table");
        var img = cardTable.GetElementByClassName("cardtable-main_image-wrapper").GetElementsByTagName("img").FirstOrDefault();

        var card = new AnimeCardEntity
        {

            Id = int.Parse(_id),
            Name = cardTable.GetElementByClassName("heading").TextContent.Trim(),
            Img = img?.GetAttribute("srcset")?.Split(' ').FirstOrDefault() ?? img?.GetAttribute("src"),
            Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, _id)

        };

        var cardInfoList = cardTable.GetElementByClassName("innertable").GetElementsByTagName("tr");

        foreach (var row in cardInfoList)
        {

            var header = row.FirstChild?.TextContent?.Trim();
            var data = row.Children.ElementAtOrDefault(1)?.TextContent?.Trim();

            if (header is not null)
            {

                switch (header)
                {

                    case "Card type":
                        card.CardType = data;
                        break;
                    case "Attribute":
                        card.Attribute = data;
                        break;
                    case "Property":
                        card.Property = data;
                        break;
                    case "Level":
                        card.Level = data;
                        break;
                    case "Rank":
                        card.Rank = data;
                        break;
                    case "Pendulum Scale":
                        card.PendulumScale = data;
                        break;
                    case "Link Arrows":
                        card.LinkArrows = data.Replace(" , ", ", ");
                        break;
                    case "Appearances":
                        var rowData = row.Children
                            .ElementAtOrDefault(1)
                            ?
                            .GetElementByClassName("plainlist")
                            .FirstElementChild?
                            .Children
                            .Aggregate("", (aggregator, appearances) =>
                            {

                                var appearance = appearances.TextContent;

                                return !string.IsNullOrWhiteSpace(appearance) ? $"{aggregator}\n{appearances.TextContent.Trim()}" : aggregator;

                            })
                            .Trim();

                        card.Appearances = rowData;
                        break;

                }

            }

        }

        GetLoreboxInfo(cardTable, card);

        return card;

    }

    private static void GetLoreboxInfo(IElement cardTable, AnimeCardEntity card)
    {

        var loreboxes = cardTable.GetElementsByClassName("lorebox");

        if (loreboxes.Length == 0)
            return;

        var regularLore = loreboxes.Length == 1 ? loreboxes.FirstOrDefault() : loreboxes[1];

        foreach (var citation in regularLore.GetElementsByTagName("sup"))
            citation.RemoveFromParent();

        card.Lore = regularLore.GetElementByClassName("lorebox-lore").TextContent?.Trim();

        if (!card.CardType.Equals("Monster", StringComparison.OrdinalIgnoreCase))
            return;

        card.Types = regularLore.GetElementByClassName("card-table-types").TextContent;
        var statLines = regularLore.GetElementByClassName("lorebox-stats")?.GetElementsByTagName("a");

        if (statLines is { Length: > 0 })
        {

            card.Atk = statLines[1].TextContent;
            var secondaryStat = statLines[3].TextContent;

            if (card.Types.Contains("Link", StringComparison.OrdinalIgnoreCase))
                card.Link = int.Parse(secondaryStat);
            else
                card.Def = secondaryStat;

        }

        if (!card.Types.Contains("pendulum", StringComparison.OrdinalIgnoreCase))
            return;

        var pendulumLore = loreboxes.FirstOrDefault().GetElementByClassName("lorebox-lore").TextContent?.Trim();
        card.Lore = $"Pendulum Effect${pendulumLore}Monster Effect${card.Lore}";

    }

    private async Task<IDocument> GetDom(string url)
    {

        var parseResponse = await Constant.HttpClient.GetStringAsync(url);
        var parseJToken = JObject.Parse(parseResponse)["parse"];
        var html = parseJToken?.Value<string>("text") ?? parseJToken?["text"]?.Value<string>("*");

        return html is not null ? await Constant.HtmlParser.ParseDocumentAsync(html) : null;

    }

}