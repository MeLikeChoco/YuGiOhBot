using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Criterion;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Games : MainInteractionBase<SocketSlashCommand>
    {

        private readonly ILoggerFactory _loggerFactory;
        private readonly Random _random;

        private static Criteria<SocketMessage> BaseCriteria
            => new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new NotBotCriteria());

        public Games(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random random
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web)
        {
            _loggerFactory = loggerFactory;
            _random = random;
        }

        [SlashCommand("guess", "Starts an image/card guessing game!")]
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
                        var url = $"{Constants.ArtBaseUrl}{card.Passcode}.{Constants.ArtFileType}";
                        var name = $"{card.Name}";

                        if (!string.IsNullOrEmpty(card.RealName))
                            name += $" / {card.RealName}";

                        logger.Info(name);
                        logger.Info($"{Constants.ArtBaseUrl}{card.Passcode}.{Constants.ArtFileType}");

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
                var answer = await NextMessageAsync(BaseCriteria.AddCriterion(criteria), TimeSpan.FromSeconds(GuildConfig.GuessTime));

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
                int displacement;

                switch (_random.Next(0, 3))
                {

                    case 0:
                        startingChar = 'a';
                        displacement = _random.Next(0, 26);
                        break;
                    case 1:
                        startingChar = 'A';
                        displacement = _random.Next(0, 26);
                        break;
                    default:
                        startingChar = '0';
                        displacement = _random.Next(0, 10);
                        break;

                }

                str += (char) (startingChar + displacement);

            }

            return str;

        }

    }
}