using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace YuGiOh.Scraper
{
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

        public Config Config
        {

            get
            {

                if (_config is null)
                {

                    using var file = File.OpenText("config.json");
                    using var reader = new JsonTextReader(file);

                    _config = JsonSerializer.CreateDefault().Deserialize<Config>(reader);

                    reader.Close();
                    file.Close();

                }

                return _config;

            }

        }

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

    }
}
