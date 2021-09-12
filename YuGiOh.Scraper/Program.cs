using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Common.Repositories;
using YuGiOh.Common.Services;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Models.Parsers;
using YuGiOh.Scraper.Models.Responses;

namespace YuGiOh.Scraper
{
    public class Program : IYuGiOhRepositoryConfiguration
    {

        private static readonly Options Options = Options.Instance;
        private ParallelOptions _parallelOptions => Options.IsDev ? Constant.SerialOptions : Constant.ParallelOptions;

        public static Task Main()
            => new Program().Run();

        public async Task Run()
        {

            Log("Getting TCG cards.");

            var tcgLinks = await GetLinks(ConstantString.MediaWikiTcgCards);

            Log("Getting OCG cards.");

            var ocgLinks = await GetLinks(ConstantString.MediaWikiOcgCards);

            Log("Processing cards.");

            var cardProcResponse = ProcessCards(tcgLinks, ocgLinks);

            Log($"Processed {cardProcResponse.Count} cards. There were {cardProcResponse.Errors.Count} errors.");
            Log("Getting TCG booster packs.");

            tcgLinks = await GetLinks(ConstantString.MediaWikiTcgPacks);

            Log("Getting OCG booster packs.");

            ocgLinks = await GetLinks(ConstantString.MediaWikiOcgPacks);

            Log($"Processing boosterpacks.");

            var boosterProcResponse = ProcessBoosters(tcgLinks, ocgLinks);

            Log($"Processed {boosterProcResponse.Count} booster packs. There were {boosterProcResponse.Errors.Count} errors.");
            Log($"Processing errors.");

            var errors = cardProcResponse.Errors.Concat(boosterProcResponse.Errors);

            ProcessErrors(errors);

            Log($"Processed {errors.Count()} errors.");

        }

        public CardProcessorResponse ProcessCards(IDictionary<string, string> tcgLinks, IDictionary<string, string> ocgLinks)
        {

            var nameToLinks = tcgLinks
                .Concat(ocgLinks.Where(kv => !tcgLinks.ContainsKey(kv.Key)));
            var size = nameToLinks.Count();

            if (Options.MaxCardsToParse <= size)
                nameToLinks = nameToLinks.RandomSubset(Options.MaxCardsToParse);
            //nameToLinks = nameToLinks.Take(Options.MaxCardsToParse);

            size = nameToLinks.Count();
            var errors = new ConcurrentBag<Error>();
            var repo = new YuGiOhRepository(this);
            var count = 0;
            var countLock = new object();

            Parallel.ForEach(nameToLinks, _parallelOptions, (nameToLink, _) =>
            {

                var retryCount = 0;
                Exception check;

                do
                {

                    try
                    {

                        var parser = new CardParser(nameToLink.Key, nameToLink.Value);
                        var card = parser.Parse().Result;
                        card.TcgExists = tcgLinks.ContainsKey(nameToLink.Key);
                        card.OcgExists = ocgLinks.ContainsKey(nameToLink.Key);

                        repo.InsertCardAsync(card).GetAwaiter().GetResult();

                        check = null;

                    }
                    catch (Exception ex)
                    {

                        retryCount++;
                        check = ex;

                        if (retryCount == Options.Config.MaxRetry)
                        {

                            errors.Add(new Error
                            {
                                Name = nameToLink.Key,
                                Message = ex.Message,
                                StackTrace = ex.StackTrace,
                                Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, nameToLink.Value),
                                Type = Common.Models.YuGiOh.Type.Card
                            });

                        }

                        Task.Delay(Options.Config.RetryDelay).GetAwaiter().GetResult();

                    }

                } while (check != null && retryCount < Options.Config.MaxRetry);

                Task.Run(() =>
                {

                    lock (countLock)
                    {

                        var current = Interlocked.Increment(ref count);

                        var display = $"Cards processed: {current}/{size} ({current / (double)size * 100:0.00}%)";

                        if (current % 1000 == 0 && Options.IsSubProc)
                            Log(display);
                        else
                            InlineLog(display);

                    }

                });

            });

            return new CardProcessorResponse
            {

                Count = size,
                Errors = errors

            };

        }

