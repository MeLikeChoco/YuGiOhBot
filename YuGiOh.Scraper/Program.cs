using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json.Linq;
using Npgsql;
using YuGiOh.Common.Extensions;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Common.Repositories;
using YuGiOh.Common.Services;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Models;
using YuGiOh.Scraper.Models.Exceptions;
using YuGiOh.Scraper.Models.ParserOptions;
using YuGiOh.Scraper.Models.Parsers;
using YuGiOh.Scraper.Models.Responses;

namespace YuGiOh.Scraper;

public class Program : IYuGiOhRepositoryConfiguration
{

    private static readonly HttpClient HttpClient = new();
    private static readonly Options Options = Options.GetInstance(new OptionsArgs());
    private static ParallelOptions ParallelOptions => Options.IsDebug ? Constant.SerialOptions : Constant.ParallelOptions;

    private static readonly IEnumerable<TypeInfo> Parsers = Assembly.GetEntryAssembly()!.DefinedTypes.Where(type =>
    {

        var parserModuleAttribute = type.GetCustomAttribute<ParserModuleAttribute>();

        return parserModuleAttribute != null && parserModuleAttribute.Name == Options.Module;

    });

    public static Task Main()
        => new Program().Run();

    private async Task Run()
    {

        Log($"Dev mode: {Options.IsDev}");
        Log($"Debug mode: {Options.IsDebug}");
        Log($"Processing with {ParallelOptions.MaxDegreeOfParallelism} threads");
        Log("Getting TCG cards.");

        var tcgLinks = await GetLinks(ConstantString.MediaWikiTcgCards);

        Log("Getting OCG cards.");

        var ocgLinks = await GetLinks(ConstantString.MediaWikiOcgCards);

        Log("Getting anime cards.");

        var animeLinks = await GetLinks(ConstantString.MediaWikiAnimeCards);

        Log("Filtering cards...");

        var links = await FilterCardLinks(tcgLinks, ocgLinks);

        Log("Filtered cards.");

        var cardProcResponse = await ProcessCards(tcgLinks, ocgLinks, links);

        Log($"Processed {cardProcResponse.Count} cards. There were {cardProcResponse.Errors.Count} errors.");
        Log("Getting TCG booster packs.");

        tcgLinks = await GetLinks(ConstantString.MediaWikiTcgPacks);

        Log("Getting OCG booster packs.");

        ocgLinks = await GetLinks(ConstantString.MediaWikiOcgPacks);

        Log("Filtering booster packs...");

        links = FilterBoosterPackLinks(tcgLinks, ocgLinks);

        Log("Filtered booster packs...");
        Log("Processing boosterpacks.");

        var boosterProcResponse = await ProcessBoosters(links, tcgLinks, ocgLinks);

        Log($"Processed {boosterProcResponse.Count} booster packs. There were {boosterProcResponse.Errors.Count} errors.");
        Log("Processing anime cards.");

        var animeCardProcResponse = await ProcessAnimeCards(animeLinks);

        Log($"Processed {animeCardProcResponse.Count} anime cards. There were {animeCardProcResponse.Errors.Count} errors.");
        Log("Processing errors.");

        var errors = cardProcResponse.Errors.Concat(boosterProcResponse.Errors).Concat(animeCardProcResponse.Errors).ToList();

        await ProcessErrors(errors);

        Log($"Processed {errors.Count} errors.");
        Log("Sending Discord webhook database updated status...");

        await SendDiscordWebhook();

        Log("Sent Discord webhook database updated status.");
        Log("Finished updating the database.");

    }

    private async Task<IDictionary<string, string>> FilterCardLinks(
        IDictionary<string, string> tcgLinks,
        IDictionary<string, string> ocgLinks
    )
    {

        var links = tcgLinks.Union(ocgLinks);
        var tokenLinks = await GetLinks(ConstantString.MediaWikiTokenCards);
        var skillLinks = await GetLinks(ConstantString.MediaWikiSkillCards);

        return links
            .Except(tokenLinks)
            .Except(skillLinks)
            .Where(kv => !kv.Key.ContainsIgnoreCase("alternate password"))
            .Where(kv => !kv.Key.ContainsIgnoreCase("rush duel"))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

    }

    private IDictionary<string, string> FilterBoosterPackLinks(
        IDictionary<string, string> tcgLinks,
        IDictionary<string, string> ocgLinks
    )
    {

        var links = tcgLinks.Union(ocgLinks);

        return links
            .Where(kv => !kv.Key.ContainsIgnoreCase("category:"))
            .ToDictionary();

    }

