using System;
using System.Collections.Generic;
using System.Text;

namespace YuGiOhScraper.Extensions
{
    public static class StringExtensions
    {


        /// <summary>
        /// Uses string.Contains with StringComparison.OrdinalIgnoreCase
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns>bool</returns>
        public static bool ContainsIgnoreCase(this string str, string value)
            => str.Contains(value, StringComparison.OrdinalIgnoreCase);

    }
}
