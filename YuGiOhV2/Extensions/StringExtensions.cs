﻿using System;
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

    }
}
