using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Criteria;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Commands
{
    [RequireContext(ContextType.Guild)]
    public class Games : MainBase
    {

        private Criteria BaseCriteria => new Criteria()
            .AddCriteria(new ChannelCriteria(Context.Channel))
            .AddCriteria(new NotBotCriteria());

        private readonly ILoggerFactory _loggerFactory;

        public Games(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand,
            InteractiveService interactiveService
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, rand, interactiveService)
        {
            _loggerFactory = loggerFactory;
        }

        [Command("guess")]
        [Summary("Starts an image/card guessing game!")]
        public async Task GuessCommand()
        {

            if (Cache.GuessInProgress.ContainsKey(Context.Channel.Id))
            {

                await ReplyAsync(":game_die: There is a game in progress!");

                return;

            }

            var logger = _loggerFactory.CreateLogger("Guess");

            try
            {

                Cache.GuessInProgress.TryAdd(Context.Channel.Id, null);

                Card card = null!;
                Exception e;

                do
                {

                    try
                    {

                        card = await YuGiOhDbService.GetRandomCardAsync();

                        if (string.IsNullOrEmpty(card.Passcode))
                            throw new NullReferenceException(nameof(card.Passcode));

                        //https://storage.googleapis.com/ygoprodeck.com/pics_artgame/{passcode}.jpg
                        var url = $"{Constants.Url.ArtBaseUrl}{card.Passcode}.{Constants.ArtFileType}";
                        var name = $"{card.Name}";

                        if (!string.IsNullOrEmpty(card.RealName))
                            name += $" / {card.RealName}";

                        logger.Info("{CardName} {Url}", name, url);

                        await using (var stream = await Web.GetStream(url))
                            await UploadAsync(stream, $"{GenObufscatedString()}.{Constants.ArtFileType}", $":stopwatch: You have **{GuildConfig.GuessTime}** seconds to guess what card this art belongs to! Case insensitive (used to be case sensitive)!");

                        e = null;

                    }
                    catch (NullReferenceException nullref)
                    {
                        e = nullref;
                    }

                } while (e is not null);

                var criteria = new GuessCriteria(card.Name, card.RealName);
                var answer = await NextMessageAsync(BaseCriteria.AddCriteria(criteria), TimeSpan.FromSeconds(GuildConfig.GuessTime));

                if (answer.IsSuccess)
                {

                    var message = answer.Value;
                    
                    await ReplyAsync($":trophy: The winner is **{(message.Author as SocketGuildUser)?.Nickname ?? message.Author.Username}**! The card was `{criteria.Answer}`!");
                    
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
                logger.Error(ex, "There was a problem with guess!");
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

            var logger = _loggerFactory.CreateLogger("Hangman");

            Cache.HangmanInProgress[Context.Channel.Id] = null;

            try
            {
                var card = await YuGiOhDbService.GetRandomCardAsync();

                logger.Info(card.Name);

                var cts = new CancellationTokenSource();
                var hangmanService = new Hangman(card.Name);

                var criteria = BaseCriteria
                    .AddCriteria(new NotCommandCriteria(GuildConfig))
                    .AddCriteria(new NotInlineSearchCriteria());

                if (!GuildConfig.HangmanAllowWords)
                    criteria.AddCriteria(new CharacterOnlyCriteria());

                var time = TimeSpan.FromSeconds(GuildConfig.HangmanTime);

                await ReplyAsync("You can now type more than a letter for hangman!\n" +
                                 $"As well as change the hangman time ({GuildConfig.Prefix}hangmantime <seconds>)! Ask an admin about it!\n" +
                                 $"You may also disable the ability to input more than one letter! ({GuildConfig.Prefix}hangmanwords <true/false>)\n" +
                                 $"You have **{time.ToPrettyString()}**!\n" +
                                 hangmanService.GetCurrentDisplay());

                var _ = new Timer((cancelTokenSrc) => (cancelTokenSrc as CancellationTokenSource)!.Cancel(), cts, TimeSpan.FromSeconds(GuildConfig.HangmanTime), Timeout.InfiniteTimeSpan);
                SocketUser user = null;

                do
                {

                    var input = await NextMessageAsync(criteria, cts.Token);

                    if (cts.IsCancellationRequested)
                        break;

                    var message = input.Value;
                    user = message.Author;

                    switch (hangmanService.AddGuess(message.Content))
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
                            await ReplyAsync($":trophy: The winner is **{(user as SocketGuildUser)!.Nickname ?? user!.Username}**!");
                            break;
                        case CompletionStatus.Hanged:
                            await ReplyAsync($"You have been hanged! The card was `{hangmanService.Word}`.");
                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was a problem with hangman!");
            }
            finally
            {
                Cache.HangmanInProgress.TryRemove(Context.Channel.Id, out _);
            }

        }

        private string GenObufscatedString()
        {

            var str = "";

            for (var i = 0; i < Rand.Next(10, 20); i++)
            {

                var displacement = Rand.Next(0, 26);
                str += (char) ('a' + displacement);

            }

            return str;

        }

        // private async Task<KeyValuePair<string, string>> GetArtTumblr()
        // {
        //
        //     var offset = Rand.Next(0, Cache.FYeahYgoCardArtPosts - 20);
        //     var response = await Web.GetDeserializedContent<JObject>($"https://api.tumblr.com/v2/blog/fyeahygocardart/posts/photo?api_key={Cache.TumblrKey}&limit=20&offset={offset}");
        //     var post = response["response"]["posts"].ToObject<JArray>().RandomSubset(1).First().ToObject<YgoCardArtPost>();
        //     var key = post.Name;
        //     var value = post.Photos.First().OriginalSize.Url;
        //
        //     return new KeyValuePair<string, string>(key, value);
        //
        // }

    }
}