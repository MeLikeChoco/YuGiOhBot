using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Attributes;
using YuGiOh.Bot.Models.BoosterPacks;

namespace YuGiOh.Bot.Modules
{
    public class Miscellaneous : MainBase
    {

        [Command("probability"), Alias("prob")]
        [Summary("Returns the chance of your hand occuring!")]
        public Task ProbabilityCommand(int deckSize, int inDeck, int handSize, int inHand)
        {

            if (inDeck > deckSize)
                return ReplyAsync($"There are more cards in deck `({inDeck})` than the deck size `({inHand})`!");

            if (handSize > deckSize)
                return ReplyAsync($"The hand is larger `({handSize})` than the deck size `({deckSize})`!");

            if (inHand > deckSize)
                return ReplyAsync($"There are more copies of the card in hand `({inHand})` than the deck size `({deckSize})`!");

            if (inHand > inDeck)
                return ReplyAsync($"There are more copies of the card in hand `({inHand})` than the copies in deck `({inDeck})`!");

            if (inHand > handSize)
                return ReplyAsync($"There are more cards in hand `({inHand})` than the hand size `({handSize})`!");

            try
            {

                var exactly = Probability(deckSize, inDeck, handSize, inHand);
                double less, more, lessequal, moreequal;
                less = more = lessequal = moreequal = 0;

                for (int i = inHand; i >= 0; i--)
                {

                    lessequal += Probability(deckSize, inDeck, handSize, i);

                    if (i != inHand)
                        less += Probability(deckSize, inDeck, handSize, i);

                }

                for (int i = inHand; i < handSize; i++)
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

                if (inHand != 1)
                {

                    double onetoInHand = 0;

                    for (int i = inHand; i >= 1; i--)
                        onetoInHand += Probability(deckSize, inDeck, handSize, i);

                    display += $"\n1-{inHand} copies in hand: {onetoInHand}%";

                }

                return ReplyAsync($"```{display}```");

            }
            catch { return ReplyAsync("There was an error. Please check your values and try again!\nEx. `y!prob 40 7 5 2`"); };


        }

        [Command("booster")]
        [Summary("Gets information on a booster pack!")]
        public Task BoosterCommand([Remainder] string input)
        {

            if (!Cache.BoosterPacks.TryGetValue(input, out BoosterPack boosterPack))
            {

                var inputWords = input.Split(' ');
                boosterPack = Cache.BoosterPacks.FirstOrDefault(kv => inputWords.All(word => kv.Key.Contains(word, StringComparison.OrdinalIgnoreCase))).Value;

            }

            if (boosterPack != null)
            {

                var descBuilder = new StringBuilder()
                    .Append("**Amount:** ").Append(boosterPack.Cards.Length).AppendLine(" cards")
                    .AppendLine()
                    .AppendLine("**Release dates**");

                descBuilder = boosterPack.ReleaseDates
                    .Aggregate(descBuilder, (current, kv) =>
                        current.Append("**").Append(kv.Key).Append(":** ")
                        .AppendFormat("{0: MM/dd/yyyy}", kv.Value)
                        .AppendLine()
                    );

                var options = PagedOptions;
                options.FieldsPerPage = 1;

                var paginator = new PaginatedMessage()
                {

                    Title = boosterPack.Name,
                    Color = Rand.NextColor(),
                    AlternateDescription = descBuilder.ToString(),
                    Options = options

                };

                var pages = new LinkedList<EmbedFieldBuilder>();
                paginator.Pages = pages;

                foreach (var kv in boosterPack.RarityToCards)
                {

                    var cards = kv.Value
                        .Aggregate(new StringBuilder(), (current, next) => current.AppendLine(next))
                        .ToString();

                    while (cards.Length >= 1024)
                    {

                        const int maxLength = 1000;

                        var substring = cards.Substring(0, maxLength);
                        var cutoff = substring.LastIndexOf('\n');
                        substring = cards.Substring(0, cutoff);
                        var cardsField = $"```{substring}```";

                        pages.AddLast(new EmbedFieldBuilder()
                            .WithName(kv.Key)
                            .WithValue(cardsField));

                        if (cards.Length >= maxLength)
                            cards = cards[cutoff..];

                    }

                    if (!string.IsNullOrEmpty(cards) || !string.IsNullOrWhiteSpace(cards))
                    {

                        pages.AddLast(new EmbedFieldBuilder()
                            .WithName(kv.Key)
                            .WithValue($"```{cards}```"));

                    }

                }

                return PagedReplyAsync(paginator);

                //var builder = new EmbedBuilder()
                //    .WithAuthor(boosterPack.Name, url: boosterPack.Url)
                //    .WithDescription($"**Amount:** {boosterPack.Cards.Length} cards")
                //    .WithColor(Rand.NextColor())
                //    .AddField("Release dates", boosterPack.ReleaseDates.Aggregate("", (current, kv) => $"{current}\n**{kv.Key}:** {kv.Value: MM/dd/yyyy}"));

                //foreach (var kv in boosterPack.RarityToCards)
                //{

                //    var cards = kv.Value.Aggregate(new StringBuilder(), (current, next) => current.AppendLine(next)).ToString();

                //    if (cards.Length >= 1024)
                //    {

                //        do
                //        {

                //            const int maxLength = 1000;

                //            var substring = cards.Substring(0, maxLength);
                //            var cutoff = substring.LastIndexOf('\n');
                //            substring = cards.Substring(0, cutoff);
                //            var cardsField = $"```{substring}```";

                //            builder.AddField(kv.Key, cardsField);

                //            if (cards.Length >= maxLength)
                //                cards = cards[cutoff..];

                //        } while (cards.Length >= 1024);

                //    }

                //    builder.AddField(kv.Key, $"```{cards}```");

                //}

                //return SendEmbed(builder);

            }
            else
                return NoResultError("booster packs", input);

        }

