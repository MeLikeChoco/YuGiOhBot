using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;

namespace YuGiOh.Scraper.Models.Parsers.Yugipedia;

[ParserModule(ConstantString.YugipediaModuleName)]
public class BoosterPackParser(string id, string name) : BaseBoosterParser
{

    private IElement _parserOutput;
    private IHtmlCollection<IElement> _table;

    protected override async Task BeforeParseAsync()
    {

        await base.BeforeParseAsync();

        var url = ConstantString.YugipediaUrl + string.Format(ConstantString.MediaWikiParseIdUrl, id);
        var dom = await YugipediaParserTools.GetDom(url);
        _parserOutput = await YugipediaParserTools.GetParserOutput(dom);
        _table = dom.GetElementsByClassName("card-list").FirstOrDefault()?.FirstElementChild?.Children;

    }

    protected override Task<int> GetId()
        => Task.FromResult(int.Parse(id));

    protected override Task<string> GetName()
        => Task.FromResult(name);

    protected override Task<List<BoosterPackDateEntity>> GetDates()
    {

        var dates = new List<BoosterPackDateEntity>();
        var infobox = _parserOutput.GetElementByClassName("infobox")?.FirstElementChild?.Children;
        var releaseDateHeader = infobox?.FirstOrDefault(element => !string.IsNullOrEmpty(element.TextContent) && element.TextContent.Contains("release dates", StringComparison.InvariantCultureIgnoreCase));

        if (releaseDateHeader is null)
            return Task.FromResult(dates);

        var startIndex = infobox.Index(releaseDateHeader) + 1;

        for (var i = startIndex; i < infobox.Length; i++)
        {

            var dateInfo = infobox[i];

            if (dateInfo.Children.Length == 2)
            {

                var region = dateInfo.FirstElementChild?.TextContent.Trim();
                var date = dateInfo.Children[1].TextContent.Trim();

                if (date?.Contains('[') == true)
                {

                    var openBracketIndex = date.IndexOf('[');
                    date = date[..openBracketIndex];

                }

                dates.Add(new BoosterPackDateEntity
                {
                    Name = region,
                    Date = date
                });

            }
            else
                break;

        }

        return Task.FromResult(dates);

    }

    protected override Task<List<BoosterPackCardEntity>> GetCards()
    {

        var tableHead = _table.First();
        var nameIndex = GetColumnIndex(tableHead, "name");
        var rarityIndex = GetColumnIndex(tableHead, "rarity");
        var cardTable = _table.Skip(1);
        var cards = new List<BoosterPackCardEntity>();

        foreach (var row in cardTable)
        {

            var name = TrimName(row.Children[nameIndex].TextContent.Trim().Trim('"'));
            var rarities = new List<string>();

            if (rarityIndex != -1)
                rarities = row
                    .Children[rarityIndex]
                    .Children.Select(element => element.TextContent.Trim())
                    .Where(text => !string.IsNullOrEmpty(text))
                    .ToList();

            var card = new BoosterPackCardEntity { Name = name, Rarities = rarities };

            cards.Add(card);

        }

        return Task.FromResult(cards);

    }

    private static int GetColumnIndex(IParentNode tableHead, string name)
    {

        var column = tableHead.Children.FirstOrDefault(element => element.TextContent.Contains(name, StringComparison.OrdinalIgnoreCase));

        return tableHead.Children.Index(column);

    }

    private static string TrimName(string name)
    {

        if (!name.StartsWith('"') || !name.EndsWith('"'))
            return name.Trim();

        if (name[^1] == '"' && name[^2] == '"')
            name = name.TrimStart('"')[..(name.Length - 2)];
        else
            name = name.Trim('"');

        return name.Trim();

    }

    protected override Task<string> GetUrl()
        => Task.FromResult(string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, id));

    protected override Task<bool> GetOcgExists()
        => Task.FromResult(false);

    protected override Task<bool> GetTcgExists()
        => Task.FromResult(false);

}