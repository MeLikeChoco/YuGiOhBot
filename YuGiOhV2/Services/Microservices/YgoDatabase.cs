using Dapper;
using Dapper.Contrib.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Models;

namespace YuGiOhV2.Services.Microservices
{

    public class YgoDatabase
    {

        private readonly Timer _reformDatabase;
        //private readonly Cache _cache;
        //private readonly Web _web;

        //private IUserMessage _message;
        //private IEnumerable<Card> _cards;
        //private CancellationTokenSource _tokenSource;

        private const string BaseUrl = "http://yugipedia.com";

        private string DotNetRuntime
        {

            get
            {

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return Path.GetFullPath("/usr/share/dotnet/dotnet");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");

                else return null;

            }

        }

        public YgoDatabase(DiscordShardedClient client, Cache cache)
        {

            var args = Environment.GetCommandLineArgs();
            var info = new { Client = client, Cache = cache };

            if (args.Contains("nofirstscrape"))
            {

                var lastScrape = DateTime.Parse(File.ReadAllText("Databases/LastScrape.txt"));
                var difference = DateTime.UtcNow - lastScrape;

                Log($"Last scrape was {difference.TotalHours} hour(s) ago");

                if (difference > TimeSpan.FromDays(7))
                    difference = TimeSpan.FromDays(7);

                _reformDatabase = new Timer(ReformDatabase, info, difference, TimeSpan.FromDays(7));

            }
            else if (args.Contains("noscrape"))
                _reformDatabase = null;
            else
                _reformDatabase = new Timer(ReformDatabase, info, TimeSpan.FromSeconds(10), TimeSpan.FromDays(7));

            //=> _reformDatabase = Environment.GetCommandLineArgs().Contains("noscrape") ? null : new Timer(ReformDatabase, new { Client = client, Cache = cache }, TimeSpan.FromSeconds(10), TimeSpan.FromDays(7));
            //=> _reformDatabase = new Timer(ReformDatabase, new { Client = client, Cache = cache }, TimeSpan.FromSeconds(10), TimeSpan.FromDays(7));

        }

        public async void ReformDatabase(object state)
        {

            Log($"Updating \"{Constants.DatabaseString}\"...");

            var shouldRedo = false;
            var info = state as dynamic;
            var client = info.Client as DiscordShardedClient;
            var cache = info.Cache as Cache;

            if (File.Exists(Constants.DatabasePath))
                File.Copy(Constants.DatabasePath, $"Databases/Backup/{Constants.DatabaseName}.db", true);

            do
            {

                using (var process = new Process
                {

                    StartInfo = new ProcessStartInfo
                    {

                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        FileName = DotNetRuntime,
                        WorkingDirectory = "Databases/",
                        Arguments = $"YuGiOhScraper.dll 1"

                    }

                })
                {

                    process.OutputDataReceived += PrintOutput;
                    process.ErrorDataReceived += PrintOutput;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                }

                IEnumerable<string> errors;

                using (var db = new SqliteConnection("Data Source = Databases/ygopedia.db"))
                {

                    await db.OpenAsync();

                    errors = db.Query<string>("select Name from CardErrors");

                    db.Close();

                }

                var message = await (await client.GetUser(Config.Instance.OwnerId).CreateDMChannelAsync()).SendMessageAsync(embed: FancifyErrorList(errors));
                var tks = new CancellationTokenSource();

                #region CheckReaction
                Task CheckReaction(Cacheable<IUserMessage, ulong> messageCache, Cacheable<IMessageChannel, ulong> _, SocketReaction reaction)
                {

                    if (messageCache.Id == message.Id)
                    {

                        if (reaction.Emote.Name == "\u2705")
                        {

                            tks.Cancel();

                            shouldRedo = false;

                        }
                        else if (reaction.Emote.Name == "🔁")
                            shouldRedo = true;

                    }

                    return Task.CompletedTask;

                }
                #endregion CheckReaction

                client.ReactionAdded += CheckReaction;

                //this is so stupid...
                try { await Task.Delay(-1, tks.Token); } catch { }

                client.ReactionAdded -= CheckReaction;

            } while (shouldRedo);

            cache.Initialize();

            File.WriteAllText("Databases/LastScrape.txt", DateTime.UtcNow.ToString());

        }

