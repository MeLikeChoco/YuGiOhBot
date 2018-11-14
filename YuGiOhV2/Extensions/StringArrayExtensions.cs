using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Extensions
{
    public static class StringArrayExtensions
    {

        public static string ToString(this string[] array, char seperator)
            => array.Join(seperator);

        public static string ToString(this string[] array, string seperator)
            => array.Join(seperator);

    }
}
