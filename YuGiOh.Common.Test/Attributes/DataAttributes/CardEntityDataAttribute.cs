// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Xunit;
// using Xunit.Sdk;
// using Xunit.v3;
// using YuGiOh.Common.Models.YuGiOh;
//
// namespace YuGiOh.Common.Test.Attributes.DataAttributes;
//
// [AttributeUsage(AttributeTargets.Method)]
// public class CardEntityDataAttribute : DataAttribute
// {
//
//     private readonly string _path;
//
//     public CardEntityDataAttribute(string path)
//     {
//         _path = Path.Combine(Constants.TestDataDirectory, path);
//     }
//
//     public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
//     {
//         
//         var cards = JsonSerializer.Deserialize<IEnumerable<CardEntity>>(File.ReadAllText(_path));
//         // var cards = JsonConvert.DeserializeObject<CardEntity[]>(File.ReadAllText(_path));
//
//         
//         
//         return cards!.Select(card => new[] { card });
//         
//     }
//
//     public override bool SupportsDiscoveryEnumeration()
//     {
//         return true;
//     }
//     
// }