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

namespace YuGiOh.Bot.Modules.Commands
{
    [RequireContext(ContextType.Guild)]
    public class Games : MainBase
    {

        private Criteria<SocketMessage> BaseCriteria => new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new NotBotCriteria());

        [Command("guess")]
        [Summary("Starts an image/card guessing game!")]
        public async Task GuessCommand()
        {

            if (Cache.GuessInProgress.ContainsKey(Context.Channel.Id))
            {

                await ReplyAsync(":game_die: There is a game in progress!");

                return;

            }

            try
            {

                Cache.GuessInProgress.TryAdd(Context.Channel.Id, null);

                Card card = null;
                Exception e;

                do
                {

                    try
                    {

                        card = await YuGiOhDbService.GetRandomCardAsync();

                        if (string.IsNullOrEmpty(card.Passcode))
                            throw new NullReferenceException(nameof(card.Passcode));

                        //https://storage.googleapis.com/ygoprodeck.com/pics_artgame/{passcode}.jpg
                        var url = $"{Constants.ArtBaseUrl}{card.Passcode}.{Constants.ArtFileType}";
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
                var answer = await NextMessageAsync(BaseCriteria.AddCriterion(criteria), TimeSpan.FromSeconds(_guildConfig.GuessTime));

                if (answer is not null)
                {
                    await ReplyAsync($":trophy: The winner is **{(answer.Author as SocketGuildUser)?.Nickname ?? answer.Author.Username}**! The card was `{criteria.Answer}`!");
                }
                else
                {

                    var possibleAnswersOutput = criteria.PossibleAnswers
                        .Skip(1)
                        .Aggregate(
                            new StringBuilder($"`{criteria.PossibleAnswers.First()}`"),
                            (strBuilder, possibleAnswer) => strBuilder.Append(" or `").Append(possibleAnswer).Append('`')
                        );

                    await ReplyAsync($":stop_button: Ran out of time! The card was {possibleAnswersOutput}!");

                }

            }
            catch (Exception ex)
            {
                AltConsole.Write("Command", "Guess", "There was a problem with guess!", exception: ex);
            }
            finally
            {
                Cache.GuessInProgress.TryRemove(Context.Channel.Id, out _);
            }


        }

        [Command("hangman")]
        [Summary("Starts a hangman game!")]
        public async Task HangmanCommand()
        {

            if (Cache.HangmanInProgress.ContainsKey(Context.Channel.Id))
            {

                await ReplyAsync(":game_die: There is a game in progress in this channel!");

                return;

            }

            Cache.HangmanInProgress[Context.Channel.Id] = null;

            try
            {
                var card = await YuGiOhDbService.GetRandomCardAsync();

                AltConsole.Write("Command", "Hangman", card.Name);

                var cts = new CancellationTokenSource();
                var hangmanService = new HangmanService(card.Name);

                var criteria = BaseCriteria
                    .AddCriterion(new NotCommandCriteria(_guildConfig))
                    .AddCriterion(new NotInlineSearchCriteria());

                if (!_guildConfig.HangmanAllowWords)
                    criteria.AddCriterion(new CharacterOnlyCriteria());

                var time = TimeSpan.FromSeconds(_guildConfig.HangmanTime);

                await ReplyAsync("You can now type more than a letter for hangman!\n" +
                    $"As well as change the hangman time ({_guildConfig.Prefix}hangmantime <seconds>)! Ask an admin about it!\n" +
                    $"You may also disable the ability to input more than one letter! ({_guildConfig.Prefix}hangmanwords <true/false>)\n" +
                    $"You have **{time.ToPrettyString()}**!\n" +
                    hangmanService.GetCurrentDisplay());

                var _ = new Timer((cts) => (cts as CancellationTokenSource)?.Cancel(), cts, TimeSpan.FromSeconds(_guildConfig.HangmanTime), Timeout.InfiniteTimeSpan);
                SocketUser user = null;

                do
                {

                    var input = await NextMessageAsync(criteria, token: cts.Token);

                    if (cts.IsCancellationRequested)
                        break;

                    user = input.Author;

                    switch (hangmanService.AddGuess(input.Content))
                    {

                        case GuessStatus.Duplicate:
                            await ReplyAsync($"You already guessed `{input}`!\n" +
                                hangmanService.GetCurrentDisplay());
                            break;
                        case GuessStatus.Nonexistent:
                            await ReplyAsync($"```fix\n{hangmanService.GetHangman()}```\n" +
                                hangmanService.GetCurrentDisplay());
                            break;
                        case GuessStatus.Accepted:
                            await ReplyAsync(hangmanService.GetCurrentDisplay());
                            break;

                    }

                } while (!cts.IsCancellationRequested && hangmanService.CompletionStatus == CompletionStatus.Incomplete);

                if (cts.IsCancellationRequested)
                {
                    await ReplyAsync($"Time is up! No one won! The card is `{hangmanService.Word}`");
                }
                else
                {

                    switch (hangmanService.CompletionStatus)
                    {

                        case CompletionStatus.Complete:
                            await ReplyAsync($":trophy: The winner is **{(user as SocketGuildUser)?.Nickname ?? user.Username}**!");
                            break;
                        case CompletionStatus.Hanged:
                            await ReplyAsync($"You have been hanged! The card was `{hangmanService.Word}`.");
                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                AltConsole.Write("Command", "Hangman", "There was a problem with hangman!", exception: ex);
            }
            finally
            {
                Cache.HangmanInProgress.TryRemove(Context.Channel.Id, out _);
            }

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
