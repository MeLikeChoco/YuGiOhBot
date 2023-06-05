using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using YuGiOh.Common.Extensions;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Extensions;
using YuGiOh.Scraper.Models.ParserOptions;

namespace YuGiOh.Scraper.Models.Parsers.Yugipedia;

[ParserModule("yugipedia")]
public class CardParser : ICanParse<CardEntity>
{

    private static readonly string[] ArchetypesHeaderMustHaveWords = { "archetypes", "series" };
    private static readonly string[] PendulumLoreAdditionalTypesCanHaveWords = { "synchro", "fusion", "xyz" };

    private string _parseOutput;

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

        var dom = await GetDom();
        var parserOutput = dom.GetElementByClassName("mw-parser-output");
        var imgElement = parserOutput.GetElementByClassName("cardtable-main_image-wrapper")?.GetElementByTagName("img");

        var card = new CardEntity
        {

            Id = int.Parse(_id),
            Name = _name,
            Img = imgElement?.GetAttribute("srcset")?.Split(' ').ElementAtOrDefault(2) ?? imgElement?.GetAttribute("src"),
            Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, _id)

        };

        var table = parserOutput.GetElementByClassName("card-table");
        var realName = table.FirstElementChild?.TextContent.Trim();

        if (_name != realName)
            card.RealName = realName;

        //don't inline for readability
        var tableRows = table.GetElementByClassName("innertable")?.GetElementsByTagName("tr");

        foreach (var row in tableRows)
        {

            #region Card Data

            //firstordefault because of statuses not always having a header
            var header = row.FirstChild?.TextContent?.Trim();
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
                    //var statuses = tableRows.Where(element => !element.Children.Any(child => child.ClassName == "cardtablerowheader" || child.ClassName == "cardtablespanrow") &&
                    //    element.Children.Length == 1).Select(element => element.FirstElementChild);
                    var statuses = row.Children[1].FirstElementChild?.Children ?? row.Children[1].Children;

                    foreach (var element in statuses)
                    {

                        var rawStatus = element.TextContent;
                        var status = rawStatus.Trim();
                        var index = status.IndexOf('(');

                        if (index != -1)
                            status = status[..index].Trim();

                        if (rawStatus.ContainsIgnoreCase("ocg"))
                            card.OcgStatus = status;
                        else if (rawStatus.ContainsIgnoreCase("tcg"))
                        {

                            if (rawStatus.ContainsIgnoreCase("speed duel") || rawStatus.ContainsIgnoreCase("traditional"))
                                card.TcgTrnStatus = status;
                            else
                                card.TcgAdvStatus = status;

                        }
                        //else if (rawStatus.ContainsIgnoreCase("trad"))
                        //    card.TcgTrnStatus = status;
                        //else if (rawStatus.ContainsIgnoreCase("tcg"))
                        //    card.TcgAdvStatus = status;

                    }

                    break;

            }

            #endregion Card Data

            #region Old Code

            //if (row.FirstElementChild.ClassName == "cardtablespanrow")
            //{

            //    //for future stuff
            //    #region Lore
            //    if (card.PendulumScale == -1 && row.FirstElementChild.FirstElementChild.TagName == "P")
            //    {

            //        var descriptionUnformatted = row.FirstElementChild;
            //        var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", "\\n"), "<[^>]*>", "").Trim();
            //        card.Lore = WebUtility.HtmlDecode(descriptionFormatted);

            //    }
            //    else if (card.PendulumScale > -1 && row.TextContent.Contains("effect", StringComparison.InvariantCultureIgnoreCase))
            //        card.Lore = row.TextContent.Replace("\n", " ").Trim();
            //    #endregion Lore

            //}
            //else
            //{

            //    #region Card Data
            //    //firstordefault because of statuses not always having a header
            //    var header = row.FirstChild?.TextContent?.Trim();
            //    var data = row.Children.ElementAtOrDefault(1)?.TextContent?.Trim();

            //    switch (header)
            //    {

            //        case "Card type":
            //            card.CardType = data;
            //            break;
            //        case "Attribute":
            //            card.Attribute = data;
            //            break;
            //        case "Types":
            //        case "Type":
            //            card.Types = data;
            //            break;
            //        case "Level":
            //            card.Level = int.Parse(data);
            //            break;
            //        case "Rank":
            //            card.Rank = int.Parse(data);
            //            break;
            //        case "Pendulum Scale":
            //            card.PendulumScale = int.Parse(data);
            //            break;
            //        case "Materials":
            //            card.Materials = data;
            //            break;
            //        case "ATK / DEF":
            //            var array = data.Split(new string[] { " / " }, StringSplitOptions.None);
            //            card.Atk = array[0];
            //            card.Def = array[1];
            //            break;
            //        case "ATK / LINK":
            //            array = data.Split(new string[] { " / " }, StringSplitOptions.None);

            //            if (array.Length == 2)
            //            {

            //                card.Atk = array[0];
            //                card.Link = int.Parse(array[1]);

            //            }
            //            else
            //                card.Level = int.Parse(array[0]);

            //            break;
            //        case "Property":
            //            card.Property = data;
            //            break;
            //        case "Link Arrows":
            //            card.LinkArrows = data.Replace(" , ", ", ");
            //            break;
            //        case "Password":
            //        case "Passcode":
            //            card.Passcode = data.TrimStart('0');
            //            break;
            //        case "Status":
            //        case "Statuses":
            //            //var statuses = tableRows.Where(element => !element.Children.Any(child => child.ClassName == "cardtablerowheader" || child.ClassName == "cardtablespanrow") &&
            //            //    element.Children.Length == 1).Select(element => element.FirstElementChild);
            //            var statuses = row.Children.ElementAtOrDefault(1)?.Children;
            //            var count = statuses.Count();
            //            var index = data.IndexOf('(');

            //            if (count > 0)
            //                card.OcgStatus = data.Substring(0, index).TrimEnd();
            //            else
            //                card.OcgStatus = data;

            //            if (count == 2)
            //            {

            //                card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
            //                card.TcgTrnStatus = statuses.ElementAt(1).FirstElementChild.TextContent;

            //            }
            //            else if (count == 1)
            //            {

            //                card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
            //                card.TcgTrnStatus = card.TcgAdvStatus;

            //            }
            //            else
            //            {

            //                card.TcgAdvStatus = card.OcgStatus;
            //                card.TcgTrnStatus = card.OcgStatus;

            //            }

            //            break;

            //    }
            //    #endregion Card Data

            //}

            #endregion Old Code

        }

        GetLore(card, parserOutput);
        GetArchetypesSupportsAnti(card, parserOutput);
        GetTranslations(card, parserOutput);

        //card.CardTrivia = await GetCardTrivia(parserOutput);

        return card;

    }

    private static void GetLore(CardEntity card, IElement parserOutput)
    {

        var table = parserOutput.GetElementByClassName("card-table");
        var loreUnformatted = table
            .GetElementByClassName("lore")
            .InnerHtml;

        var loreFormatted = Regex
            .Replace(loreUnformatted, "<(?!br[\x20/>])[^<>]+>", "")
            .Replace("<br>", "\n")
            .Trim();

        loreFormatted = WebUtility.HtmlDecode(loreFormatted);
        var isPendulum = card.PendulumScale > -1;

        if (isPendulum)
        {

            var splitLore = loreFormatted.Split("Monster Effect");

            if (splitLore.Length < 2)
                card.Lore = splitLore[0].Trim();
            else
            {

                var pendulumLore = splitLore[0].Replace("Pendulum Effect", "").Trim();
                var lore = splitLore[1].Trim();

                card.PendulumLore = pendulumLore;
                card.Lore = lore;

            }

        }
        else
            card.Lore = loreFormatted;

    }

    #region Search Categories

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
                .Select(element => element.TextContent.Trim())
                .ToList();

            if (
                (ArchetypesHeaderMustHaveWords.All(word => header.ContainsIgnoreCase(word)) && !header.ContainsIgnoreCase("related"))
                || (header.ContainsIgnoreCase("supports") && header.ContainsIgnoreCase("archetypes"))
            ) //filter out "related archetypes and series"
                card.Archetypes = card.Archetypes is null ? value : card.Archetypes.Union(value).ToList();

            if (header.ContainsIgnoreCase("anti-supports"))
                card.AntiSupports = value;

            if (
                (header.ContainsIgnoreCase("supports") && !header.ContainsIgnoreCase("archetypes"))
                || (header.ContainsIgnoreCase("supports") && header.ContainsIgnoreCase("archetypes"))
            )
                card.Supports = card.Supports is null ? value : card.Supports.Union(value).ToList();

        }

        return card;

    }

    private static IEnumerable<IElement> GetSearchCategories(IElement parserOutput)
    {

        var firstH2Element = parserOutput.Children.FirstOrDefault(element => element.TagName.ContainsIgnoreCase("h2") && element.TextContent.ContainsIgnoreCase("search categories"));

        if (firstH2Element is null)
            return null;

        var startIndex = parserOutput.Children.Index(firstH2Element) + 1;
        var searchCategories = new List<IElement>();
        IElement currentElement;

        for (var i = startIndex; i < parserOutput.Children.Length; i++)
        {

            currentElement = parserOutput.Children[i];

            if (currentElement.ClassName?.ContainsIgnoreCase("hlist") == true)
                searchCategories.Add(currentElement);
            else
                break;

        }

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

        if (sectionHeader is null)
            return null;

        var sectionContent = sectionHeader.NextElementSibling;

        return sectionContent.FirstElementChild.Children.Skip(1); //skip the table header

    }

    #endregion Translations

    private async Task<string> GetCardTrivia(IElement parserOutput)
    {

        var triviaUrlElement = parserOutput
            .GetElementsByClassName("hlist")
            .FirstOrDefault(element => element.TextContent.ContainsIgnoreCase("Trivia"))
            ?
            .FirstElementChild?.Children
            .FirstOrDefault(element => element.TextContent.ContainsIgnoreCase("Trivia"));

        if (triviaUrlElement is null)
            return null;

        var triviaLink = triviaUrlElement.GetElementByTagName("a").GetAttribute("href");

        if (triviaLink.ContainsIgnoreCase("redlink=1"))
            return null;

        // var triviaUrl = string.Format(ConstantString.MediaWikiParseNameUrl, Uri.EscapeDataString(triviaLink));
        var dom = await GetDom();
        parserOutput = dom.GetElementByClassName("mw-parser-output");
        var triviaElements = parserOutput?.Children
            .Where(element => element.TagName == "UL");

        if (triviaElements is null)
            return null;

        var triviaEleArray = triviaElements as IElement[] ?? triviaElements.ToArray();
        var trivias = new List<string>(triviaEleArray.Length);

        trivias.AddRange(triviaEleArray.Select(triviaElement => triviaElement.InnerHtml)
            .Select(html
                => Regex.Replace(html, ConstantString.HtmlTagRegex, "").Replace("\n", "\\n")
            ));

        return string.Join('|', trivias);

    }

    private async Task<string> GetParseOutput()
    {

        if (_parseOutput != null)
            return _parseOutput;

        var url = string.Format(ConstantString.MediaWikiParseIdUrl, _id);
        _parseOutput = await Constant.GetHttpClient(_options).GetStringAsync(url);

        return _parseOutput;

    }

    private async Task<IDocument> GetDom()
    {

        var parseResponse = await GetParseOutput();
        var parseJToken = JObject.Parse(parseResponse)["parse"];
        var html = parseJToken?.Value<string>("text") ?? parseJToken?["text"]?.Value<string>("*");

        return html is not null ? await Constant.HtmlParser.ParseDocumentAsync(html) : null;

    }

}