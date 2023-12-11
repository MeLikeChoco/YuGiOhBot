using System.Text.RegularExpressions;

namespace YuGiOh.Scraper.Constants;

public static partial class ConstantRegex
{

    [GeneratedRegex("<(?!br[\x20/>])[^<>]+>")]
    public static partial Regex HtmlNewLine();

}