        [Command("open")]
        [RequireChannel(410082506935894016)]
        public async Task OpenCommand([Remainder] string input)
        {

            try
            {

                if (!Cache.BoosterPacks.TryGetValue(input, out BoosterPack boosterPack))
                {

                    var inputWords = input.Split(' ');
                    boosterPack = Cache.BoosterPacks
                        .Where(kv => inputWords.All(word => kv.Key.Contains(word, StringComparison.OrdinalIgnoreCase)))
                        .ToList()
                        .PartialSortBy(1, kv => kv.Key)
                        .FirstOrDefault()
                        .Value;

                }

                if (boosterPack != null)
                {

                    var cards = new Dictionary<string, string>(9);
                    var randoms = new List<int>(9);
                    var commonCards = boosterPack.Commons.Length;
                    var builder = new StringBuilder("```fix\n");
                    int index;

                    for (int i = 0; i < 7; i++)
                    {

                        do
                            index = Rand.Next(commonCards);
                        while (randoms.Contains(index));

                        cards.Add(boosterPack.Commons[index], "Common");
                        randoms.Add(index);

                    }

                    var superRare = boosterPack.Foils.RandomSubset(1, Rand).FirstOrDefault();

                    cards.Add(boosterPack.Rares.RandomSubset(1, Rand).FirstOrDefault(), "Rare");
                    cards.Add(superRare.Value.RandomSubset(1, Rand).FirstOrDefault(), superRare.Key);

                    foreach (var card in cards)
                    {

                        builder.Append("Name: ").AppendLine(card.Key);
                        builder.Append("Rarity: ").AppendLine(card.Value);
                        builder.AppendLine();

                    }

                    builder.Append("```");

                    await ReplyAsync(builder.ToString());

                    return;

                }
                else
                    await NoResultError("booster packs", input);

            }
            catch
            {
                await ReplyAsync("There was an error opening the booster pack. This is most likely due to unknown ratios or the pack being unique (ex. gold rare only pack). This problem is temporary and will be fixed soon™.");

                return;
            }

        }

        public double HyperGeometricProbability(int deckSize, int inDeck, int handSize, int inHand)
        {

            var firstCombinational = Combinational(inDeck, inHand);
            var secondCombinational = Combinational(deckSize - inDeck, handSize - inHand);
            var thirdCombinational = Combinational(deckSize, handSize);

            return (firstCombinational * secondCombinational) / thirdCombinational;

        }

        public double Probability(int deckSize, int inDeck, int handSize, int inHand)
            => HyperGeometricProbability(deckSize, inDeck, handSize, inHand) * 100;

        public double Combinational(int n, int k)
        {

            var numerator = Factorial(n);
            var denominator = Factorial(k) * Factorial(n - k);
            return (double)(numerator / denominator);

        }

        //who needs memory amirite
        public BigInteger InsaneFactorial(int value)
        {

            if (value == 2)
                return value;
            else
                return value * InsaneFactorial(value - 1);

        }

        public BigInteger Factorial(int n)
        {

            var value = (BigInteger)1;

            for (uint i = 2; i <= n; i++)
                value *= i;

            return value;

        }

    }
}
