using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;

namespace YuGiOh.Scraper.Models.Parsers.Fandom;

public abstract class BaseFandomCardParser : BaseCardParser
{

    protected virtual Task<IElement> GetParserOutput(IDocument dom)
        => Task.FromResult(dom.GetElementByClassName("mw-parser-output"));

    protected virtual async Task<IDocument> GetDom(string url)
    {

        var parseResponse = await Constant.HttpClient.GetStringAsync(url);
        var parseJToken = JObject.Parse(parseResponse)["parse"];
        var html = parseJToken?.Value<string>("text") ?? parseJToken?["text"]?.Value<string>("*");

        return html is not null ? await Constant.HtmlParser.ParseDocumentAsync(html) : null;

    }

}