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

namespace YuGiOh.Scraper.Models.Parsers
{
    public class CardParser
    {

        private static readonly string[] ArchetypesHeaderMustHaveWords = new string[] { "archetypes", "series" };
        private static readonly string[] PendulumLoreAdditionalTypesCanHaveWords = new string[] { "synchro", "fusion", "xyz" };

        private string _parseOutput;

        public int Id { get; }
        public string Name { get; }

        public CardParser(string name, string id)
        {
            Name = name;
            Id = int.Parse(id);
        }

        public async Task<string> GetParseOutput()
        {

            if (_parseOutput == null)
            {

                var url = string.Format(ConstantString.MediaWikiParseIdUrl, Id);
                _parseOutput = await Constant.HttpClient.GetStringAsync(url);

            }

            return _parseOutput;

        }

        public async Task<CardEntity> Parse()
        {

            var dom = await GetDom();
            var parserOutput = dom.GetElementByClassName("mw-parser-output");
            var imgElement = parserOutput.GetElementByClassName("cardtable-main_image-wrapper").GetElementByTagName("img");

            var card = new CardEntity
            {

                Name = Name,
                Img = imgElement.GetAttribute("srcset")?.Split(' ').ElementAtOrDefault(2) ?? imgElement.GetAttribute("src")

            };

            var table = parserOutput.GetElementByClassName("card-table");
            var realName = table.FirstElementChild.TextContent?.Trim();

            if (Name != realName)
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
                        var array = data.Split(new string[] { " / " }, StringSplitOptions.None);
                        card.Atk = array[0];
                        card.Def = array[1];
                        break;
                    case "ATK / LINK":
                        array = data.Split(new string[] { " / " }, StringSplitOptions.None);

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
                        var count = statuses.Length;

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

            card.Lore = GetLore(card.PendulumScale, card.Types, parserOutput);

            #region Search Categories
            var searchCategories = GetSearchCategories(parserOutput);

            if (searchCategories?.Any() == true)
            {

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

            }
            #endregion Search Categories

            //card.CardTrivia = await GetCardTrivia(parserOutput);
            card.Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, Id);
            card.Id = Id;

            return card;

        }

        private static string GetLore(int pendulumScale, string types, IElement parserOutput)
        {

            var table = parserOutput.GetElementByClassName("card-table");
            var loreBox = table.GetElementByClassName("lore");

            if (pendulumScale == -1 || (pendulumScale >= 0 && PendulumLoreAdditionalTypesCanHaveWords.Any(word => types.ContainsIgnoreCase(word))))
            {

                //I need the new lines because of bullet points
                var descriptionUnformatted = loreBox.FirstElementChild;
                var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", "\\n"), ConstantString.HtmlTagRegex, "").Trim();
                return WebUtility.HtmlDecode(descriptionFormatted);

            }
            else
                return loreBox.TextContent.Replace("\n", "").Trim();

        }

        private static IEnumerable<IElement> GetSearchCategories(IElement parserOutput)
        {

            var firstH2Element = parserOutput.Children.FirstOrDefault(element => element.TagName.ContainsIgnoreCase("h2") && element.TextContent.ContainsIgnoreCase("search categories"));

            if (firstH2Element is null)
                return null;

            var startIndex = parserOutput.Children.Index(firstH2Element) + 1;
            var searchCategories = new List<IElement>();
            IElement currentElement;

            for (int i = startIndex; i < parserOutput.Children.Length; i++)
            {

                currentElement = parserOutput.Children[i];

                if (currentElement.ClassName?.ContainsIgnoreCase("hlist") == true)
                    searchCategories.Add(currentElement);
                else
                    break;

            }

            return searchCategories;

        }

        private async Task<string> GetCardTrivia(IElement parserOutput)
        {

            var triviaUrlElement = parserOutput
                .GetElementsByClassName("hlist")
                .FirstOrDefault(element => element.TextContent.ContainsIgnoreCase("Trivia"))?
                .FirstElementChild
                .Children
                .FirstOrDefault(element => element.TextContent.ContainsIgnoreCase("Trivia"));

            if (triviaUrlElement is not null)
            {

                var triviaLink = triviaUrlElement.GetElementByTagName("a").GetAttribute("href");

                if (!triviaLink.ContainsIgnoreCase("redlink=1"))
                {

                    var triviaUrl = string.Format(ConstantString.MediaWikiParseNameUrl, Uri.EscapeDataString(triviaLink));
                    var dom = await GetDom();
                    parserOutput = dom.GetElementByClassName("mw-parser-output");
                    var triviaElements = parserOutput?.Children
                        .Where(element => element.TagName == "UL");

                    if (triviaElements is not null)
                    {

                        var trivias = new List<string>(triviaElements.Count());

                        foreach (var triviaElement in triviaElements)
                        {

                            var html = triviaElement.InnerHtml;
                            var cleanedTrivia = Regex
                                .Replace(html, ConstantString.HtmlTagRegex, "")
                                .Replace("\n", "\\n");

                            trivias.Add(cleanedTrivia);

                        }

                        return string.Join('|', trivias);

                    }

                }

            }

            return null;

        }

        private async Task<IDocument> GetDom()
        {

            var parseResponse = await GetParseOutput();
            var parseJToken = JObject.Parse(parseResponse)["parse"];
            var html = parseJToken.Value<string>("text") ?? parseJToken["text"].Value<string>("*");

            return await Constant.HtmlParser.ParseDocumentAsync(html);

        }

    }
}
