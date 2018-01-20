using AngleSharp;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Objects;
using YuGiOhV2.Objects.Criterion;
using YuGiOhV2.Objects.Deserializers;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Games : CustomBase
    {

        private Cache _cache;
        private Web _web;
        private Random _rand;
        private Criteria<SocketMessage> _criteria;
        private Database _database;
        private Setting _setting;

        //private static ConcurrentDictionary<ulong, object> _inProgress = new ConcurrentDictionary<ulong, object>();
        private static ConcurrentDictionary<ulong, object> _inProgress = new ConcurrentDictionary<ulong, object>();

        public Games(Cache cache, Web web, Random random, Database database)
        {

            _cache = cache;
            _rand = random;
            _web = web;
            _criteria = new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureNotBot());
            _database = database;

        }

        protected override void BeforeExecute(CommandInfo command)
            => _setting = _database.Settings[Context.Guild.Id];

        [Command("guess")]
        [Summary("Starts an image/card guessing game!")]
        public async Task GuessCommand()
        {

            if (_inProgress.TryAdd(Context.Channel.Id, null))
            {

                //var art = await GetArt();

                //Console.WriteLine(art.Value);

                //using (var stream = await _web.GetStream(art.Value).ConfigureAwait(false))
                //{

                //    await UploadAsync(stream, $"{GenObufscatedString()}.png", ":stopwatch: You have **60** seconds to guess what card this art belongs to! Case sensitive!");
                //    await stream.FlushAsync();

                //}

                KeyValuePair<string, string> passcode;
                Exception e;

                do
                {

                    try
                    {

                        passcode = _cache.Passcodes.RandomSubset(1).First();

                        Console.WriteLine($"https://raw.githubusercontent.com/shadowfox87/YGOTCGOCGPics323x323/master/{passcode.Key}.png");

                        using (var stream = await GetArtGithub(passcode.Key))
                            await UploadAsync(stream, $"{GenObufscatedString()}.png", $":stopwatch: You have **{_setting.GuessTime}** seconds to guess what card this art belongs to! Case sensitive!");

                        e = null;

                    }
                    catch (NullReferenceException nullref)
                    {

                        e = nullref;

                    }

                } while (e != null);

                //_criteria.AddCriterion(new GuessCriteria(art.Key));
                _criteria.AddCriterion(new GuessCriteria(passcode.Value));

                var answer = await NextMessageAsync(_criteria, TimeSpan.FromSeconds(_setting.GuessTime));

                if (answer != null)
                {

                    var author = answer.Author as SocketGuildUser;

                    //await ReplyAsync($":trophy: The winner is **{author.Nickname ?? author.Username}**! The card was `{art.Key}`!");
                    await ReplyAsync($":trophy: The winner is **{author.Nickname ?? author.Username}**! The card was `{passcode.Value}`!");

                }
                else
                    await ReplyAsync($":stop_button: Ran out of time! The card was `{passcode.Value}`!");

            }
            else
                await ReplyAsync($":game_die: There is a game in progress!");

            _inProgress.TryRemove(Context.Channel.Id, out var blarg);

        }

        [Command("hangman")]
        [Summary("Starts a hangman game!")]
        public async Task HangmanCommand()
        {

            var card = _cache.Uppercase.RandomSubset(1, _rand).First();
            var counter = 0;

            var indexToDisplay = card.Select(c =>
            {

                if (char.IsLetterOrDigit(c))
                    return "\\_ ";
                else if (char.IsWhiteSpace(c))
                    return "   ";
                else
                    return c.ToString();

            }).ToDictionary(s => counter++, s => s);

            var display = string.Join("", indexToDisplay.Values);

            var check = new StringBuilder(string.Join("", card.Select(c =>
            {

                if (char.IsLetterOrDigit(c))
                    return ' ';
                else
                    return c;

            })));

            //await ReplyAsync(card);
            AltConsole.Print("Command", "Hangman", $"{card}");
            await ReplyAsync($"You have **5** minutes to figure this card out!\n{display}");

            var cts = new CancellationTokenSource();
            var timer = new Timer(new TimerCallback((token) => (token as CancellationTokenSource).Cancel()), cts, TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(300));
            var lower = card.ToLower();
            var hanging = 0;
            SocketGuildUser winner = null;

            await Task.Run(async () =>
            {

                var guesses = new List<string>();

                do
                {

                    var input = await Task.Run(async () => await NextMessageAsync(_criteria), cts.Token);

                    if (cts.Token.IsCancellationRequested && input == null)
                        break;

                    var content = input.Content?.ToLower();

                    if (content == null)
                        continue;
                    else if (content.Length != 1)
                        continue;
                    else if (guesses.Contains(content))
                        await ReplyAsync($"You already guessed `{content}`!");
                    else if (!lower.Contains(content))
                    {

                        await ReplyAsync($"```fix\n{GetHangman(++hanging)}```");
                        guesses.Add(content);

                        if (hanging == 6)
                            break;

                    }
                    else
                    {

                        var indexes = new List<int>(5);

                        for (int i = lower.IndexOf(content); i != -1; i = lower.IndexOf(content, ++i))
                            indexes.Add(i);

                        indexes.ForEach(i => check[i] = card[i]);
                        indexes.ForEach(i => indexToDisplay[i] = $"__{card[i]}__ ");
                        guesses.Add(content);

                        await ReplyAsync(string.Join("", indexToDisplay.Values));

                        if (check.ToString() == card)
                            winner = input.Author as SocketGuildUser;

                    }

                } while (check.ToString() != card);

            }, cts.Token);

            timer.Dispose();

            if (winner != null)
                await ReplyAsync($":trophy: The winner is **{winner.Nickname ?? winner.Username}**!");
            else if (hanging == 6)
                await ReplyAsync($":stop_button: The guy got hanged! There was no winner. The card was `{card}`!");
            else
                await ReplyAsync($":stop_button: Time is up! There was no winner. The card was `{card}`!");

        }

        private string GetHangman(int hangman)
        {

            var noose = " _________     \n" +
                        "|         |    \n";

            switch (hangman)
            {

                case 1:
                    return noose +
                        "|         0    \n";
                case 2:
                    return noose +
                        "|         0    \n" +
                        "|         |    \n";
                case 3:
                    return noose +
                    "|         0    \n" +
                    "|        /|    \n";
                case 4:
                    return noose +
                    "|         0    \n" +
                    "|        /|\\  \n";
                case 5:
                    return noose +
                    "|         0    \n" +
                    "|        /|\\  \n" +
                    "|        /     \n";
                case 6:
                    return noose +
                    "|         0    \n" +
                    "|        /|\\  \n" +
                    "|        / \\  \n";
                default:
                    return "";

            }

        }

        private string GenObufscatedString()
        {

            var str = "";

            for (int i = 0; i < _rand.Next(10, 20); i++)
            {

                var displacement = _rand.Next(0, 26);
                str += (char)('a' + displacement);

            }

            return str;

        }

        private Task<Stream> GetArtGithub(string passcode)
        {

            var url = $"https://raw.githubusercontent.com/shadowfox87/YGOTCGOCGPics323x323/master/{passcode}.png";
            return _web.GetStream(url);

        }

        private async Task<KeyValuePair<string, string>> GetArt()
        {

            var offset = _rand.Next(0, _cache.FYeahYgoCardArtPosts - 20);
            var response = await _web.GetDeserializedContent<JObject>($"https://api.tumblr.com/v2/blog/fyeahygocardart/posts/photo?api_key={_cache.TumblrKey}&limit=20&offset={offset}");
            var post = response["response"]["posts"].ToObject<JArray>().RandomSubset(1).First().ToObject<YgoCardArtPost>();
            var key = post.Name;
            var value = post.Photos.First().OriginalSize.Url;

            return new KeyValuePair<string, string>(key, value);

        }

    }
}
