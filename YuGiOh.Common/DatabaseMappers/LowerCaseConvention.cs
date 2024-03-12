using System.Reflection;
using Dommel;

namespace YuGiOh.Common.DatabaseMappers;

public class LowerCaseConvention : IColumnNameResolver
{
        
    public static readonly LowerCaseConvention Instance = new LowerCaseConvention();

    public string ResolveColumnName(PropertyInfo propertyInfo)
        => propertyInfo.Name.ToLower();

}