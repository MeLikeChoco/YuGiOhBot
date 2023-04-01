using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CommandLine;

namespace YuGiOh.Scraper.Models.ParserOptions;

public class Options
{

    private static Options _instance;
    private Config _config;
    public Config Config => _config ??= JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));

    public static Options GetInstance(IOptionsArgs optionsArgs)
    {

        if (_instance is not null)
            return _instance;

        _instance = Parser.Default
            .ParseArguments<Options>(optionsArgs.GetOptionsArgs())
            .Value;

        // var result = Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs());
        //
        // if (result is NotParsed<Options>)
        // {
        //
        //     var sentenceBuilder = SentenceBuilder.Create();
        //     var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, sentenceBuilder.FormatError, sentenceBuilder.FormatMutuallyExclusiveSetErrors, 1);
        //     var exList = errorMessages.Select(msg => new ArgumentException(msg)).ToList();
        //
        //     if (exList.Count > 0)
        //         throw new AggregateException(exList);
        //
        // }
        //
        // _instance = result.Value;

        return _instance;

    }

    [Option('m', "module", Required = true)]
    public string Module { get; set; }

    [Option('d', "dev", Default = false)]
    public bool IsDev { get; set; }

    [Option('p', "subprocess", Default = false)]
    public bool IsSubProc { get; set; }

    [Option('j', "json", Default = false)]
    public bool ShouldSaveToJson { get; set; }

    [Option('s', "sqlite", Default = false)]
    public bool ShouldSaveToSqlite { get; set; }

    [Option('c', "cards", Default = int.MaxValue)]
    public int MaxCardsToParse { get; set; }

    [Option('b', "boosters", Default = int.MaxValue)]
    public int MaxBoostersToParse { get; set; }

    [Option("anime", Default = int.MaxValue)]
    public int AnimeCardsToParse { get; set; }

    [Option("ignore_hash", Default = false)]
    public bool ShouldIgnoreHash { get; set; }

    public bool IsDebug
    {
        get
        {

            var isDebug = false;

            CheckDebug(ref isDebug);

            return isDebug;

        }
    }

    [Conditional("DEBUG")]
    private static void CheckDebug(ref bool isDebug)
        => isDebug = true;

}