    private async Task<ProcessorResponse> ProcessCards(
        IDictionary<string, string> tcgLinks,
        IDictionary<string, string> ocgLinks,
        IDictionary<string, string> links
    )
    {

        var size = links.Count;

        if (Options.MaxCardsToParse <= size)
        {
            links = links.RandomSubset(Options.MaxCardsToParse).ToDictionary(kv => kv.Key, kv => kv.Value);
            size = links.Count;
        }

        //links = links.Take(Options.MaxCardsToParse);

        var parserModule = GetParserModule<CardEntity>();
        var errors = new ConcurrentBag<Error>();
        var repo = new YuGiOhRepository(this);
        var count = 0;

        await Parallel.ForEachAsync(links, ParallelOptions, async (nameToLink, _) =>
        {

            var retryCount = 0;
            Exception check = null;

            do
            {

                var (name, id) = nameToLink;

                try
                {

                    var parser = Activator.CreateInstance(parserModule, id, name) as ICanParse<CardEntity>;
                    // var parserHash = await parser.GetParseOutput().ContinueWith(outputTask => outputTask.Result.GetMurMurHash(), _); //wanted to try ContinueWith instead of wrapping await
                    //
                    // //this is to determine whether or not to parse
                    // if (!Options.ShouldIgnoreHash && await repo.GetCardHashAsync(parser.Id) == parserHash)
                    //     continue;

                    var card = await parser!.ParseAsync();
                    card.TcgExists = tcgLinks.ContainsKey(name);
                    card.OcgExists = ocgLinks.ContainsKey(name);

                    await repo.InsertCardAsync(card);
                    // await repo.InsertCardHashAsync(card.Id, parserHash);

                    check = null; //reset exception if previous runs errored

                }
                catch (RushDuelException ex)
                {

                    errors.Add(new Error
                    {
                        Name = name,
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        Url = string.Format(Constant.ModuleToBaseUrl[Options.Module] + ConstantString.MediaWikiIdUrl, id),
                        Type = Common.Models.YuGiOh.Type.Card,
                        Timestamp = DateTime.UtcNow
                    });

                }
                catch (Exception ex)
                {

                    retryCount++;
                    check = ex;

                    if (retryCount == Options.Config.MaxRetry)
                    {

                        errors.Add(new Error
                        {
                            Name = name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            Url = string.Format(Constant.ModuleToBaseUrl[Options.Module] + ConstantString.MediaWikiIdUrl, id),
                            Type = Common.Models.YuGiOh.Type.Card,
                            Timestamp = DateTime.UtcNow
                        });

                    }

                    await Task.Delay(Options.Config.RetryDelay, CancellationToken.None);

                }

            } while (check is not null && retryCount < Options.Config.MaxRetry);

            var __ = WriteProgress("Cards", ref count, size);

        });

        ClearLine();

        return new ProcessorResponse
        {

            Count = size,
            Errors = errors

        };

    }

