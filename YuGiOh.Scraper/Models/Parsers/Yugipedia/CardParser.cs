using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Dom;
using YuGiOh.Common.Extensions;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;

namespace YuGiOh.Scraper.Models.Parsers.Yugipedia;

[ParserModule(ConstantString.YugipediaModuleName)]
public class CardParser(string id, string name) : BaseCardParser
{

    private static readonly string[] ArchetypesHeaderMustHaveWords = { "archetypes", "series" };
    private static readonly string[] StatusHeaderMayHaveWords = { "Status", "Statuses" };

    private string[] _lore;
    private IElement _parserOutput, _table;
    private Dictionary<string, string> _tableRows;
    private Dictionary<string, IEnumerable<IElement>> _searchCategories;

    protected override async Task BeforeParseAsync()
    {

        await base.BeforeParseAsync();

        var url = ConstantString.YugipediaUrl + string.Format(ConstantString.MediaWikiParseIdUrl, id);
        var dom = await YugipediaParserTools.GetDom(url);
        _parserOutput = await YugipediaParserTools.GetParserOutput(dom);
        _table = _parserOutput.GetElementByClassName("card-table");
        _tableRows = _table
            .GetElementByClassName("innertable")
            ?
            .GetElementsByTagName("tr")
            .Where(row => !string.IsNullOrWhiteSpace(row.FirstChild?.FirstChild?.TextContent))
            .ToDictionary(
                row => row.TextContent.Trim(),
                row => row.Children.ElementAtOrDefault(1)?.TextContent.Trim(),
                StringComparer.OrdinalIgnoreCase
            );

        var loreUnformatted = _table
            .GetElementByClassName("lore")
            .InnerHtml;

        var loreFormatted = ConstantRegex.HtmlNewLine()
            .Replace(loreUnformatted, "")
            .Replace("<br>", "\n")
            .Trim();

        _lore = WebUtility.HtmlDecode(loreFormatted).Split("Monster Effect");

        _searchCategories = _parserOutput.Children
            .SkipWhile(element => element.TextContent.Trim() != "Search categories")
            .Skip(1)
            .TakeWhile(element => element.ClassName == "hlist")
            .Select(element => element.FirstElementChild)
            .ToDictionary(
                element => element!.FirstElementChild!.TextContent,
                element => element!.Children.Skip(1),
                StringComparer.OrdinalIgnoreCase
            );

    }

    protected override Task<int> GetId()
        => Task.FromResult(int.Parse(id));

    protected override Task<string> GetName()
        => Task.FromResult(name);

    protected override Task<string> GetRealName()
    {

        var realName = _table.FirstElementChild?.TextContent.Trim();

        return Task.FromResult(name != realName ? realName : null);

    }

    protected override Task<string> GetCardType()
        => Task.FromResult(
            _tableRows.TryGetValue("Card type", out var cardType) ?
                cardType :
                null
        );

    protected override Task<string> GetProperty()
        => Task.FromResult(
            _tableRows.TryGetValue("Property", out var property) ?
                property :
                null
        );

    protected override Task<string> GetTypes()
    {

        if (_tableRows.TryGetValue("Types", out var types) || _tableRows.TryGetValue("Type", out types))
            return Task.FromResult(types);

        return Task.FromResult<string>(null);

    }

    protected override Task<string> GetAttribute()
        => Task.FromResult(
            _tableRows.TryGetValue("Attribute", out var attribute) ?
                attribute :
                null
        );

    protected override Task<string> GetMaterials()
        => Task.FromResult(
            _tableRows.TryGetValue("Materials", out var materials) ?
                materials :
                null
        );

    protected override Task<string> GetLore()
    {

        var isPendulum = _tableRows.TryGetValue("Pendulum Scale", out var scale) && !string.IsNullOrWhiteSpace(scale);

        return isPendulum ?
            Task.FromResult(_lore.Length < 2 ?
                _lore[0].Trim() :
                _lore[1].Trim()) :
            Task.FromResult(_lore[0].Trim());

    }

