using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using YuGiOh.Common.Extensions;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;
using YuGiOh.Scraper.Models.Exceptions;
using YuGiOh.Scraper.Models.ParserOptions;

namespace YuGiOh.Scraper.Models.Parsers.Fandom;

[ParserModule("fandom")]
public class CardParser : IParser<CardEntity>
{

    private static readonly string[] ArchetypesHeaderMustHaveWords = { "archetypes", "series" };
    private static readonly string[] PendulumLoreAdditionalTypesCanHaveWords = { "synchro", "fusion", "xyz" };

    private readonly string _id, _name;
    private readonly Options _options;

    public CardParser(string name, string id, Options options)
    {
        _id = id;
        _name = name;
        _options = options;
    }

    public async Task<CardEntity> ParseAsync()
    {


        var url = string.Format(ConstantString.MediaWikiParseIdUrl, _id);
        var dom = await GetDom(url);
        var parserOutput = dom.GetElementByClassName("mw-parser-output");

        if (IsRushDuelCard(parserOutput))
            throw new RushDuelException(_id, _name, ConstantString.RushDuelCardExceptionMessage);

        var imgElement = parserOutput.GetElementByClassName("cardtable-cardimage")?.GetElementByTagName("img");

        var card = new CardEntity
        {

            Name = _name,
            Img = imgElement?.GetAttribute("src")

        };

        var tableRows = parserOutput.GetElementsByClassName("cardtablerow");
        var realname = tableRows.FirstOrDefault()?.Children.ElementAtOrDefault(1)?.TextContent.Trim();

        if (_name != realname)
            card.RealName = realname;

        foreach (var row in tableRows)
        {

            var header = row.FirstElementChild?.TextContent?.Trim();
            var data = row.Children.ElementAtOrDefault(1)?.TextContent?.Trim();

            switch (header)
            {

                case "Card type":
                    card.CardType = data;
                    break;
                case "Attribute":
                    card.Attribute = data;
                    break;
                case "Types":
                case "Type":
                    if (data.Contains("Skill"))
                        throw new SkillCardException(_id, _name, ConstantString.SkillCardExceptionMessage);

                    card.Types = data;
                    break;
                case "Level":
                    card.Level = int.Parse(data);
                    break;
                case "Rank":
                    card.Rank = int.Parse(data);
                    break;
                case "Pendulum Scale":
                    card.PendulumScale = int.Parse(data);
                    break;
                case "Materials":
                    card.Materials = data;
                    break;
                case "ATK / DEF":
                    var array = data.Split(new[] { " / " }, StringSplitOptions.None);
                    card.Atk = array[0];
                    card.Def = array[1];
                    break;
                case "ATK / LINK":
                    array = data.Split(new[] { " / " }, StringSplitOptions.None);

                    if (array.Length == 2)
                    {

                        card.Atk = array[0];
                        card.Link = int.Parse(array[1]);

                    }
                    else
                        card.Level = int.Parse(array[0]);

                    break;
                case "Property":
                    card.Property = data;
                    break;
                case "Link Arrows":
                    card.LinkArrows = data.Replace(" , ", ", ");
                    break;
                case "Password":
                case "Passcode":
                    card.Passcode = data.TrimStart('0');
                    break;
                case "Status":
                case "Statuses":
                {

                    var statuses = tableRows
                        .Where(element => !element
                                              .Children
                                              .Any(child => child.ClassName is "cardtablerowheader" or "cardtablespanrow") &&
                                          element.Children.Length == 1)
                        .Select(element => element.FirstElementChild)
                        .ToArray();
                    var count = statuses.Length;
                    var index = data.IndexOf('(');

                    card.OcgStatus = count > 0 ? data[..index].TrimEnd() : data;

                    switch (count)
                    {
                        case 2:
                            card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
                            card.TcgTrnStatus = statuses.ElementAt(1).FirstElementChild.TextContent;
                            break;
                        case 1:
                            card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
                            card.TcgTrnStatus = card.TcgAdvStatus;
                            break;
                        default:
                            card.TcgAdvStatus = card.OcgStatus;
                            card.TcgTrnStatus = card.OcgStatus;
                            break;
                    }

                    break;

                }

            }

        }

        GetLore(card, parserOutput);
        GetArchetypesSupportsAnti(card, parserOutput);
        GetTranslations(card, parserOutput);

        //card.CardTrivia = await GetCardTrivia(parserOutput);
        card.Url = string.Format(ConstantString.YuGiOhFandomUrl + ConstantString.MediaWikiIdUrl, _id);
        card.Id = int.Parse(_id);

        return card;

    }

