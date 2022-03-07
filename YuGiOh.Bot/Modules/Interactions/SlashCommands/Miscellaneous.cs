using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Modules.Interactions.Autocompletes;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands;

public class Miscellaneous : MainInteractionBase<SocketSlashCommand>
{

    private readonly Random _random;

    public Miscellaneous(
        ILoggerFactory loggerFactory,
        Cache cache,
        IYuGiOhDbService yuGiOhDbService,
        IGuildConfigDbService guildConfigDbService,
        Web web,
        Random random
    ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web)
    {
        _random = random;
    }

    [SlashCommand("probability", "Gets the chance of a card appearing in your opening hand")]
    public Task ProbabilityCommand(
        [Summary(description: "deck size")] int deckSize,
        [Summary(description: "copies in deck")]
        int inDeck,
        [Summary(description: "hand size")]
        int handSize,
        [Summary(description: "copies you want in hand")]
        int inHand
    )
    {

        if (inDeck > deckSize)
            return RespondAsync($"There are more cards in deck `({inDeck})` than the deck size `({inHand})`!");

        if (handSize > deckSize)
            return RespondAsync($"The hand is larger `({handSize})` than the deck size `({deckSize})`!");

        if (inHand > deckSize)
            return RespondAsync($"There are more copies of the card in hand `({inHand})` than the deck size `({deckSize})`!");

        if (inHand > inDeck)
            return RespondAsync($"There are more copies of the card in hand `({inHand})` than the copies in deck `({inDeck})`!");

        if (inHand > handSize)
            return RespondAsync($"There are more cards in hand `({inHand})` than the hand size `({handSize})`!");

        try
        {

            var exactly = Probability(deckSize, inDeck, handSize, inHand);
            double more, lessequal, moreequal;
            var less = more = lessequal = moreequal = 0;

            for (var i = inHand; i >= 0; i--)
            {

                lessequal += Probability(deckSize, inDeck, handSize, i);

                if (i != inHand)
                    less += Probability(deckSize, inDeck, handSize, i);

            }

            for (var i = inHand; i < handSize; i++)
            {

                moreequal += Probability(deckSize, inDeck, handSize, i);

                if (i != inHand)
                    more += Probability(deckSize, inDeck, handSize, i);

            }

            var display = $"{deckSize} cards in deck\n" +
                          $"{inDeck} copies in deck\n" +
                          $"{handSize} cards in hand\n" +
                          $"{inHand} copies in hand\n" +
                          $"Exactly {inHand} in hand: {exactly}%\n" +
                          $"Less than {inHand} in hand: {less}%\n" +
                          $"Less or equal to {inHand} in hand: {lessequal}%\n" +
                          $"More than {inHand} in hand: {more}%\n" +
                          $"More or equal to {inHand} in hand: {moreequal}%";

            if (inHand == 1)
                return RespondAsync($"```{display}```");

            double onetoInHand = 0;

            for (var i = inHand; i >= 1; i--)
                onetoInHand += Probability(deckSize, inDeck, handSize, i);

            display += $"\n1-{inHand} copies in hand: {onetoInHand}%";

            return RespondAsync($"```{display}```");

        }
        catch
        {
            return RespondAsync("There was an error. Please check your values and try again!\nEx. `y!prob 40 7 5 2`");
        }

    }

    #region Probability

    private static double HyperGeometricProbability(
        int deckSize,
        int inDeck,
        int handSize,
        int inHand
    )
    {

        var firstCombinational = Combinational(inDeck, inHand);
        var secondCombinational = Combinational(deckSize - inDeck, handSize - inHand);
        var thirdCombinational = Combinational(deckSize, handSize);

        return (firstCombinational * secondCombinational) / thirdCombinational;

    }

    private static double Probability(
        int deckSize,
        int inDeck,
        int handSize,
        int inHand
    )
        => HyperGeometricProbability(deckSize, inDeck, handSize, inHand) * 100;

    private static double Combinational(int n, int k)
    {

        var numerator = Factorial(n);
        var denominator = Factorial(k) * Factorial(n - k);
        return (double) (numerator / denominator);

    }

    //who needs memory amirite
    private static BigInteger Factorial(int n)
    {

        var value = (BigInteger) 1;

        for (uint i = 2; i <= n; i++)
            value *= i;

        return value;

    }

    #endregion Probability

    [SlashCommand("booster", "Gets information on a booster pack")]
    public async Task BoosterCommand([Autocomplete(typeof(BoosterPackAutocomplete)), Summary(description: "The boosterpack")] string input)
    {

        var boosterPack = await YuGiOhDbService.GetBoosterPackAsync(input);

        if (boosterPack is not null)
        {

            var descBuilder = new StringBuilder()
                .Append("**Amount:** ")
                .Append(boosterPack.Cards.Count)
                .AppendLine(" cards")
                .AppendLine()
                .AppendLine("**Release dates**");

            descBuilder = boosterPack.Dates
                .Aggregate(descBuilder, (current, date) =>
                    current.Append("**")
                        .Append(date.Name)
                        .Append(":** ")
                        .AppendFormat("{0: MM/dd/yyyy}", date.Date)
                        .AppendLine()
                );

            var options = PagedOptions;
            options.FieldsPerPage = 1;

            var paginator = new PaginatedMessage()
            {

                Title = boosterPack.Name,
                Color = _random.NextColor(),
                AlternateDescription = descBuilder.ToString(),
                Options = options

            };

            var pages = new LinkedList<EmbedFieldBuilder>();
            paginator.Pages = pages;
            var rarityToCards = new Dictionary<string, List<string>>();

            foreach (var card in boosterPack.Cards)
            {

                foreach (var rarity in card.Rarities)
                {

                    if (!rarityToCards.ContainsKey(rarity))
                        rarityToCards[rarity] = new List<string>();

                    rarityToCards[rarity].Add(card.Name);

                }

            }

            foreach (var (rarity, card) in rarityToCards)
            {

                var cards = card
                    .Aggregate(new StringBuilder(), (current, next) => current.AppendLine(next))
                    .ToString();

                while (cards.Length >= 1024)
                {

                    const int maxLength = 1000;

                    var substring = cards[..maxLength];
                    var cutoff = substring.LastIndexOf('\n');
                    substring = cards[..cutoff];
                    var cardsField = $"```{substring}```";

                    pages.AddLast(new EmbedFieldBuilder()
                        .WithName(rarity)
                        .WithValue(cardsField));

                    if (cards.Length >= maxLength)
                        cards = cards[cutoff..];

                }

                if (!string.IsNullOrEmpty(cards) || !string.IsNullOrWhiteSpace(cards))
                {

                    pages.AddLast(new EmbedFieldBuilder()
                        .WithName(rarity)
                        .WithValue($"```{cards}```"));

                }

            }

            await PagedComponentReplyAsync(paginator);

        }
        else
            await NoResultError("booster packs", input);

    }

}