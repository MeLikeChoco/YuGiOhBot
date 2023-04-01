using YuGiOh.Scraper.Models.ParserOptions;

namespace YuGiOh.Scraper.Test.Parsers.Fandom;

public class FandomOptionsArgs : IOptionsArgs
{

    public string[] GetOptionsArgs()
        => new[] { "fandom" };

}