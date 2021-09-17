using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Extensions
{
    public static class StringJoinExtensions
    {

        public static string Join(this string[] strArray, char separator)
            => string.Join(separator, strArray);

        public static string Join(this string[] strArray, string separator)
            => string.Join(separator, strArray);

        public static string Join(this IEnumerable<string> strEnumerable, char separator)
            => string.Join(separator, strEnumerable);

        public static string Join(this IEnumerable<string> strEnumerable, string separator)
            => string.Join(separator, strEnumerable);

        public static string Join(this char[] strArray, char separator)
            => string.Join(separator, strArray);

        public static string Join(this char[] strArray, string separator)
            => string.Join(separator, strArray);

        public static string Join(this IEnumerable<char> strEnumerable, char separator)
            => string.Join(separator, strEnumerable);

        public static string Join(this IEnumerable<char> strEnumerable, string separator)
            => string.Join(separator, strEnumerable);

    }
}