        public BoosterProcessorResponse ProcessBoosters(IDictionary<string, string> tcgLinks, IDictionary<string, string> ocgLinks)
        {

            var nameToLinks = tcgLinks
                .Concat(ocgLinks.Where(kv => !tcgLinks.ContainsKey(kv.Key)));
            var size = nameToLinks.Count();

            if (Options.MaxBoostersToParse <= size)
                nameToLinks = nameToLinks.RandomSubset(Options.MaxBoostersToParse);
            //nameToLinks = nameToLinks.Take(Options.MaxBoostersToParse);

            size = nameToLinks.Count();
            var errors = new ConcurrentBag<Error>();
            var repo = new YuGiOhRepository(this);
            var count = 0;
            var countLock = new object();

            Parallel.ForEach(nameToLinks, _parallelOptions, (nameToLink, _) =>
            {

                var retryCount = 0;
                Exception check;

                do
                {

                    try
                    {

                        var parser = new BoosterPackParser(nameToLink.Key, nameToLink.Value);
                        var card = parser.Parse().Result;
                        card.TcgExists = tcgLinks.ContainsKey(nameToLink.Key);
                        card.OcgExists = ocgLinks.ContainsKey(nameToLink.Key);

                        repo.InsertBoosterPack(card).GetAwaiter().GetResult();

                        check = null;

                    }
                    catch (Exception ex)
                    {

                        retryCount++;
                        check = ex;

                        if (retryCount == Options.Config.MaxRetry)
                        {

                            errors.Add(new Error
                            {
                                Name = nameToLink.Key,
                                Message = ex.Message,
                                StackTrace = ex.StackTrace,
                                Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, nameToLink.Value),
                                Type = Common.Models.YuGiOh.Type.Card
                            });

                        }

                        Task.Delay(Options.Config.RetryDelay).GetAwaiter().GetResult();

                    }

                } while (check != null && retryCount < Options.Config.MaxRetry);

                Task.Run(() =>
                {

                    lock (countLock)
                    {

                        var current = Interlocked.Increment(ref count);

                        var display = $"Booster packs processed: {current}/{size} ({current / (double)size * 100:0.00}%)";

                        if (current % 1000 == 0 && Options.IsSubProc)
                            Log(display);
                        else
                            InlineLog(display);

                    }

                });

            });

            return new BoosterProcessorResponse
            {

                Count = size,
                Errors = errors

            };

        }

        public void ProcessErrors(IEnumerable<Error> errors)
        {

            var repo = new YuGiOhRepository(this);
            var semaphore = new SemaphoreSlim(ConstantValue.ProcessorCount);
            var size = errors.Count();
            var tasks = new Task[size];
            var count = 0;
            var countLock = new object();

            for (var i = 0; i < size; i++)
            {

                var index = i;
                tasks[index] = Task.Run(async () =>
                {

                    await semaphore.WaitAsync();

                    try
                    {

                        await repo.InsertErrorAsync(errors.ElementAt(index));

                        lock (countLock)
                        {

                            var current = Interlocked.Increment(ref count);

                            var display = $"Cards processed: {current}/{size} ({current / (double)size * 100:0.00}%)";

                            if (current % 1000 == 0 && Options.IsSubProc)
                                Log(display);
                            else
                                InlineLog(display);

                        }

                    }
                    catch (Exception) { }
                    finally
                    {
                        semaphore.Release();
                    }

                });

            }

            Task.WaitAll(tasks);

        }

        public async Task<IDictionary<string, string>> GetLinks(string baseUrl)
        {

            var cmcontinue = "";
            var links = new Dictionary<string, string>();

            do
            {

                var url = baseUrl + string.Format(ConstantString.CmcontinueQuery, cmcontinue);
                var response = await Constant.HttpClient.GetStringAsync(url);
                var responseJObject = JObject.Parse(response);
                cmcontinue = responseJObject["continue"]?.Value<string>("cmcontinue");

                foreach (var item in responseJObject["query"].Value<JArray>("categorymembers"))
                    links[item.Value<string>("title")] = item.Value<string>("pageid");

            } while (!string.IsNullOrEmpty(cmcontinue));

            return links;

        }

        public void Log(string message)
        {

            if (Options.IsSubProc)
                Console.WriteLine(message);
            else
                Logger.Info(message);

        }

        public void InlineLog(string message)
        {

            if (Options.IsSubProc)
                Console.WriteLine(message);
            else
                Logger.InlineLog(LogLevel.Info, message);

        }

        public NpgsqlConnection GetYuGiOhDbConnection()
        {

            var config = Options.Config;

            var connectionStr = new NpgsqlConnectionStringBuilder
            {

                Host = config.Database.Host,
                Port = config.Database.Port,
                Database = "yugioh",
                Username = config.Database.Username,
                Password = config.Database.Password


            }.ToString();

            return new NpgsqlConnection(connectionStr);

        }

    }
}