    private async Task<ProcessorResponse> ProcessBoosters(
        IDictionary<string, string> links,
        IDictionary<string, string> tcgLinks,
        IDictionary<string, string> ocgLinks
    )
    {

        var size = links.Count;

        if (Options.MaxBoostersToParse <= size)
        {
            links = links.RandomSubset(Options.MaxBoostersToParse).ToDictionary();
            size = links.Count;
        }

        //nameToLinks = nameToLinks.Take(Options.MaxBoostersToParse);
        var parserModule = GetParserModule<BoosterPackEntity>();
        var errors = new ConcurrentBag<Error>();
        var repo = new YuGiOhRepository(this);
        var count = 0;

        await Parallel.ForEachAsync(links, ParallelOptions, async (nameToLink, _) =>
        {

            var retryCount = 0;
            Exception check;

            do
            {

                var (name, id) = nameToLink;

                try
                {

                    var parser = Activator.CreateInstance(parserModule, id, name) as ICanParse<BoosterPackEntity>;
                    var card = await parser!.ParseAsync();
                    card.TcgExists = tcgLinks.ContainsKey(name);
                    card.OcgExists = ocgLinks.ContainsKey(name);

                    await repo.InsertBoosterPack(card);

                    check = null;

                }
                catch (InvalidBoosterPack)
                {

                    check = null;

                    errors.Add(new Error()
                    {
                        Name = name,
                        Message = "Invalid booster pack",
                        Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, id),
                        Type = Common.Models.YuGiOh.Type.Booster,
                        Timestamp = DateTime.UtcNow
                    });

                }
                catch (Exception ex)
                {

                    retryCount++;
                    check = ex;

                    if (retryCount == Options.Config.MaxRetry)
                    {

                        errors.Add(new Error
                        {
                            Name = name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, id),
                            Type = Common.Models.YuGiOh.Type.Booster,
                            Timestamp = DateTime.UtcNow
                        });

                    }

                    await Task.Delay(Options.Config.RetryDelay, CancellationToken.None);

                }

            } while (check is not null && retryCount < Options.Config.MaxRetry);

            var __ = WriteProgress("Booster packs", ref count, size);

        });

        ClearLine();

        return new ProcessorResponse
        {

            Count = size,
            Errors = errors

        };

    }

    private async Task<ProcessorResponse> ProcessAnimeCards(IDictionary<string, string> links)
    {

        var size = links.Count;

        if (Options.AnimeCardsToParse <= size)
        {
            links = links.RandomSubset(Options.AnimeCardsToParse).ToDictionary(kv => kv.Key, kv => kv.Value);
            size = links.Count;
        }

        var parserModule = GetParserModule<AnimeCardEntity>();
        var errors = new ConcurrentBag<Error>();
        var repo = new YuGiOhRepository(this);
        var count = 0;

        await Parallel.ForEachAsync(links, ParallelOptions, async (nameToLink, _) =>
        {

            var retryCount = 0;
            Exception check;

            do
            {

                var (name, id) = nameToLink;

                try
                {

                    var parser = Activator.CreateInstance(parserModule, id, name) as ICanParse<AnimeCardEntity>;
                    var card = await parser!.ParseAsync();

                    await repo.InsertAnimeCardAsync(card);

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
                            Name = name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            Url = string.Format(Constant.ModuleToBaseUrl[Options.Module] + ConstantString.MediaWikiIdUrl, id),
                            Type = Common.Models.YuGiOh.Type.Anime,
                            Timestamp = DateTime.UtcNow
                        });

                    }

                    await Task.Delay(Options.Config.RetryDelay, CancellationToken.None);

                }

            } while (check is not null && retryCount < Options.Config.MaxRetry);

            var __ = WriteProgress("Anime cards", ref count, size);

        });

        ClearLine();

        return new ProcessorResponse
        {

            Count = size,
            Errors = errors

        };

    }

    private async Task ProcessErrors(IEnumerable<Error> errors)
    {

        var repo = new YuGiOhRepository(this);
        var errorList = errors.ToList();
        var size = errorList.Count;
        var count = 0;

        await Parallel.ForEachAsync(errorList, ParallelOptions, async (error, _) =>
        {

            await repo.InsertErrorAsync(error);

            var __ = WriteProgress("Errors", ref count, size);

        });

        ClearLine();

    }

    private static async Task<IDictionary<string, string>> GetLinks(string baseUrl)
    {

        var cmcontinue = "";
        var links = new Dictionary<string, string>();

        do
        {

            var url = Constant.ModuleToBaseUrl[Options.Module] + baseUrl + string.Format(ConstantString.CmcontinueQuery, cmcontinue);
            var response = await HttpClient.GetStringAsync(url);
            var responseJObject = JObject.Parse(response);
            cmcontinue = responseJObject["continue"]?.Value<string>("cmcontinue");

            foreach (var item in responseJObject["query"]!.Value<JArray>("categorymembers")!)
                links[item.Value<string>("title")!] = item.Value<string>("pageid");

        } while (!string.IsNullOrEmpty(cmcontinue));

        return links;

    }

    private static TypeInfo GetParserModule<T>()
        => Parsers.FirstOrDefault(parserType => parserType.ImplementedInterfaces.Any(type => type.GenericTypeArguments.Contains(typeof(T))));

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static Task WriteProgress(string what, ref int current, int size)
    {

        var curr = Interlocked.Increment(ref current);
        var display = $"{what} processed: {curr}/{size} ({(double) current / size * 100:0.00}%)";

        if (curr % 1000 == 0 && Options.IsSubProc)
            Log(display);
        else
            InlineLog(display);

        return Task.CompletedTask;

    }

    private static async Task SendDiscordWebhook()
    {

        var webhookConfig = Options.Config.Webhook;

        if (!string.IsNullOrWhiteSpace(webhookConfig.Url) && !Options.IsDebug && !Options.IsDev)
        {

            using var httpClient = new HttpClient();
            var random = new Random();
            var payload = new WebhookMessage
            {
                Content = null,
                Embeds =
                [
                    new WebhookMessageEmbed
                    {
                        Title = webhookConfig.Content,
                        Color = $"{random.Next(0x1000000)}"
                    }
                ]
            };
            var json = JsonSerializer.Serialize(payload);
            // var json = "{\"content\":null,\"embeds\":[{\"title\":\"Database updated\", \"color\":5814783, \"timestamp\":\"2022-04-01T04:00:00.000Z\"}]}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(webhookConfig.Url, content);

            Log($"Discord webhook status: {response.StatusCode}");

        }

    }

    private static void Log(string message)
    {

        if (Options.IsSubProc)
            Console.WriteLine(message);
        else
            Logger.Info(message);

    }

    private static void InlineLog(string message)
    {

        if (Options.IsSubProc)
            Console.WriteLine(message);
        else
            Logger.InlineLog(LogLevel.Info, message);

    }

    private static void ClearLine()
        => Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");

    public NpgsqlConnection GetYuGiOhDbConnection()
    {

        var connectionStr = Options.IsDev ? Options.Config.Databases.Staging : Options.Config.Databases.Production;

        if (!connectionStr.EndsWith(';'))
            connectionStr += ';';

        connectionStr += $"Pooling=true;Minimum Pool Size={ParallelOptions.MaxDegreeOfParallelism};Maximum Pool Size={ParallelOptions.MaxDegreeOfParallelism};";

        return new NpgsqlConnection(connectionStr);

    }

}