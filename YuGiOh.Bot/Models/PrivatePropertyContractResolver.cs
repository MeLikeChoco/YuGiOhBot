using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace YuGiOh.Bot.Models
{
    public class PrivatePropertyContractResolver : DefaultContractResolver
    {

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {

            var property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable)
            {

                var propertyInfo = member as PropertyInfo;

                if (propertyInfo is not null)
                {

                    var hasPrivateSetter = propertyInfo.GetSetMethod(true) is not null;
                    property.Writable = hasPrivateSetter;

                }
            }

            return property;

        }

    }
}