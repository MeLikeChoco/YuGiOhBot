using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.Test.Attributes.DataAttributes;

public class InputCardEntityDataAttribute : DataAttribute
{

    private readonly string _path;

    public InputCardEntityDataAttribute(string path)
    {
        _path = Path.Combine(Constants.TestDataDirectory, path);
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {

        var json = File.ReadAllText(_path);
        var dataArray = JArray.Parse(json);

        return dataArray.Select(data => new object[] { data["Input"]!.ToString(), data["Card"]!.ToObject<CardEntity>() ?? throw new JsonSerializationException() });

    }

}