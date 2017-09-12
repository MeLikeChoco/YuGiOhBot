using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Extensions
{
    public static class StringExtensions
    {

        public static string Title(this string str)
        {

            return char.ToUpper(str.First()) + str.Substring(1).ToLower();

        }

    }
}