    protected override Task<string> GetPendulumLore()
    {

        var isPendulum = _tableRows.TryGetValue("Types", out var types) && types.ContainsIgnoreCase("pendulum");

        return Task.FromResult(isPendulum ? _lore[0].Replace("Pendulum Effect", "").Trim() : null);

    }

    protected override Task<List<TranslationEntity>> GetTranslations()
    {

        //using queryselector because IElement doesn't have GetElementById
        var sectionHeader = _parserOutput.QuerySelector("#Other_languages")?.ParentElement;

        if (sectionHeader is null)
            return Task.FromResult(new List<TranslationEntity>());

        var sectionContent = sectionHeader.NextElementSibling;

        if (sectionContent is null)
            return Task.FromResult(new List<TranslationEntity>());

        var translations = sectionContent
            .FirstElementChild!
            .Children
            .Skip(1)
            .Where(row => row.Children.Length == 3)
            .Select(row =>
            {

                var translation = new TranslationEntity
                {

                    Language = row.Children[0].TextContent.Trim(),
                    Lore = row.Children[2].TextContent.Trim()

                };

                //man, why does only japanese have this problem reeeeeeeeeeeeeeeeeeee
                if (translation.Language.Equals("japanese", StringComparison.OrdinalIgnoreCase))
                {

                    var rubyChildren = row.GetElementsByTagName("ruby").SelectMany(element => element.Children);

                    //don't inline for readibility
                    foreach (var child in rubyChildren)
                    {
                        if (child.LocalName != "rb")
                            child.Remove();
                    }

                    translation.Name = row.Children[1].TextContent.Trim();

                }
                else
                    translation.Name = row.Children[1].TextContent.Trim();

                return translation;

            })
            .ToList();

        return Task.FromResult(translations);

    }

    protected override Task<List<string>> GetArchetypes()
    {

        var archetypes = Enumerable.Empty<string>();

        var archetypeRow = _searchCategories
            .FirstOrDefault(kv => ArchetypesHeaderMustHaveWords.All(word => kv.Key.ContainsIgnoreCase(word)) && !kv.Key.ContainsIgnoreCase("related"));

        if (archetypeRow.Value != null)
            archetypes = archetypes.Union(archetypeRow.Value.Select(element => element.TextContent.Trim()));

        var supportsArchetypesRow = _searchCategories
            .FirstOrDefault(kv => kv.Key.ContainsIgnoreCase("supports archetypes"));

        if (supportsArchetypesRow.Value != null)
            archetypes = archetypes.Union(supportsArchetypesRow.Value.Select(element => element.TextContent.Trim()));

        return Task.FromResult(archetypes.ToList());

    }

    protected override Task<List<string>> GetSupports()
    {

        var supportRow = _searchCategories
            .FirstOrDefault(kv => kv.Key == "Supports");

        return supportRow.Equals(default(KeyValuePair<string, IEnumerable<IElement>>)) ?
            Task.FromResult(new List<string>()) :
            Task.FromResult(supportRow.Value.Select(element => element.TextContent.Trim()).ToList());

    }

    protected override Task<List<string>> GetAntiSupports()
    {

        var antiSupportRow = _searchCategories
            .FirstOrDefault(kv => kv.Key == "Anti-supports");

        return antiSupportRow.Equals(default(KeyValuePair<string, IEnumerable<IElement>>)) ?
            Task.FromResult(new List<string>()) :
            Task.FromResult(antiSupportRow.Value.Select(element => element.TextContent.Trim()).ToList());

    }

    protected override Task<int> GetLinkCount()
    {

        return Task.FromResult(
            _tableRows.TryGetValue("ATK / LINK", out var atkLink) ?
                int.Parse(atkLink.Split("/")[1].Trim()) :
                0
        );

    }

    protected override Task<string> GetLinkArrows()
    {

        return Task.FromResult(
            _tableRows.TryGetValue("Link Arrows", out var linkArrows) ?
                linkArrows :
                null
        );

    }

