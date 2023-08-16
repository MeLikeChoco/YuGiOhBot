// using MoreLinq;
// using Newtonsoft.Json.Linq;
// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using YuGiOh.Bot.Extensions;
//
// namespace YuGiOh.Bot.Models.BoosterPacks
// {
//     public class BoosterPackParser
//     {
//
//         public string Name { get; set; }
//         public string Dates { get; set; }
//         public string Cards { get; set; }
//         public string Url { get; set; }
//
//         public BoosterPack Parse()
//         {
//
//             var releaseDates = new Dictionary<string, DateTime>();
//
//             if (!string.IsNullOrEmpty(Dates))
//             {
//
//                 var releaseDateList = JObject.Parse(Dates).Children();
//
//                 foreach (var date in releaseDateList)
//                 {
//
//                     var property = date.ToObject<JProperty>();
//                     var name = property.Name;
//                     var dateString = property.Value.ToString().StripDateOrdinals();
//
//                     try
//                     {
//                         releaseDates[name] = DateTime.ParseExact(dateString, "MMMM d, yyyy", new CultureInfo("en-US"));
//                     }
//                     catch (FormatException)
//                     {
//                         try
//                         {
//                             releaseDates[name] = DateTime.ParseExact(dateString, "MMMM, yyyy", new CultureInfo("en-US"));
//                         }
//                         catch (FormatException)
//                         {
//                             try
//                             {
//                                 releaseDates[name] = DateTime.ParseExact(dateString, "MMMM yyyy", new CultureInfo("en-US"));
//                             }
//                             catch (FormatException)
//                             {
//                                 releaseDates[name] = DateTime.ParseExact(dateString, "yyyy", new CultureInfo("en-US"));
//                             }
//                         }
//                     }
//
//                 }
//
//             }
//
//             var cards = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
//
//             foreach (var cardObject in JArray.Parse(Cards).Children())
//             {
//
//                 var cardName = cardObject.Value<string>("Name");
//                 var rarities = cardObject.Value<JArray>("Rarities").ToObject<string[]>();
//
//                 foreach (var rarity in rarities)
//                 {
//
//                     cards.TryAdd(rarity, new List<string>());
//                     cards[rarity].Add(cardName);
//
//                 }
//
//                 //var rarities = cardObject.Value<string[]>("Rarities").Select(rarity =>
//                 //{
//
//                 //    typeof(Rarity).
//
//                 //});
//
//                 //foreach (var rarity in rarities)
//                 //{
//
//                 //    cards.TryAdd(rarity, new List<string>());
//                 //    cards[rarity].Add(cardName);
//
//                 //}
//
//             }
//
//             cards.ForEach(kv => kv.Value.Sort());
//
//             return new BoosterPack(Name, releaseDates, cards, Url);
//
//         }
//
//     }
// }