        private void PrintOutput(object process, DataReceivedEventArgs args)
        {

            if (args.Data != null)
            {

                if (args.Data.StartsWith("Progress:"))
                {

                    const int trimStart = 10;
                    var parenthesesIndex = args.Data.IndexOf('(');
                    var progress = args.Data.Substring(trimStart, parenthesesIndex - trimStart).Trim();
                    var count = int.Parse(progress.Split('/').First());

                    if (count % 1000 == 0)
                        Log($"Progress: {progress}");

                }
                else
                    Log(args.Data);

            }

        }

        /// <param name="state">Web</param>
        //public async void ReformDatabase(object state)
        //{

        //    var web = state as Web;
        //    var tempCache = new YgoDatabaseTempCache();
        //    var links = await GetCardLinks(web, tempCache);

        //    Log("Preparing to update ygodb...");

        //    _cards = GetCards(web, links, tempCache, out var errors);

        //    Log("Updates finished. Sending results...");

        //    _message = await (await _client.GetUser(Config.Instance.OwnerId).GetOrCreateDMChannelAsync()).SendMessageAsync(embed: FancifyErrorList(errors));
        //    _tokenSource = new CancellationTokenSource();

        //    _client.ReactionAdded += CheckReactionWrapper;

        //    try
        //    {

        //        await Task.Delay(-1, _tokenSource.Token);

        //    }
        //    catch { }

        //    _client.ReactionAdded -= CheckReactionWrapper;
        //    _cache.Initialize();

        //}

        //private Task CheckReactionWrapper(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        //{

        //    Task.Run(() => CheckReaction(cache, channel, reaction));

        //    return Task.CompletedTask;

        //}

        //private async Task CheckReaction(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        //{

        //    if ((await cache.GetOrDownloadAsync()).Id == _message.Id)
        //    {

        //        if (reaction.Emote.Name == "\u2705")
        //        {

        //            await CardsToSqlite(_cards);
        //            _tokenSource.Cancel();

        //        }
        //        else if (reaction.Emote.Name == "\u274e")
        //            _tokenSource.Cancel();
        //        else if (reaction.Emote.Name == "🔁")
        //            ReformDatabase(_web);

        //    }

        //}

        //private async Task CardsToSqlite(IEnumerable<Card> cards)
        //{

        //    if (File.Exists("Databases/ygo.db"))
        //    {

        //        if (File.Exists("Databases/temp/ygo.db"))
        //            File.Delete("Databases/temp/ygo.db");

        //        File.Move("Databases/ygo.db", "Databases/temp/ygo.db");

        //    }

        //    using (var db = new SqliteConnection("Data Source = Databases/ygo.db"))
        //    {

        //        await db.OpenAsync();

        //        var createTable = db.CreateCommand();
        //        createTable.CommandText = "CREATE TABLE 'Cards'(" +
        //            "'Name' TEXT, " +
        //            "'RealName' TEXT, " +
        //            "'Passcode' TEXT, " +
        //            "'CardType' TEXT, " +
        //            "'Property' TEXT, " +
        //            "'Level' INTEGER, " +
        //            "'PendulumScale' INTEGER, " +
        //            "'Rank' INTEGER, " +
        //            "'Link' INTEGER, " +
        //            "'LinkArrows' TEXT, " +
        //            "'Types' TEXT, " +
        //            "'Attribute' TEXT, " +
        //            "'Materials' TEXT, " +
        //            "'Lore' TEXT, " +
        //            "'Atk' TEXT, " +
        //            "'Def' TEXT, " +
        //            "'Archetype' TEXT, " +
        //            "'Supports' TEXT, " +
        //            "'AntiSupports' TEXT, " +
        //            "'OcgExists' INTEGER, " +
        //            "'TcgExists' INTEGER, " +
        //            "'OcgStatus' TEXT, " +
        //            "'TcgAdvStatus' TEXT, " +
        //            "'TcgTrnStatus' TEXT, " +
        //            "'Img' TEXT, " +
        //            "'Url' TEXT " +
        //            ")";

        //        Log("Saving to ygo.db...");
        //        await createTable.ExecuteNonQueryAsync();
        //        await db.InsertAsync(cards);
        //        Log("Done saving to ygo.db.");

