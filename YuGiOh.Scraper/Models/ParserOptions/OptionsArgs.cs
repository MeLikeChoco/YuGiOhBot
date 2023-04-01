using System;

namespace YuGiOh.Scraper.Models.ParserOptions;

public class OptionsArgs : IOptionsArgs
{

    public string[] GetOptionsArgs()
        => Environment.GetCommandLineArgs();

}