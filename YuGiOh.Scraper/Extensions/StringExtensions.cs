using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Scraper.Extensions
{
    public static class StringExtensions
    {

        public static bool ContainsIgnoreCase(this string str, string value)
            => str.Contains(value, StringComparison.OrdinalIgnoreCase);

    }
}
