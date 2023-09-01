using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;

namespace YuGiOh.Scraper.Models.Parsers.Yugipedia;

public static class YugipediaParserTools
{

    public static Task<IElement> GetParserOutput(IDocument dom)
        => Task.FromResult(dom.GetElementByClassName("mw-parser-output"));

    public static async Task<IDocument> GetDom(string url)
    {

        var parseResponse = await Constant.HttpClient.GetStringAsync(url);
        var parseJToken = JObject.Parse(parseResponse)["parse"];
        var html = parseJToken?.Value<string>("text") ?? parseJToken?["text"]?.Value<string>("*");

        return html is not null ? await Constant.HtmlParser.ParseDocumentAsync(html) : null;

    }

}