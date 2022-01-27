using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Extensions
{
    public static class CharArrayExtensions
    {

        public static string ToString(this char[] array)
            => new(array);

        public static string ToString(this IEnumerable<char> array)
            => ToString(array.ToArray());

    }
}
