using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;
using YuGiOhScraper.Parsers;

namespace YuGiOhScraper.Modules
{
    public abstract class ModuleBase
    {

        public string ModuleName { get; set; }
        public string DatabaseName { get; set; }

        protected string SqliteName => DatabaseName + ".db";
        protected string SqlitePath => SqliteName;
        protected static string InMemoryConnectionString => $"Data Source = :memory:";
        protected string FlatFileConnectionString => $"Data Source = \"{SqlitePath}\"";
        protected string JsonFileName => DatabaseName + ".json";
        protected string JsonFilePath => JsonFileName;
        protected HttpClient HttpClient { get; private set; } //probably not a good idea if it starts scaling, but eh, doubt I'll get port exhaustion, better for parallel function anyway
        protected IDictionary<string, string> TcgCards, OcgCards, TcgBoosters, OcgBoosters;

        public ModuleBase(string moduleName, string databaseName, string baseAddress)
        {

            ModuleName = moduleName;
            DatabaseName = databaseName;
            HttpClient = new HttpClient(new HttpClientHandler() { MaxConnectionsPerServer = Environment.ProcessorCount }) { BaseAddress = new Uri(baseAddress) };

        }

        public async Task RunAsync()
        {

            Console.WriteLine($"{ModuleName} module has began running...");

            var cardLinks = await GetCardLinks();
            var boosterPackLinks = await GetBoosterPackLinks();

            Console.WriteLine($"There are {cardLinks.Count} cards and {boosterPackLinks.Count} booster packs.");

            await DoBeforeParse(cardLinks, boosterPackLinks);

            var cards = ParseCards(cardLinks, out var errors);
            var boosterPacks = ParseBoosterPacks(boosterPackLinks, out var errors2);
            errors = errors.Concat(errors2);

            await DoBeforeSaveToDb(cards, boosterPacks, errors);

            if (Settings.Sqlite)
                await SaveToSqliteDb(cards, boosterPacks, errors);

            if (Settings.Json)
                await SaveToJsonFile(cards, boosterPacks, errors);

            //using (var memoryDb = new SqliteConnection(InMemoryConnectionString))
            //using (var flatFileDb = new SqliteConnection(FlatFileConnectionString))
            //{

            //    await memoryDb.OpenAsync();
            //    await flatFileDb.OpenAsync();
            //    await SaveToDatabase(cards, boosterPacks, errors, memoryDb, flatFileDb);
            //    memoryDb.Close();
            //    flatFileDb.Close();

            //}

            Console.WriteLine($"{ModuleName} module has finished running.");

        }

        protected abstract Task<IDictionary<string, string>> GetCardLinks();
        protected abstract Task<IDictionary<string, string>> GetBoosterPackLinks();
        protected abstract Task DoBeforeParse(IDictionary<string, string> cardLinks, IDictionary<string, string> boosterPackLinks);
        protected abstract IEnumerable<Card> ParseCards(IDictionary<string, string> cardLinks, out IEnumerable<Error> errors);
        protected abstract IEnumerable<BoosterPack> ParseBoosterPacks(IDictionary<string, string> boosterPackLinks, out IEnumerable<Error> errors);
        protected abstract Task DoBeforeSaveToDb(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors);

        //protected virtual async Task<(IEnumerable<T> result, IEnumerable<Error> errors)> DoWork<T>(Dictionary<string, string> links)
        //{

        //    var limit = Environment.ProcessorCount;
        //    var type = typeof(T).Name.ToLower() + "s";
        //    string retry = "";
        //    IEnumerable<T> results = new List<T>();
        //    IEnumerable<Error> errors = new List<Error>();

        //    do
        //    {

        //        Console.WriteLine($"Getting {type}...");

        //        foreach (var batch in links.Batch(limit))
        //        {

        //            var parsers = new List<Task>(batch.Select(kv => (Activator.CreateInstance(typeof(T), kv.Key, kv.Value) as IParser<T>).ParseAsync()));

        //            await Task.WhenAll(parsers).ContinueWith(task => errors.Concat(;

        //        }

