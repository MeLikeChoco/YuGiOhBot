using System.Reflection;
using Dapper.FluentMap.Conventions;
using Dommel;

namespace YuGiOh.Common.DatabaseMappers
{
    public class LowerCaseConvention : Convention, IColumnNameResolver
    {

        public LowerCaseConvention()
        {

            Properties()
                .Configure(config => config.Transform(propName => propName.ToLower()));

        }

        public string ResolveColumnName(PropertyInfo propertyInfo)
            => propertyInfo.Name.ToLower();

    }
}