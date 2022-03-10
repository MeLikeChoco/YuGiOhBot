using System;
using System.Text;

namespace YuGiOh.Bot.Extensions
{
    public static class TimespanExtensions
    {

        public static string ToPrettyString(this TimeSpan timespan, bool displayZeroes = false)
        {

            var strBuilder = new StringBuilder();

            if (timespan.Days > 0 || displayZeroes)
                strBuilder.Append(timespan.Days).Append(" day(s), ");

            if (timespan.Hours > 0 || displayZeroes)
                strBuilder.Append(timespan.Hours).Append(" hour(s), ");

            if (timespan.Minutes > 0 || displayZeroes)
                strBuilder.Append(timespan.Minutes).Append(" minute(s), ");

            if (timespan.Seconds > 0 || displayZeroes)
                strBuilder.Append(timespan.Seconds).Append(" second(s), ");

            if (timespan.Milliseconds > 0 || displayZeroes)
                strBuilder.Append(timespan.Milliseconds).Append(" millisecond(s)");

            var prettyStr = strBuilder.ToString();

            if (prettyStr.EndsWith(", "))
                prettyStr = prettyStr[0..^2];

            return prettyStr;

        }

    }

}