        //        results = results.Concat(GetCards(cardLinks, out errors));
        //        links = errors.ToDictionary(error => error.Name, error => error.Url);
        //        //var cards = GetCards(httpClient, links.ToList().GetRange(0, 100).ToDictionary(kv => kv.Key, kv => kv.Value));

        //        if (links.Any() && !Settings.IsSubProcess)
        //        {

        //            Console.WriteLine($"There were {errors.Count()} errors. Retry? (y/n): ");
        //            retry = Console.ReadLine();

        //        }
        //        else
        //            Console.WriteLine($"Finished getting {type}.");

        //    } while (links.Any() && retry == "y" && !Settings.IsSubProcess);

        //    return (results, errors);

        //}

        protected virtual async Task SaveToSqliteDb(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {

            Exception exception = null;

            do
            {

                try
                {

                    if (File.Exists(SqlitePath))
                        File.Delete(SqlitePath);

                    using (var memoryDb = new SqliteConnection(InMemoryConnectionString))
                    using (var flatFileDb = new SqliteConnection(FlatFileConnectionString))
                    {

                        await memoryDb.OpenAsync();

                        SqliteCommand createCardTable, createboosterPackTable, createErrorTable;
                        createCardTable = createboosterPackTable = createErrorTable = null;

                        createCardTable = memoryDb.CreateCommand();
                        createboosterPackTable = memoryDb.CreateCommand();
                        createErrorTable = memoryDb.CreateCommand();

                        createCardTable.CommandText = ScraperConstants.CreateCardTableSql;
                        createboosterPackTable.CommandText = ScraperConstants.CreateBoosterPackTableSql;
                        createErrorTable.CommandText = ScraperConstants.CreateErrorTable;

                        Console.WriteLine($"Saving to {SqliteName}...");
                        await createCardTable.ExecuteNonQueryAsync();
                        await createboosterPackTable.ExecuteNonQueryAsync();
                        await createErrorTable.ExecuteNonQueryAsync();

                        await memoryDb.InsertAsync(cards);
                        await memoryDb.InsertAsync(boosterPacks);
                        await memoryDb.InsertAsync(errors);

                        memoryDb.BackupDatabase(flatFileDb);

                        Console.WriteLine($"Finished saving to {SqliteName}.");

                        memoryDb.Close();

                    }

                    exception = null;

                }
                catch (IOException ioexception)
                {

                    exception = ioexception;

                    Console.WriteLine($"IOException occured. Most likely cause is {SqliteName} being held open by another program. Hit enter when resolved...");

                    while (Console.ReadKey().Key != ConsoleKey.Enter) { }

                }

            } while (exception != null);

        }

        protected virtual Task SaveToJsonFile(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors)
        {

            Console.WriteLine($"Saving to {JsonFileName}...");

            Exception exception;

            do
            {

                try
                {

                    if (File.Exists(JsonFilePath))
                        File.Delete(JsonFilePath);

                    var json = new
                    {

                        Cards = cards,
                        BoosterPack = boosterPacks,
                        Errors = errors

                    };

                    return File.WriteAllTextAsync(JsonFilePath, JsonConvert.SerializeObject(json, Formatting.Indented)).ContinueWith((task) => Console.WriteLine($"Finished writing to {JsonFileName}."));

                }
                catch (IOException ioexception)
                {

                    exception = ioexception;

                    Console.WriteLine($"IOException occured. Most likely cause is {JsonFileName} being held open by another program. Hit enter when resolved...");

                    while (Console.ReadKey().Key != ConsoleKey.Enter) { }

                }

            } while (exception != null);

            return Task.CompletedTask;

        }
        //protected abstract Task SaveToDatabase(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors, SqliteConnection memoryDb, SqliteConnection flatFileDb);

        //private  object _inlineLock = new object();
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void InlineWrite(string message)
        {

            //lock (_inlineLock)
            //{

            //    int currentLineCursor = Console.CursorTop;
            //    Console.SetCursorPosition(0, Console.CursorTop);
            //    Console.Write(new string(' ', Console.WindowWidth));
            //    Console.SetCursorPosition(0, currentLineCursor);
            //    //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            //    Console.Write(message);

            //}

            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            Console.Write(message);


        }

    }
}