    private static void GetLore(CardEntity card, IElement parserOutput)
    {
        var table = parserOutput.GetElementByClassName("cardtable");
        var loreUnformatted = table.Children
            .FirstOrDefault(element => element.TextContent.ContainsIgnoreCase("card description"))
            .GetElementByClassName("navbox-list")
            .InnerHtml;

        var loreFormatted = Regex
            .Replace(loreUnformatted, "<(?!br[\x20/>])[^<>]+>", "")
            .Replace("<br>", "\n")
            .Trim();

        var isPendulum = card.PendulumScale > -1;

        if (isPendulum)
        {

            loreFormatted = WebUtility.HtmlDecode(loreFormatted);
            var splitLore = loreFormatted.Split("Monster Effect:");

            if (splitLore.Length < 2)
            {

                var lore = TrimPropertyCardTextError(splitLore[0]);

                card.Lore = lore;

            }
            else
            {

                var pendulumLore = TrimPropertyCardTextError(splitLore[0]);
                var lore = TrimPropertyCardTextError(splitLore[1]);

                card.PendulumLore = pendulumLore;
                card.Lore = lore;

            }

        }
        else
        {

            loreFormatted = TrimPropertyCardTextError(loreFormatted);
            card.Lore = WebUtility.HtmlDecode(loreFormatted);

        }

        // var loreBox = table.GetElementByClassName("lore");
        //
        // if (isPendulum)
        //     return loreBox.TextContent.Replace("\n", "").Trim();
        //
        // //I need the new lines because of bullet points
        // var descriptionUnformatted = loreBox.FirstElementChild;
        // var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", "\\n"), ConstantString.HtmlTagRegex, "").Trim();
        //
        // return WebUtility.HtmlDecode(descriptionFormatted);

    }

    private static string TrimPropertyCardTextError(string str)
    {

        if (string.IsNullOrWhiteSpace(str))
            return str;

        var errorIndex = str.IndexOf("Property \"Card text", StringComparison.OrdinalIgnoreCase);

        return errorIndex != -1 ? str[..errorIndex].Trim() : str;

    }

    #region Search Categories

    private static bool IsRushDuelCard(IElement parserOutput)
    {

        var searchCategories = GetSearchCategories(parserOutput);

        return searchCategories?.Any(categories => categories.TextContent.ContainsIgnoreCase("rush duel")) == true;

    }

    private static CardEntity GetArchetypesSupportsAnti(CardEntity card, IElement parserOutput)
    {

        var searchCategories = GetSearchCategories(parserOutput)?.ToArray();

        if (searchCategories?.Any() != true)
            return card;

        foreach (var searchCategory in searchCategories)
        {

            var header = searchCategory.GetElementByTagName("dt").TextContent;
            var value = searchCategory.GetElementsByTagName("dd")
                .Where(element => !string.IsNullOrEmpty(element.TextContent))
                .Select(element => element.TextContent.Trim());

            //damn im fancy
            //please dont criticize, i like to appear flamboyantly unnecessary
            if (ArchetypesHeaderMustHaveWords.All(word => header.ContainsIgnoreCase(word)) && !header.ContainsIgnoreCase("related")) //filter out "related archetypes and series"
                card.Archetypes = value.ToList();
            else if (header.ContainsIgnoreCase("anti-supports"))
                card.AntiSupports = value.ToList();
            else if (header.ContainsIgnoreCase("supports") && !header.ContainsIgnoreCase("archetypes"))
                card.Supports = value.ToList();

        }

        return card;

    }

    private static IEnumerable<IElement> GetSearchCategories(IElement parserOutput)
    {

        var searchCategories = Enumerable.Empty<IElement>()
            .Union(
                parserOutput
                    .GetElementByClassName("cardtable-categories")
                    ?
                    .GetElementsByClassName("hlist") ?? Enumerable.Empty<IElement>()
            );

        return searchCategories;

    }

    #endregion Search Categories

    #region Translations

    private static CardEntity GetTranslations(CardEntity card, IElement parserOutput)
    {

        var translations = GetTranslationElements(parserOutput);

        if (translations is null)
            return card;

        card.Translations = new List<TranslationEntity>();

        foreach (var row in translations)
        {

            if (row.Children.Length != 3)
                continue;

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

            card.Translations.Add(translation);

        }

        return card;

    }

    private static IEnumerable<IElement> GetTranslationElements(IElement parserOutput)
    {

        var sectionHeader = parserOutput.QuerySelector("#Other_languages")?.ParentElement;

        var sectionContent = sectionHeader?.NextElementSibling;

        return sectionContent?.FirstElementChild?.Children.Skip(1); //skip the table header

    }

    #endregion Translations

    private async Task<IDocument> GetDom(string url)
    {

        var parseResponse = await Constant.GetHttpClient(_options).GetStringAsync(url);
        var parseJToken = JObject.Parse(parseResponse)["parse"];
        var html = parseJToken?.Value<string>("text") ?? parseJToken?["text"]?.Value<string>("*");

        return html is not null ? await Constant.HtmlParser.ParseDocumentAsync(html) : null;

    }

}