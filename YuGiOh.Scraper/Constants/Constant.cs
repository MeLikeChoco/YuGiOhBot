using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace YuGiOh.Scraper.Constants;

public static class Constant
{

    public static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = ConstantValue.ProcessorCount };
    public static readonly ParallelOptions SerialOptions = new() { MaxDegreeOfParallelism = 1 };
    public static readonly HttpClient HttpClient = new() { BaseAddress = new Uri(ConstantString.YugipediaUrl) };
    public static readonly HtmlParser HtmlParser = new();

}