using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using YuGiOh.Scraper.Models.ParserOptions;

namespace YuGiOh.Scraper.Constants;

public static class Constant
{

    private static HttpClient _httpClient;

    public static readonly IReadOnlyDictionary<string, string> ModuleToBaseUrl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "yugipedia", "https://yugipedia.com/" },
        { "fandom", "https://yugioh.fandom.com/" }
    };

    public static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = ConstantValue.ProcessorCount };
    public static readonly ParallelOptions SerialOptions = new() { MaxDegreeOfParallelism = 1 };
    public static readonly HtmlParser HtmlParser = new();
    
    public static HttpClient GetHttpClient(Options options)
        => _httpClient ??= new HttpClient { BaseAddress = new Uri(ModuleToBaseUrl[options.Module]) };

}