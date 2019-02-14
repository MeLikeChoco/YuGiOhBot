using Dapper;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Services;

namespace YuGiOhV2.Models.Services
{
    public class YuGiOhScraper : PredefinedService
    {

        public override TimeSpan Delay()
        {

            var args = Environment.GetCommandLineArgs();

            if (args.Contains("0"))
            {

                var lastScrape = DateTime.Parse(File.ReadAllText(Path.Combine(ServiceDirectory, "LastScrape.txt")));
                var delay = DateTime.UtcNow - lastScrape;

                Log($"Last scrape was {delay.TotalHours} hour(s) ago");

                if (delay < TimeSpan.FromDays(7))
                    return TimeSpan.FromDays(7);

            }
            else if (args.Contains("1"))
                return TimeSpan.MaxValue;

            return TimeSpan.FromSeconds(10);

        }

        private readonly DiscordShardedClient _client;
        private readonly Cache _cache;

        public YuGiOhScraper(DiscordShardedClient client, Cache cache)
            : base("YuGiOhScraper", "YuGiOhScraper", "YuGiOhScraper")
        {

            _client = client;
            _cache = cache;

        }

        public override async void Execute(object state)
        {

            Log($"Starting service \"{Name}\"...");

            var shouldRedo = false;

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
                        WorkingDirectory = ServiceDirectory,
                        Arguments = $"{Executable} -s 1 -j 1 -p 1"

                    }

                })
                //using (var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.In))
                {

                    process.OutputDataReceived += PrintOutput;
                    process.ErrorDataReceived += PrintOutput;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    //await pipe.ConnectAsync();
                    //Log($"Connected to \"{Name}\".");

                    //var notification = Serializer.Deserialize<bool>(pipe);

                    //Log($"Finished getting info from \"{Name}\".");
                    //pipe.Close();
                    process.WaitForExit();
                    Log($"Finished service \"{Name}\".");

                }

                IEnumerable<string> errors;

                using (var db = new SqliteConnection($"Data Source = {Path.Combine(ServiceDirectory, "ygofandom.db")}"))
                {

                    await db.OpenAsync();

                    errors = db.Query<string>("select Name from Errors");

                    db.Close();

                }

                var message = await (await _client.GetUser(Config.Instance.OwnerId).GetOrCreateDMChannelAsync()).SendMessageAsync(embed: FancifyErrorList(errors));
                var tks = new CancellationTokenSource();

                #region CheckReaction
                Task checkReaction(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel channel, SocketReaction reaction)
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

                _client.ReactionAdded += checkReaction;

                //this is so stupid...
                try { await Task.Delay(-1, tks.Token); } catch { }

                _client.ReactionAdded -= checkReaction;

            } while (shouldRedo);

            if (File.Exists($"Databases/ygofandom.db"))
                File.Delete($"Databases/ygofandom.db");

            File.Move(Path.Combine(ServiceDirectory, "ygofandom.db"), "Databases/ygofandom.db");

            if (File.Exists($"Databases/ygopedia.db"))
                File.Delete($"Databases/ygopedia.db");

            File.Move(Path.Combine(ServiceDirectory, "ygopedia.db"), "Databases/ygopedia.db");

            _cache.Initialize();

            File.WriteAllText(Path.Combine(ServiceDirectory, "LastScrape.txt"), DateTime.UtcNow.ToString());

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

    }
}
