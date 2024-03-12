using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Criteria;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Games : MainInteractionBase<SocketSlashCommand>
    {

        private readonly ILoggerFactory _loggerFactory;
        private readonly Random _random;

        private Criteria BaseCriteria
            => new Criteria()
                .AddCriteria(new ChannelCriteria(Context.Channel))
                .AddCriteria(new NotBotCriteria());

        // private static Criteria<SocketMessage> BaseCriteria
        //     => new Criteria<SocketMessage>()
        //         .AddCriterion(new EnsureSourceChannelCriterion())
        //         .AddCriterion(new NotBotCriteria());

        public Games(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random random,
            InteractiveService interactiveService
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, interactiveService)
        {
            _loggerFactory = loggerFactory;
            _random = random;
        }

        [SlashCommand("hangman", "Starts a hangman game")]
        public async Task HangmanCommand()
        {

            if (Cache.HangmanInProgress.ContainsKey(Context.Channel.Id))
            {

                await RespondAsync(":game_die: There is a game in progress in this channel!");

                return;

            }

            var logger = _loggerFactory.CreateLogger("Hangman");

            Cache.HangmanInProgress[Context.Channel.Id] = null;

            try
            {
                var card = await YuGiOhDbService.GetRandomCardAsync();

                logger.Info(card.Name);

                var hangmanService = new Hangman(card.Name);

                var criteria = BaseCriteria
                    .AddCriteria(new NotCommandCriteria(GuildConfig))
                    .AddCriteria(new NotInlineSearchCriteria());

                if (!GuildConfig.HangmanAllowWords)
                    criteria.AddCriteria(new CharacterOnlyCriteria());

                var time = TimeSpan.FromSeconds(GuildConfig.HangmanTime);

                await RespondAsync("You can now type more than a letter for hangman!\n" +
                                   $"As well as change the hangman time ({GuildConfig.Prefix}hangmantime <seconds>)! Ask an admin about it!\n" +
                                   $"You may also disable the ability to input more than one letter! ({GuildConfig.Prefix}hangmanwords <true/false>)\n" +
                                   $"You have **{time.ToPrettyString()}**!\n" +
                                   hangmanService.GetCurrentDisplay());

                SocketUser user = null;
                InteractiveResult<SocketMessage> input;

                //todo move this logic into HangmanService
                do
                {

                    input = await NextMessageAsync(criteria, time);

                    if (input.IsCanceled || input.IsTimeout)
                        break;

                    user = input.Value!.Author;
                    var message = input.Value.Content;

                    switch (hangmanService.AddGuess(message))
                    {

                        case GuessStatus.Duplicate:
                            await ReplyAsync($"You already guessed `{message}`!\n" +
                                             hangmanService.GetCurrentDisplay());
                            break;
                        case GuessStatus.Nonexistent:
                            await ReplyAsync($"```fix\n{hangmanService.GetHangman()}```\n" +
                                             hangmanService.GetCurrentDisplay());
                            break;
                        case GuessStatus.Accepted:
                            await ReplyAsync(hangmanService.GetCurrentDisplay());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                } while (hangmanService.CompletionStatus == CompletionStatus.Incomplete);

                if (input.IsTimeout)
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
                logger.Error(ex, "There was a problem with hangman");
            }
            finally
            {
                Cache.HangmanInProgress.TryRemove(Context.Channel.Id, out _);
            }

        }

        [SlashCommand("guess", "Starts a card guessing game")]
        public async Task GuessCommand()
        {

            if (Cache.GuessInProgress.ContainsKey(Context.Channel.Id))
            {

                await RespondAsync(":game_die: There is a game in progress!");

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
                            await UploadAsync(stream, $"{GetConfusingString()}.{Constants.ArtFileType}", $":stopwatch: You have **{GuildConfig.GuessTime}** seconds to guess what card this art belongs to! Case insensitive (used to be case sensitive)!");

                        e = null;

                    }
                    catch (NullReferenceException nullref)
                    {
                        e = nullref;
                    }

                } while (e is not null);

                var criteria = new GuessCriteria(card.Name, card.RealName);
                var input = await NextMessageAsync(BaseCriteria.AddCriteria(criteria), TimeSpan.FromSeconds(GuildConfig.GuessTime));

                if (input.Value is not null)
                {
                    await ReplyAsync($":trophy: The winner is **{(input.Value.Author as SocketGuildUser)?.Nickname ?? input.Value.Author.Username}**! The card was `{criteria.Answer}`!");
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
                logger.Error("There was a problem with guess!", ex);
            }
            finally
            {
                Cache.GuessInProgress.TryRemove(Context.Channel.Id, out _);
            }

        }

        //BEHOLD, THIS WONDERFULLY OVERENGINEERED STRING OBFUSCATION
        private string GetConfusingString()
        {

            var str = "";

            for (var i = 0; i < _random.Next(10, 20); i++)
            {

                char startingChar;
                int offset;

                switch (_random.Next(0, 3))
                {

                    case 0:
                        startingChar = 'a';
                        offset = _random.Next(0, 26);
                        break;
                    case 1:
                        startingChar = 'A';
                        offset = _random.Next(0, 26);
                        break;
                    default:
                        startingChar = '0';
                        offset = _random.Next(0, 10);
                        break;

                }

                str += (char)(startingChar + offset);

            }

            return str;

        }

    }
}