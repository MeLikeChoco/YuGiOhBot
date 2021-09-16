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
