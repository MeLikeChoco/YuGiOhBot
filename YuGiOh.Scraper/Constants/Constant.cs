using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace YuGiOh.Scraper.Constants;

public static class Constant
{

    private static HttpClient _httpClient;

    public static HttpClient HttpClient
    {

        get
        {

            if (_httpClient is not null)
                return _httpClient;

            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "YuGiOh discord bot created by melikechoco");

            return _httpClient;

        }

    }

    public static readonly IReadOnlyDictionary<string, string> ModuleToBaseUrl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ConstantString.YugipediaModuleName, ConstantString.YugipediaUrl },
        { ConstantString.FandomModuleName, ConstantString.YuGiOhFandomUrl }
    };

    public static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = ConstantValue.ProcessorCount };
    public static readonly ParallelOptions SerialOptions = new() { MaxDegreeOfParallelism = 1 };
    public static readonly HtmlParser HtmlParser = new();

}