using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using Newtonsoft.Json.Linq;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Criterion;
using YuGiOh.Bot.Models.Deserializers;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Games : MainBase
    {

        private readonly Criteria<SocketMessage> _criteria = new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureNotBot());

        [Command("guess")]
        [Summary("Starts an image/card guessing game!")]
        public async Task GuessCommand()
        {

            if (Cache.GuessInProgress.TryAdd(Context.Channel.Id, null))
            {

                Card card = null;
                Exception e;

                do
                {

                    try
                    {

                        card = await YuGiOhDbService.GetRandomCardAsync();

                        var url = $"{Constants.ArtBaseUrl}{card.Passcode}.{Constants.ArtFileType}";

                        //$"https://storage.googleapis.com/ygoprodeck.com/pics_artgame/{passcode}.jpg"
                        var consoleOutput = $"{card.Name}";

                        if (!string.IsNullOrEmpty(card.RealName))
                            consoleOutput += $" / {card.RealName}";

                        consoleOutput += $"\n{Constants.ArtBaseUrl}{card.Passcode}.{Constants.ArtFileType}";

                        Console.WriteLine(consoleOutput);

                        using (var stream = await Web.GetStream(url))
                            await UploadAsync(stream, $"{GenObufscatedString()}.{Constants.ArtFileType}", $":stopwatch: You have **{_guildConfig.GuessTime}** seconds to guess what card this art belongs to! Case insensitive (used to be case sensitive)!");

                        e = null;

                    }
                    catch (NullReferenceException nullref) { e = nullref; }

                } while (e is not null);

                var criteria = new GuessCriteria(card.Name, card.RealName);

                //_criteria.AddCriterion(new GuessCriteria(art.Key));
                _criteria.AddCriterion(criteria);

                var answer = await NextMessageAsync(_criteria, TimeSpan.FromSeconds(_guildConfig.GuessTime));

                if (answer is not null)
                {

                    var author = answer.Author as SocketGuildUser;

                    //await ReplyAsync($":trophy: The winner is **{author.Nickname ?? author.Username}**! The card was `{art.Key}`!");
                    await ReplyAsync($":trophy: The winner is **{author.Nickname ?? author.Username}**! The card was `{criteria.Answer}`!");

                }
                else
                {

                    var possibleAnswersOutput = criteria.PossibleAnswers.Skip(1).Aggregate(new StringBuilder($"`{criteria.PossibleAnswers.First()}`"), (strBuilder, possibleAnswer) => strBuilder.Append(" or `").Append(possibleAnswer).Append('`'));

                    await ReplyAsync($":stop_button: Ran out of time! The card was {possibleAnswersOutput}!");

                }

            }
            else
                await ReplyAsync(":game_die: There is a game in progress!");

            Cache.GuessInProgress.TryRemove(Context.Channel.Id, out _);

        }

        [Command("hangman")]
        [Summary("Starts a hangman game!")]
        public async Task HangmanCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();
            var name = card.Name;
            var counter = 0;

            var indexToDisplay = name.Select(c =>
            {

                if (char.IsLetterOrDigit(c))
                    return "\\_ ";
                else if (char.IsWhiteSpace(c))
                    return "   ";
                else
                    return c.ToString();

            }).ToDictionary(_ => counter++, s => s);

            var display = indexToDisplay.Values.Join("");

            var check = new StringBuilder(name.Select(c =>
            {

                if (char.IsLetterOrDigit(c))
                    return ' ';
                else
                    return c;

            }).Join(""));

            //await ReplyAsync(card);
            AltConsole.Write("Command", "Hangman", $"{name}");
            await ReplyAsync($"You have **5** minutes to figure this card out!\n{display}");

            var cts = new CancellationTokenSource();
            var timer = new Timer((token) => (token as CancellationTokenSource).Cancel(), cts, TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(300));

            var lower = name.ToLower();
            var hanging = 0;
            SocketGuildUser winner = null;

            var guesses = new List<string>();

            do
            {

                var input = await NextMessageAsync(_criteria, token: cts.Token);

                if (cts.Token.IsCancellationRequested && input is null)
                    break;

                var content = input?.Content?.ToLower();

                if (content is null)
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

                    indexes.ForEach(i => check[i] = name[i]);
                    indexes.ForEach(i => indexToDisplay[i] = $"__{name[i]}__ ");
                    guesses.Add(content);

                    await ReplyAsync(indexToDisplay.Values.Join(""));

                    if (check.ToString() == name)
                        winner = input.Author as SocketGuildUser;

                }

            } while (check.ToString() != name);


            if (winner is not null)
                await ReplyAsync($":trophy: The winner is **{winner.Nickname ?? winner.Username}**!");
            else if (hanging == 6)
                await ReplyAsync($":stop_button: The guy got hanged! There was no winner. The card was `{name}`!");
            else
                await ReplyAsync($":stop_button: Time is up! There was no winner. The card was `{name}`!");

        }

        private string GetHangman(int hangman)
        {

            const string noose = " _________     \n" +
                                 "|         |    \n";

            return hangman switch
            {
                1 => noose +
                    "|         0    \n",
                2 => noose +
                    "|         0    \n" +
                    "|         |    \n",
                3 => noose +
                    "|         0    \n" +
                    "|        /|    \n",
                4 => noose +
                    "|         0    \n" +
                    "|        /|\\  \n",
                5 => noose +
                    "|         0    \n" +
                    "|        /|\\  \n" +
                    "|        /     \n",
                6 => noose +
                    "|         0    \n" +
                    "|        /|\\  \n" +
                    "|        / \\  \n",
                _ => "",
            };
        }

        private string GenObufscatedString()
        {

            var str = "";

            for (int i = 0; i < Rand.Next(10, 20); i++)
            {

                var displacement = Rand.Next(0, 26);
                str += (char)('a' + displacement);

            }

            return str;

        }

        private async Task<KeyValuePair<string, string>> GetArtTumblr()
        {

            var offset = Rand.Next(0, Cache.FYeahYgoCardArtPosts - 20);
            var response = await Web.GetDeserializedContent<JObject>($"https://api.tumblr.com/v2/blog/fyeahygocardart/posts/photo?api_key={Cache.TumblrKey}&limit=20&offset={offset}");
            var post = response["response"]["posts"].ToObject<JArray>().RandomSubset(1).First().ToObject<YgoCardArtPost>();
            var key = post.Name;
            var value = post.Photos.First().OriginalSize.Url;

            return new KeyValuePair<string, string>(key, value);

        }

    }
}
