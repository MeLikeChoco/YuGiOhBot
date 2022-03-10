using System;

namespace YuGiOh.Common.Test.Extensions;

public static class StringExtensions
{

    public static bool EqualsIgnoreCase(this string str1, string str2)
        => str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

}