    protected override Task<string> GetAtk()
    {

        var atkRow = _tableRows.FirstOrDefault(kv => kv.Key.StartsWith("ATK"));

        return Task.FromResult(
            atkRow.Equals(default(KeyValuePair<string, string>)) ?
                null :
                atkRow.Value.Split('/')[0].Trim()
        );

    }

    protected override Task<string> GetDef()
    {
        return Task.FromResult(
            _tableRows.TryGetValue("ATK / DEF", out var atkLink) ?
                atkLink.Split('/')[1].Trim() :
                null
        );
    }

    protected override Task<int> GetLevel()
    {

        return Task.FromResult(
            _tableRows.TryGetValue("Level", out var level) ?
                int.Parse(level) :
                -1
        );

    }

    protected override Task<int> GetPendulumScale()
    {

        return Task.FromResult(
            _tableRows.TryGetValue("Pendulum Scale", out var scale) ?
                int.Parse(scale) :
                -1
        );

    }

    protected override Task<int> GetRank()
    {

        return Task.FromResult(
            _tableRows.TryGetValue("Rank", out var rank) ?
                int.Parse(rank) :
                -1
        );

    }

    protected override Task<bool> GetTcgExists()
        => Task.FromResult(
            (_tableRows.TryGetValue("Status", out var status) && status.ContainsIgnoreCase("tcg"))
            || (_tableRows.TryGetValue("Statuses", out status) && status.ContainsIgnoreCase("tcg"))
        );

    protected override Task<bool> GetOcgExists()
        => Task.FromResult(
            (_tableRows.TryGetValue("Status", out var status) && status.ContainsIgnoreCase("ocg"))
            || (_tableRows.TryGetValue("Statuses", out status) && status.ContainsIgnoreCase("ocg"))
        );

    protected override Task<string> GetImgLink()
        => Task.FromResult(
            (_parserOutput.GetElementByClassName("cardtable-cardimage") ?? _parserOutput.GetElementByClassName("cardtable-main_image-wrapper"))
            ?.GetElementByTagName("img")
            .GetAttribute("src")
        );

    protected override Task<string> GetUrl()
        => Task.FromResult(string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, id));

    protected override Task<string> GetPasscode()
        => Task.FromResult(_tableRows.GetValueOrDefault("Password"));

    protected override Task<string> GetOcgStatus()
    {

        var statusElement = _table
            .GetElementByClassName("innertable")
            ?
            .GetElementsByTagName("tr")
            .FirstOrDefault(element
                => StatusHeaderMayHaveWords.Any(word
                    => element.FirstElementChild?.TextContent?.ContainsIgnoreCase(word) == true));

        if (statusElement is null)
            return Task.FromResult("Unreleased");

        var status = statusElement.LastElementChild?.FirstElementChild?.FirstElementChild;

        return Task.FromResult(
            status?.TextContent.ContainsIgnoreCase("ocg") == true ?
                status.GetElementByTagName("a").TextContent.Trim() :
                "Unreleased"
        );

    }

    protected override Task<string> GetTcgAdvStatus()
    {

        var statusElement = _table
            .GetElementByClassName("innertable")
            ?
            .GetElementsByTagName("tr")
            .FirstOrDefault(element
                => StatusHeaderMayHaveWords.Any(word
                    => element.FirstElementChild?.TextContent.ContainsIgnoreCase(word) == true)
            );

        if (statusElement is null)
            return Task.FromResult("Unreleased");

        var status = statusElement
            .LastElementChild?
            .FirstElementChild?
            .Children
            .FirstOrDefault(element => element.TextContent.Trim().EndsWith("(TCG)"));

        return Task.FromResult(
            status != null ?
                status.GetElementByTagName("a").TextContent.Trim() :
                "Unreleased"
        );

    }

    protected override Task<string> GetTcgTrnStatus()
    {
        return Task.FromResult<string>(null);
    }

    protected override Task<string> GetCardTrivia()
    {
        return Task.FromResult<string>(null);
    }

}