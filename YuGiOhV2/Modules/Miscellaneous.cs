using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Modules
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
            var result = (double)(numerator / denominator);

            return result;

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
