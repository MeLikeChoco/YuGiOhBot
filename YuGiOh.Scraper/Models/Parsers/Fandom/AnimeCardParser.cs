using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using YuGiOh.Common.Extensions;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;
using YuGiOh.Scraper.Models.ParserOptions;

namespace YuGiOh.Scraper.Models.Parsers.Fandom;

[ParserModule(ConstantString.FandomModuleName)]
public class AnimeCardParser : ICanParse<AnimeCardEntity>
{

    private readonly string _name, _id;
    private readonly Options _options;

    public AnimeCardParser(string name, string id, Options options)
    {
        _name = name;
        _id = id;
        _options = options;
    }

    public async Task<AnimeCardEntity> ParseAsync()
    {

        var url = ConstantString.YuGiOhFandomUrl + string.Format(ConstantString.MediaWikiParseIdUrl, _id);
        var parserOutput = await GetDom(url);
        var cardTableRows = parserOutput.GetElementByClassName("infocolumn").GetElementsByTagName("tr");
        var imgElement = parserOutput.GetElementByClassName("cardtable-main_image-wrapper").GetElementByTagName("img");
        var imgUrl = imgElement?.GetAttribute("src") ?? imgElement?.GetAttribute("data-src");

        if (!string.IsNullOrWhiteSpace(imgUrl))
        {

            const string type1Url = "scale-to-width-down";

            if (imgUrl.ContainsIgnoreCase(type1Url))
            {

                var cutoffIndex = imgUrl.IndexOf(type1Url, StringComparison.Ordinal);
                imgUrl = imgUrl[..cutoffIndex];

            }
            else
                imgUrl = new Uri(imgUrl).GetLeftPart(UriPartial.Path); //truncate ?cb=<stuff> querystring

        }

        var card = new AnimeCardEntity
        {

            Id = int.Parse(_id),
            Name = parserOutput.GetElementByClassName("heading").TextContent.Trim(),
            Img = imgUrl,
            Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, _id)

        };

        foreach (var row in cardTableRows)
        {

            var header = row.FirstElementChild?.TextContent?.Trim();
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
                        card.LinkArrows = data?.Replace(" , ", ", ");
                        break;
                    case "Appearances":
                    {

                        var rowData = row.Children
                            .ElementAtOrDefault(1)
                            ?
                            .GetElementsByTagName("li")
                            .Aggregate(new StringBuilder(""), (sb, element) =>
                            {

                                var appearance = element.TextContent?.Trim();

                                return string.IsNullOrWhiteSpace(appearance) ? sb : sb.AppendLine(appearance);

                            })
                            .ToString()
                            .Trim();

                        card.Appearances = rowData;

                        break;

                    }

                }

            }

        }

        return card;

    }

    private async Task<IDocument> GetDom(string url)
    {

        var parseResponse = await Constant.HttpClient.GetStringAsync(url);
        var parseJToken = JObject.Parse(parseResponse)["parse"];
        var html = parseJToken?.Value<string>("text") ?? parseJToken?["text"]?.Value<string>("*");

        return html is not null ? await Constant.HtmlParser.ParseDocumentAsync(html) : null;

    }

}