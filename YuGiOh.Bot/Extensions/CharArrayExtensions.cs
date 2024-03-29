﻿using System.Collections.Generic;
using System.Linq;

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