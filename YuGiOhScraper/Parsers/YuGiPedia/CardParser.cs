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
                        
            var dom = GetDom(httpClient).GetElementByClassName("mw-parser-output");
            //var dom = response.GetElementById("mw-content-text").FirstElementChild;
            var table = dom.GetElementByClassName("cardtable")?.FirstElementChild;

            if (table == null)
                throw new NullReferenceException("Missing card table");

            var card = new Card()
            {

                Name = Name,
                Img = dom.GetElementByClassName("cardtable-cardimage").GetElementsByTagName("img").First().GetAttribute("srcset")?.Split(' ').ElementAtOrDefault(2)
                ?? dom.GetElementByClassName("cardtable-cardimage").GetElementsByTagName("img").First().GetAttribute("src")

            };

            var realName = table.FirstElementChild.TextContent?.Trim();

            if (Name != realName)
                card.RealName = realName;

            var tableRows = table.GetElementsByClassName("cardtablerow");

            foreach (var row in tableRows)
            {

                if (row.FirstElementChild.ClassName == "cardtablespanrow")
                {

                    //for future stuff
                    #region Lore
                    if (card.PendulumScale == -1 && row.FirstElementChild.FirstElementChild.TagName == "P")
                    {

                        var descriptionUnformatted = row.FirstElementChild;
                        var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", "\\n"), "<[^>]*>", "").Trim();
                        card.Lore = WebUtility.HtmlDecode(descriptionFormatted);

                    }
                    else if (card.PendulumScale > -1 && row.TextContent.Contains("effect", StringComparison.InvariantCultureIgnoreCase))
                        card.Lore = row.TextContent.Replace("\n", " ").Trim();
                    #endregion Lore

                }
                else
                {

                    #region Card Data
                    //firstordefault because of statuses not always having a header
                    var header = row.GetElementByClassName("cardtablerowheader")?.TextContent;
                    var data = row.GetElementByClassName("cardtablerowdata").TextContent?.Trim();

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
                        case "Statuses":
                            var statuses = tableRows.Where(element => !element.Children.Any(child => child.ClassName == "cardtablerowheader" || child.ClassName == "cardtablespanrow") &&
                                element.Children.Length == 1).Select(element => element.FirstElementChild);
                            var count = statuses.Count();
                            var index = data.IndexOf('(');

                            if (count > 0)
                                card.OcgStatus = data.Substring(0, index).TrimEnd();
                            else
                                card.OcgStatus = data;

                            if (count == 2)
                            {

                                card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
                                card.TcgTrnStatus = statuses.ElementAt(1).FirstElementChild.TextContent;

                            }
                            else if (count == 1)
                            {

                                card.TcgAdvStatus = statuses.First().FirstElementChild.TextContent;
                                card.TcgTrnStatus = card.TcgAdvStatus;

                            }
                            else
                            {

                                card.TcgAdvStatus = card.OcgStatus;
                                card.TcgTrnStatus = card.OcgStatus;

                            }

                            break;
                        default:
                            break;

                    }
                    #endregion Card Data

                }

            }

            #region Search Categories
            var searchCategories = GetSearchCategories(dom);

            if (searchCategories != null && searchCategories.Any())
            {

                foreach (var searchCategory in searchCategories)
                {

                    var header = searchCategory.GetElementsByTagName("dt").First().TextContent;
                    var value = searchCategory.GetElementsByTagName("dd")
                        .AsEnumerable()
                        .Where(element => element.TextContent != null)
                        .Select(element => element.TextContent);

                    if (header.StartsWith("archetype", StringComparison.InvariantCultureIgnoreCase))
                        card.Archetype = string.Join(',', value);
                    else if (header.Contains("anti-supports", StringComparison.InvariantCultureIgnoreCase))
                        card.AntiSupports = string.Join(',', value);
                    else if (header.Contains("supports", StringComparison.InvariantCultureIgnoreCase))
                        card.Supports = string.Join(',', value);

                }

            }
            #endregion Search Categories

            return card;

        }

        private IEnumerable<IElement> GetSearchCategories(IElement dom)
        {

            var firstH2Element = dom.Children.First(element => element.TagName == "H2");
            var firstH2Index = dom.Children.Index(firstH2Element);
            var startIndex = firstH2Index + 1;
            var searchCategories = new List<IElement>();
            IElement currentElement;

            for (int i = startIndex; i < dom.Children.Length; i++)
            {

                currentElement = dom.Children[i];

                if (dom.Children[i].ClassName == "hlist")
                    searchCategories.Add(currentElement);
                else
                    break;

            }

            return searchCategories;

        }

    }
}
