using System;
using System.Linq;

namespace YuGiOh.Bot.Extensions
{
    public static class StringExtensions
    {

        public enum IgnoreCaseComparison
        {

            OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase,
            InvariantCultureIgnoreCase = StringComparison.InvariantCultureIgnoreCase,
            CurrentCultureIgnoreCase = StringComparison.CurrentCultureIgnoreCase

        }

        public static bool ContainsIgnoreCase(this string str, string value, IgnoreCaseComparison comparisonType = IgnoreCaseComparison.OrdinalIgnoreCase)
            => str.Contains(value, (StringComparison)comparisonType);

        public static bool EqualsIgnoreCase(this string str, string value, IgnoreCaseComparison comparisonType = IgnoreCaseComparison.OrdinalIgnoreCase)
            => str.Equals(value, (StringComparison)comparisonType);

        public static string Title(this string str)
        {

            return char.ToUpper(str.First()) + str.Substring(1).ToLower();

        }

        //from https://stackoverflow.com/questions/17710561/parse-very-long-date-format-to-datetime-in-c-sharp
        public static string StripDateOrdinals(this string input)
        {

            return input.Replace("0th", "0")
                .Replace("1st", "1")
                .Replace("2nd", "2")
                .Replace("3rd", "3")
                .Replace("11th", "11") // Need to handle these separately...
                .Replace("12th", "12")
                .Replace("13th", "13")
                .Replace("4th", "4")
                .Replace("5th", "5")
                .Replace("6th", "6")
                .Replace("7th", "7")
                .Replace("8th", "8")
                .Replace("9th", "9");

        }

        /// <summary>
        /// Replace Typesetter apostraphe and quotation marks to Typewriter
        /// </summary>
        /// <param name="input"></param>
        public static string ConvertTypesetterToTypewriter(this string input)
        {

            return input
                .Replace('‘', '\'') //‘ \u2018
                .Replace('’', '\'') //’ \u2019
                .Replace('“', '\"') //“ \u201c
                .Replace('”', '\"'); //” \u201d

        }

    }
}
