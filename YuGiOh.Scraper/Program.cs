using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
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
using YuGiOh.Scraper.Models.Parsers;
using YuGiOh.Scraper.Models.Responses;
using Type = YuGiOh.Common.Models.YuGiOh.Type;

namespace YuGiOh.Scraper;

public class Program : IYuGiOhRepositoryConfiguration
{

    private static readonly Options Options = Options.Instance;
    private static ParallelOptions ParallelOptions => Options.IsDev ? Constant.SerialOptions : Constant.ParallelOptions;

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

        Log("Filtering cards...");

        var links = await FilterCardLinks(tcgLinks, ocgLinks);

        Log("Filtered cards.");

        var cardProcResponse = await ProcessCards(tcgLinks, ocgLinks, links);

        Log($"Processed {cardProcResponse.Count} cards. There were {cardProcResponse.Errors.Count} errors.");
        Log("Getting TCG booster packs.");

        tcgLinks = await GetLinks(ConstantString.MediaWikiTcgPacks);

        Log("Getting OCG booster packs.");

        ocgLinks = await GetLinks(ConstantString.MediaWikiOcgPacks);

        Log("Processing boosterpacks.");

        var boosterProcResponse = await ProcessBoosters(tcgLinks, ocgLinks);

        Log($"Processed {boosterProcResponse.Count} booster packs. There were {boosterProcResponse.Errors.Count} errors.");
        Log("Processing errors.");

        var errors = cardProcResponse.Errors.Concat(boosterProcResponse.Errors).ToList();

        await ProcessErrors(errors);

        Log($"Processed {errors.Count} errors.");
        Log("Sending Discord webhook database updated status...");

        await SendDiscordWebhook();
        
        Log("Sent Discord webhook database updated status.");

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
            .ToDictionary(kv => kv.Key, kv => kv.Value);

    }

    private async Task<CardProcessorResponse> ProcessCards(
        IDictionary<string, string> tcgLinks,
        IDictionary<string, string> ocgLinks,
        IDictionary<string, string> links
    )
    {

        var size = links.Count;

        if (Options.MaxCardsToParse <= size)
            links = links.RandomSubset(Options.MaxCardsToParse).ToDictionary(kv => kv.Key, kv => kv.Value);
        //links = links.Take(Options.MaxCardsToParse);

        var errors = new ConcurrentBag<Error>();
        var repo = new YuGiOhRepository(this);
        var count = 0;

        await Parallel.ForEachAsync(links, ParallelOptions, async (nameToLink, _) =>
        {

            var retryCount = 0;
            Exception check;

            do
            {

                var (name, link) = nameToLink;

                try
                {

                    var parser = new CardParser(name, link);
                    // var parserHash = await parser.GetParseOutput().ContinueWith(outputTask => outputTask.Result.GetMurMurHash(), _); //wanted to try ContinueWith instead of wrapping await
                    //
                    // //this is to determine whether or not to parse
                    // if (!Options.ShouldIgnoreHash && await repo.GetCardHashAsync(parser.Id) == parserHash)
                    //     continue;

                    var card = await parser.ParseAsync();
                    card.TcgExists = tcgLinks.ContainsKey(name);
                    card.OcgExists = ocgLinks.ContainsKey(name);

                    await repo.InsertCardAsync(card);
                    // await repo.InsertCardHashAsync(card.Id, parserHash);

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
                            Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, link),
                            Type = Type.Card,
                            Timestamp = DateTime.UtcNow
                        });

                    }

                    await Task.Delay(Options.Config.RetryDelay, CancellationToken.None);

                }
            } while (check is not null && retryCount < Options.Config.MaxRetry);

            var __ = WriteProgress("Cards", ref count, size);

        });

        ClearLine();

        return new CardProcessorResponse
        {

            Count = size,
            Errors = errors

        };

    }

    private async Task<BoosterProcessorResponse> ProcessBoosters(IDictionary<string, string> tcgLinks, IDictionary<string, string> ocgLinks)
    {

        var nameToLinks = tcgLinks.Union(ocgLinks).ToList();
        var size = nameToLinks.Count;

        if (Options.MaxBoostersToParse <= size)
        {
            nameToLinks = nameToLinks.RandomSubset(Options.MaxBoostersToParse).ToList();
            size = nameToLinks.Count;
        }

        //nameToLinks = nameToLinks.Take(Options.MaxBoostersToParse);
        var errors = new ConcurrentBag<Error>();
        var repo = new YuGiOhRepository(this);
        var count = 0;

        await Parallel.ForEachAsync(nameToLinks, ParallelOptions, async (nameToLink, _) =>
        {

            var retryCount = 0;
            Exception check;

            do
            {

                var (name, link) = nameToLink;

                try
                {

                    var parser = new BoosterPackParser(name, link);
                    var card = await parser.Parse();
                    card.TcgExists = tcgLinks.ContainsKey(name);
                    card.OcgExists = ocgLinks.ContainsKey(name);

                    await repo.InsertBoosterPack(card);

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
                            Url = string.Format(ConstantString.YugipediaUrl + ConstantString.MediaWikiIdUrl, link),
                            Type = Type.Booster,
                            Timestamp = DateTime.UtcNow
                        });

                    }

                    await Task.Delay(Options.Config.RetryDelay, CancellationToken.None);

                }

            } while (check is not null && retryCount < Options.Config.MaxRetry);

            var __ = WriteProgress("Booster packs", ref count, size);

        });

        ClearLine();

        return new BoosterProcessorResponse
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

            var url = baseUrl + string.Format(ConstantString.CmcontinueQuery, cmcontinue);
            var response = await Constant.HttpClient.GetStringAsync(url);
            var responseJObject = JObject.Parse(response);
            cmcontinue = responseJObject["continue"]?.Value<string>("cmcontinue");

            foreach (var item in responseJObject["query"]!.Value<JArray>("categorymembers")!)
                links[item.Value<string>("title")!] = item.Value<string>("pageid");

        } while (!string.IsNullOrEmpty(cmcontinue));

        return links;

    }

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

        if (!string.IsNullOrWhiteSpace(webhookConfig.Url) && Options.IsDebug)
        {
            
            using var httpClient = new HttpClient();
            var random = new Random();
            var payload = new WebhookMessage
            {
                Content = null,
                Embeds = new[]
                {
                    new WebhookMessageEmbed
                    {
                        Title = webhookConfig.Content,
                        Color = $"{random.Next(0x1000000)}"
                    }
                }
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

        var connectionStr = Options.IsDebug ? Options.Config.Databases.Staging : Options.Config.Databases.Production;

        if (!connectionStr.EndsWith(';'))
            connectionStr += ';';
        
        connectionStr += $"Pooling=true;Minimum Pool Size={ParallelOptions.MaxDegreeOfParallelism};Maximum Pool Size={ParallelOptions.MaxDegreeOfParallelism};";

        return new NpgsqlConnection(connectionStr);

    }

}