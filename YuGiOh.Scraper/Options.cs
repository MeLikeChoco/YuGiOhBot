using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CommandLine;

namespace YuGiOh.Scraper;

public class Options
{

    private static Options _instance;

    public static Options Instance
    {

        get
        {

            if (_instance is null)
            {
                _ = Parser.Default
                    .ParseArguments<Options>(Environment.GetCommandLineArgs())
                    .WithParsed((opts) => _instance = opts);
            }

            return _instance;

        }

    }

    private Config _config;

    public Config Config => _config ??= JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));

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