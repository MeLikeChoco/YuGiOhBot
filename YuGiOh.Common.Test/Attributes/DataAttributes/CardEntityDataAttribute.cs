using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit.Sdk;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.Test.Attributes.DataAttributes;

[AttributeUsage(AttributeTargets.Method)]
public class CardEntityDataAttribute : DataAttribute
{

    private readonly string _path;

    public CardEntityDataAttribute(string path)
    {
        _path = Path.Combine(Constants.TestDataDirectory, path);
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {

        var cards = JsonSerializer.Deserialize<IEnumerable<CardEntity>>(File.ReadAllText(_path));
        // var cards = JsonConvert.DeserializeObject<CardEntity[]>(File.ReadAllText(_path));

        return cards!.Select(card => new[] { card });

    }

}