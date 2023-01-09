using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.TypeReaders;

public static class TypeReaderUtils
{

    public static string SanitizeInput(string input)
        => SanitizeMentions(
            input
                .ConvertTypesetterToTypewriter()
                .Trim()
        );

    public static string SanitizeMentions(string input)
        => input
            .Replace("@everyone", "\\@everyone")
            .Replace("@here", "\\@here");

}