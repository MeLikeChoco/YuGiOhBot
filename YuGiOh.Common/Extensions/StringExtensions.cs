using System;

namespace YuGiOh.Common.Extensions;

public static class StringExtensions
{

    public static bool ContainsIgnoreCase(this string str, string value)
        => str.Contains(value, StringComparison.OrdinalIgnoreCase);

}