using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

                if (propertyInfo != null)
                {

                    var hasPrivateSetter = propertyInfo.GetSetMethod(true) != null;
                    property.Writable = hasPrivateSetter;

                }
            }

            return property;

        }

    }
}