        //        db.Close();

        //    }

        //}

        //private IEnumerable<Card> GetCards(Web web, IDictionary<string, string> links, YgoDatabaseTempCache cache, out IEnumerable<KeyValuePair<string, Exception>> errorList)
        //{

        //    var cards = new ConcurrentBag<Card>();
        //    var count = links.Count;
        //    var pOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        //    var tempErrorList = new ConcurrentDictionary<string, Exception>();
        //    var counter = 0;

        //    Parallel.ForEach(links, pOptions, link =>
        //    {

        //        try
        //        {

        //            var url = $"{BaseUrl}{link.Value}";
        //            var response = web.GetString(url).Result;
        //            var card = new CardParser(link.Key, response).Parse();

        //            #region OCG TCG
        //            //c# int default is 0, therefore, if only one of them is 1, that means it is an format exclusive card
        //            //if both of them are 1, then it is in both formats
        //            if (cache.TCG.ContainsKey(card.Name))
        //                card.TcgExists = true;

        //            if (cache.OCG.ContainsKey(card.Name))
        //                card.OcgExists = true;
        //            #endregion OCG TCG

        //            card.Url = $"{BaseUrl}{link.Value}";

        //            cards.Add(card);

        //        }
        //        catch (Exception e)
        //        {

        //            tempErrorList[link.Key] = e;

        //        }

        //        var current = Interlocked.Increment(ref counter);

        //        if (current % 500 == 0)
        //            Log($"Ygodb status: {current}/{count}");

        //    });

        //    errorList = tempErrorList;

        //    return cards;

        //}

        ////there are two ways we could have done this
        ////1. assume the guy using this has bad internet
        ////2. assume the guy using this doesn't care about it
        ////I'll go with 2 to make my life easier
        ////I also most likely don't need parallel foreach, but whatever
        //private async Task<IDictionary<string, string>> GetCardLinks(Web web, YgoDatabaseTempCache cache)
        //{

        //    var responseTcg = await web.GetString($"{BaseUrl}/api/v1/Articles/List?category=TCG_cards&limit=20000&namespaces=0"); //as you can see, the 20000 assumes the user doesn't care about internet speed
        //    var responseOcg = await web.GetString($"{BaseUrl}/api/v1/Articles/List?category=OCG_cards&limit=20000&namespaces=0");
        //    var json = JObject.Parse(responseTcg);

        //    Parallel.ForEach(json["items"].ToObject<JArray>(), item => cache.TCG[item.Value<string>("title")] = item.Value<string>("url"));

        //    json = JObject.Parse(responseOcg);

        //    Parallel.ForEach(json["items"].ToObject<JArray>(), item => cache.OCG[item.Value<string>("title")] = item.Value<string>("url"));

        //    return cache.TCG.Concat(cache.OCG).GroupBy(kv => kv.Key).ToDictionary(group => group.Key, group => group.First().Value);
        //    //return cache.TCG.Concat(cache.OCG).GroupBy(kv => kv.Key).Take(1000).ToDictionary(group => group.Key, group => group.First().Value);

        //}

        private Embed FancifyErrorList(IEnumerable<string> errors)
        {

            var embed = new EmbedBuilder();

            if (errors.Any())
            {

                embed.Title = "Errors";
                var strBuilder = new StringBuilder("```");

                foreach (var error in errors)
                    strBuilder.AppendLine(error);

                strBuilder.Append("```");

                embed.Description = strBuilder.ToString();

            }
            else
                embed.Title = "No errors";

            return embed.Build();

        }

        //private Embed FancifyErrorList(IEnumerable<KeyValuePair<string, Exception>> errors)
        //{

        //    var embed = new EmbedBuilder();

        //    if (errors.Any())
        //    {

        //        foreach (var kv in errors)
        //            embed.AddField(kv.Key, $"```{kv.Value.Message}\n\n{kv.Value.StackTrace}```");

        //    }
        //    else
        //    {

        //        embed.Title = "No errors";

        //    }

        //    return embed.Build();

        //}

        private void InlineLog(string message)
            => AltConsole.InlineWrite("Info", "YgoDb", message);

        private void Log(string message)
            => AltConsole.Write("Info", "YgoDb", message);

    }

}
