using System;
using System.Numerics;

namespace YuGiOh.Bot.Models;

public class HandProbability
{

    public int DeckSize { get; private init; }
    public int HandSize { get; private init; }
    public int CopiesInDeck { get; private init; }
    public int TargetCopiesInHand { get; private init; }

    private decimal _exactChance, _lessChance, _moreChance, _lessOrEqualChance, _moreOrEqualChance;

    public HandProbability(
        int deckSize,
        int handSize,
        int copiesInDeck,
        int targetCopiesInHand
    )
    {

        if (deckSize < 1)
            throw new ArgumentOutOfRangeException(nameof(deckSize));

        if (handSize > deckSize || handSize < 1)
            throw new ArgumentOutOfRangeException(nameof(handSize));

        if (copiesInDeck > deckSize || copiesInDeck < 1)
            throw new ArgumentOutOfRangeException(nameof(copiesInDeck));

        if (targetCopiesInHand > deckSize || targetCopiesInHand > copiesInDeck || targetCopiesInHand > handSize || targetCopiesInHand < 0)
            throw new ArgumentOutOfRangeException(nameof(targetCopiesInHand));

        _exactChance = _lessChance = _moreChance = _lessOrEqualChance = _moreOrEqualChance = decimal.MinusOne;

        DeckSize = deckSize;
        HandSize = handSize;
        CopiesInDeck = copiesInDeck;
        TargetCopiesInHand = targetCopiesInHand;

    }

    public decimal GetExact()
        => _exactChance != decimal.MinusOne ? _exactChance : _exactChance = Probability(TargetCopiesInHand);

    public decimal GetLessOrEqual()
    {

        if (_lessOrEqualChance != decimal.MinusOne)
            return _lessOrEqualChance;

        CalculateLessChances();

        return _lessOrEqualChance;

    }

    public decimal GetLess()
    {

        if (_lessChance != decimal.MinusOne)
            return _lessChance;

        CalculateLessChances();

        return _lessChance;

    }

    public decimal GetMoreOrEqual()
    {

        if (_moreOrEqualChance != decimal.MinusOne)
            return _moreOrEqualChance;

        CalculateMoreChances();

        return _moreOrEqualChance;

    }

    public decimal GetMore()
    {

        if (_moreChance != decimal.MinusOne)
            return _moreChance;

        CalculateMoreChances();

        return _moreChance;

    }

    private void CalculateLessChances()
    {

        _lessChance = _lessOrEqualChance = decimal.Zero;

        for (var i = 0; i <= TargetCopiesInHand; i++)
        {

            _lessOrEqualChance += Probability(i);

            if (i != TargetCopiesInHand)
                _lessChance += Probability(i);

        }

        // _lessOrEqualChance = Math.Min(100, _lessOrEqualChance);
        // _lessChance = Math.Min(100, _lessChance);

    }

    private void CalculateMoreChances()
    {

        _moreChance = _moreOrEqualChance = decimal.Zero;

        for (var i = TargetCopiesInHand; i <= HandSize; i++)
        {

            _moreOrEqualChance += Probability(i);

            if (i != TargetCopiesInHand)
                _moreChance += Probability(i);

        }

        // _moreOrEqualChance = Math.Min(100, _moreOrEqualChance);
        // _moreChance = Math.Min(100, _moreChance);

    }

    private decimal Probability(int copiesInHand)
        => HyperGeometricProbability(copiesInHand) * 100;

    private decimal HyperGeometricProbability(int copiesInHand)
    {

        var firstCombinational = Combinational(CopiesInDeck, copiesInHand);
        var secondCombinational = Combinational(DeckSize - CopiesInDeck, HandSize - copiesInHand);
        var thirdCombinational = Combinational(DeckSize, HandSize);

        return (firstCombinational * secondCombinational) / thirdCombinational;

    }

    private static decimal Combinational(int n, int k)
    {

        var numerator = Factorial(n);
        var denominator = Factorial(k) * Factorial(n - k);

        return (decimal) (numerator / denominator);

    }

    private static BigInteger Factorial(int n)
    {

        BigInteger value = 1;

        for (var i = 2; i <= n; i++)
            value *= i;

        return value;

    }

}