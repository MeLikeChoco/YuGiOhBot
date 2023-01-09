using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.TypeReaders;

public static class TypeReaderUtils
{

    public static string SanitizeInput(string input)
        => input
            .ConvertTypesetterToTypewriter()
            .Replace("@everyone", "\\@everyone")
            .Replace("@here", "\\@here")
            .Trim();

}