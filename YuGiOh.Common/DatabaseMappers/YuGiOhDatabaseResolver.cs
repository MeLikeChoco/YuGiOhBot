using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.FluentMap.Conventions;
using Dommel;

namespace YuGiOh.Common.DatabaseMappers
{
    public class YuGiOhDatabaseResolver : Convention, IColumnNameResolver, IKeyPropertyResolver
    {

        public YuGiOhDatabaseResolver()
        {

            Properties()
                .Configure(config => config.Transform(propName => propName.ToLower()));

        }

        public string ResolveColumnName(PropertyInfo propertyInfo)
            => propertyInfo.Name.ToLower();

        public ColumnPropertyInfo[] ResolveKeyProperties(Type type)
        {

            var idProp = type.GetProperties().First(propInfo => propInfo.Name.Equals("id", StringComparison.OrdinalIgnoreCase));

            return new ColumnPropertyInfo[] { new ColumnPropertyInfo(idProp, DatabaseGeneratedOption.None) };

        }

    }
}
