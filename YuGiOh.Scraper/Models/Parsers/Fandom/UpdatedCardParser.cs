using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;

namespace YuGiOh.Scraper.Models.Parsers.Fandom;

[ParserModule("fandom")]
public class UpdatedCardParser : BaseFandomCardParser
{

    private readonly string _id, _name;
    private IElement _parserOutput, _table;
    private IDictionary<string, IElement> _tableRows;

    public UpdatedCardParser(string id, string name)
    {
        _id = id;
        _name = name;
    }

    protected override async Task BeforeParseAsync()
    {

        await base.BeforeParseAsync();

        var url = ConstantString.YuGiOhFandomUrl + string.Format(ConstantString.MediaWikiParseIdUrl, _id);
        var dom = await GetDom(url);
        _parserOutput = await GetParserOutput(dom);
        _table = _parserOutput.GetElementByClassName("card-table");
        _tableRows = _table
            .GetElementByClassName("innertable")
            ?
            .GetElementsByTagName("tr")
            .Where(row => !string.IsNullOrWhiteSpace(row.FirstChild?.TextContent))
            .ToDictionary(
                row => row.FirstChild?.TextContent,
                row => row.Children.ElementAtOrDefault(1),
                StringComparer.OrdinalIgnoreCase
            );

    }

    protected override Task<int> GetId()
        => Task.FromResult(int.Parse(_id));

    protected override Task<string> GetName()
        => Task.FromResult(_name);

    protected override Task<string> GetRealName()
        => Task.FromResult(_table.FirstElementChild?.TextContent.Trim());

    protected override Task<string> GetCardType()
        => GetRowContent("Card type");

    protected override Task<string> GetProperty()
        => GetRowContent("Property");

    protected override async Task<string> GetTypes()
        => await GetRowContent("Types") ?? await GetRowContent("Type");

    protected override Task<string> GetAttribute()
        => GetRowContent("Attribute");

    protected override Task<string> GetMaterials()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetLore()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetPendulumLore()
    {
        throw new NotImplementedException();
    }

    protected override Task<List<TranslationEntity>> GetTranslations()
    {
        throw new NotImplementedException();
    }

    protected override Task<List<string>> GetArchetypes()
    {
        throw new NotImplementedException();
    }

    protected override Task<List<string>> GetSupports()
    {
        throw new NotImplementedException();
    }

    protected override Task<List<string>> GetAntiSupports()
    {
        throw new NotImplementedException();
    }

    protected override Task<int> GetLinkCount()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetLinkArrows()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetAtk()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetDef()
    {
        throw new NotImplementedException();
    }

    protected override Task<int> GetLevel()
    {
        throw new NotImplementedException();
    }

    protected override Task<int> GetPendulumScale()
    {
        throw new NotImplementedException();
    }

    protected override Task<int> GetRank()
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> GetTcgExists()
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> GetOcgExists()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetImgLink()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetUrl()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetPasscode()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetOcgStatus()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetTcgAdvStatus()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetTcgTrnStatus()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetCardTrivia()
    {
        throw new NotImplementedException();
    }

    private Task<string> GetRowContent(string header)
        => Task.FromResult(
            _tableRows.TryGetValue(header, out var row) ?
                row.TextContent.Trim() :
                null
        );

}