using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;
using YuGiOhScraper.Extensions;

namespace YuGiOhScraper.Parsers.YuGiPedia
{
    public class CardParser : MediaWikiParser<Card>
    {

        public CardParser(string name, string url)
            : base(name, url) { }

        public override Card Parse(HttpClient httpClient)
        {

            var dom = GetDom(httpClient);
            var parserOutput = dom.GetElementByClassName("mw-parser-output");
            //var html = dom.Html(); //for debugging purposes
            var table = parserOutput.GetElementByClassName("card-table");

            if (table == null)
                throw new NullReferenceException("Missing card table");

            var card = new Card()
            {

                Name = Name,
                Img = parserOutput.GetElementByClassName("cardtable-main_image-wrapper").GetElementsByTagName("img").First().GetAttribute("srcset")?.Split(' ').ElementAtOrDefault(2)
                ?? parserOutput.GetElementByClassName("cardtable-main_image-wrapper").GetElementsByTagName("img").First().GetAttribute("src")

            };

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
                        var count = statuses.Count();

                        foreach (var element in statuses)
                        {

                            var rawStatus = element.TextContent;
                            var status = rawStatus.Trim();
                            var index = status.IndexOf('(');

                            if (index != -1)
                                status = status.Substring(0, index).Trim();

                            if (rawStatus.Contains("ocg", StringComparison.OrdinalIgnoreCase))
                                card.OcgStatus = status;
                            else if (rawStatus.Contains("adv", StringComparison.OrdinalIgnoreCase))
                                card.TcgAdvStatus = status;
                            else if (rawStatus.Contains("trad", StringComparison.OrdinalIgnoreCase))
                                card.TcgTrnStatus = status;
                            else if (rawStatus.Contains("tcg", StringComparison.OrdinalIgnoreCase))
                                card.TcgAdvStatus = status;

                        }

                        //for (int i = 0; i < count; i++)
                        //{

                        //    var status = statuses[i].TextContent.Trim();
                        //    var index = status.IndexOf('(');

                        //    if (index != -1)
                        //        status = status.Substring(0, index).Trim();



                        //    if (i == 0)
                        //        card.OcgStatus = status;
                        //    else if (i == 1)
                        //        card.TcgAdvStatus = status;
                        //    else if (i == 2)
                        //        card.TcgTrnStatus = status;

                        //}

                        //if (count > 0)
                        //    card.OcgStatus = data.Substring(0, index).TrimEnd();
                        //else
                        //    card.OcgStatus = data;

                        //if (count == 2)
                        //{

                        //    card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
                        //    card.TcgTrnStatus = statuses.ElementAt(1).FirstElementChild.TextContent;

                        //}
                        //else if (count == 1)
                        //{

                        //    card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
                        //    card.TcgTrnStatus = card.TcgAdvStatus;

                        //}
                        //else
                        //{

                        //    card.TcgAdvStatus = card.OcgStatus;
                        //    card.TcgTrnStatus = card.OcgStatus;

                        //}

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

            #region Lore
            var loreBox = table.GetElementByClassName("lore");

            if (card.PendulumScale == -1 ||
                (card.PendulumScale >= 0 && (card.Types.ContainsIgnoreCase("Synchro") || card.Types.ContainsIgnoreCase("Fusion") || card.Types.ContainsIgnoreCase("Xyz"))))
            {

                //I need the new lines because of bullet points
                var descriptionUnformatted = loreBox.FirstElementChild;
                var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", "\\n"), ScraperConstants.HtmlTagRegex, "").Trim();
                card.Lore = WebUtility.HtmlDecode(descriptionFormatted);

            }
            else
                card.Lore = loreBox.TextContent.Replace("\n", "").Trim();
            #endregion Lore

            #region Search Categories
            var searchCategories = GetSearchCategories(parserOutput);

            if (searchCategories?.Any() == true)
            {

                foreach (var searchCategory in searchCategories)
                {

                    var header = searchCategory.GetElementsByTagName("dt").First().TextContent;
                    var value = searchCategory.GetElementsByTagName("dd")
                        .Where(element => !string.IsNullOrEmpty(element.TextContent))
                        .Select(element => element.TextContent.Trim());

                    if (header.ContainsIgnoreCase("archetype") && header.ContainsIgnoreCase("series") && !header.ContainsIgnoreCase("related")) //filter out "related archetypes and series"
                        card.Archetype = string.Join(',', value);
                    else if (header.ContainsIgnoreCase("anti-supports"))
                        card.AntiSupports = string.Join(',', value);
                    else if (header.ContainsIgnoreCase("supports") && !header.ContainsIgnoreCase("archetypes"))
                        card.Supports = string.Join(',', value);

                }

            }
            #endregion Search Categories

            #region Card Trivia
            //i have to do it this way because i can't figure out why anglesharp is fetching the wrong index when i use the table as reference
            //var triviaUrlElement = parserOutput.Children
            //    .FirstOrDefault(element => element.TextContent.ContainsIgnoreCase("Trivia"))?
            //    .FirstElementChild
            //    .Children
            //    .FirstOrDefault(element => element.TextContent.ContainsIgnoreCase("Trivia"));

            //if (triviaUrlElement != null)
            //{

            //    var title = triviaUrlElement.FirstElementChild.GetAttribute("href");

            //    //lets hope konami never releases a card called "page does not exist"
            //    //we could also check if href ends with "redlink=1" if checking by title is problematic
            //    if (!title.ContainsIgnoreCase("redlink=1"))
            //    {

            //        var triviaUrl = ScraperConstants.YuGiPediaUrl + ScraperConstants.MediaWikiParseNameUrl + Uri.EscapeDataString(title);
            //        dom = GetDom(httpClient, triviaUrl);
            //        parserOutput = dom.GetElementByClassName("mw-parser-output");
            //        var triviaElements = parserOutput?.Children
            //            .Where(element => element.TagName == "UL");

            //        if (triviaElements != null)
            //        {

            //            var trivias = new List<string>(triviaElements.Count());

            //            foreach (var triviaElement in triviaElements)
            //            {

            //                var html = triviaElement.InnerHtml;
            //                var cleanedTrivia = Regex
            //                    .Replace(html, ScraperConstants.HtmlTagRegex, "")
            //                    .Replace("\n", "\\n");

            //                trivias.Add(cleanedTrivia);

            //            }

            //            card.CardTrivia = string.Join('|', trivias);

            //        }

            //    }

            //}
            #endregion Card Trivia

            return card;

        }

        private IEnumerable<IElement> GetSearchCategories(IElement dom)
        {

            var firstH2Element = dom.Children.FirstOrDefault(element => element.TagName.ContainsIgnoreCase("h2") && element.TextContent.ContainsIgnoreCase("search categories"));

            if (firstH2Element == null)
                return null;

            var startIndex = dom.Children.Index(firstH2Element) + 1;
            var searchCategories = new List<IElement>();
            IElement currentElement;

            for (int i = startIndex; i < dom.Children.Length; i++)
            {

                currentElement = dom.Children[i];

                if (currentElement.ClassName?.Contains("hlist", StringComparison.OrdinalIgnoreCase) == true)
                    searchCategories.Add(currentElement);
                else
                    break;

            }

            return searchCategories;

        }

    }
}
