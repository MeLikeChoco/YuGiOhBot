using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;

namespace YuGiOhScraper.Parsers.YuGiOhWikia
{
    public class CardParser
    {

        private string _name;
        private IElement _dom;

        public CardParser(string name, string link)
        {

            _name = name;
            _dom = ScraperConstants.Context.OpenAsync(link).Result.GetElementById("mw-content-text");

        }

        public Card Parse()
        {

            var table = _dom.GetElementsByClassName("cardtable").FirstOrDefault();

            if (table == null)
                throw new NullReferenceException("Missing card table");

            var card = new Card()
            {

                Name = _name,
                Img = _dom.GetElementsByClassName("cardtable-cardimage").First().FirstElementChild.GetAttribute("href")

            };

            var realName = table.GetElementsByClassName("cardtablerowdata").First().TextContent?.Trim();

            if (_name != realName)
                card.RealName = realName;

            var tableRows = table.GetElementsByClassName("cardtablerow");

            foreach (var row in tableRows)
            {

                if (row.Children.Any(element => element.ClassName == "cardtablespanrow"))
                {

                    //for future stuff
                    #region Lore
                    if (row.TextContent.ToLower().Contains("card description"))
                    {

                        var descriptionUnformatted = row.GetElementsByClassName("navbox").First()
                        .GetElementsByClassName("collapsible").First()
                        .GetElementsByTagName("tr").Last();
                        var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", "\\n"), "<[^>]*>", "").Trim();
                        card.Lore = descriptionFormatted;

                    }
                    #endregion Lore

                }
                else
                {

                    #region Card Data
                    //firstordefault because of statuses not always having a header
                    var header = row.GetElementsByClassName("cardtablerowheader").FirstOrDefault()?.TextContent;
                    var data = row.GetElementsByClassName("cardtablerowdata").First().TextContent?.Trim();

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
                        case "Passcode":
                            card.Passcode = data;
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

            var cardSearchCategories = table.GetElementsByClassName("cardtable-categories")
                .FirstOrDefault()?.Children;

            if (cardSearchCategories != null && cardSearchCategories.Any())
            {

                #region Archetypes
                var archetypeRow = cardSearchCategories.FirstOrDefault(hlist => hlist.TextContent.ToLower().Contains("archetypes"));

                if (archetypeRow != null)
                    card.Archetype = AggregateCardCategoryData(archetypeRow);
                #endregion Archetypes

                #region Supports
                var supportsRow = cardSearchCategories.FirstOrDefault(hlist => hlist.TextContent.ToLower().Contains("supports"));

                if (supportsRow != null)
                    card.Supports = AggregateCardCategoryData(supportsRow);
                #endregion Supports

                #region Anti-Supports
                var antiSupportsRow = cardSearchCategories.FirstOrDefault(hlist => hlist.TextContent.ToLower().Contains("anti-support"));

                if (antiSupportsRow != null)
                    card.AntiSupports = AggregateCardCategoryData(antiSupportsRow);
                #endregion Anti-Supports

            }

            card.Url = _dom.BaseUri;

            return card;

        }

        private string AggregateCardCategoryData(IElement row)
            => row.GetElementsByTagName("dd")
            .Select(element => element.TextContent.Trim())
            .Aggregate((current, next) => $"{current},{next}"); //using commas instead of slashes because of D/D and D/D/D

    }
}
