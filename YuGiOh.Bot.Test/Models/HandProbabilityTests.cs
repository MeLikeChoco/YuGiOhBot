using System;
using Xunit;
using YuGiOh.Bot.Models;

namespace YuGiOh.Bot.Test.Models;

public class HandProbabilityTests
{

    private const string DeckSizeParam = "deckSize";
    private const string HandSizeParam = "handSize";
    private const string CopiesInDeckParam = "copiesInDeck";
    private const string TargetCopiesInHandParam = "targetCopiesInHand";

    #region Initialization Tests

    [Theory]
    [InlineData(0, 1, 1, 1, DeckSizeParam)]
    [InlineData(1, 0, 1, 1, HandSizeParam)]
    [InlineData(1, 1, 0, 1, CopiesInDeckParam)]
    [InlineData(1, 1, 1, -1, TargetCopiesInHandParam)]
    public void InitializeObject_ValuesSmallerThanMin_ExpectFailure(
        int deckSize,
        int handSize,
        int copiesInDeck,
        int copiesInHand,
        string param
    )
    {

        Assert.Throws<ArgumentOutOfRangeException>(
            param,
            () => new HandProbability(deckSize, handSize, copiesInDeck, copiesInHand)
        );

    }

    [Fact]
    public void IntializeObject_HandLargerThanDeck_ExpectFailure()
    {

        Assert.Throws<ArgumentOutOfRangeException>(
            HandSizeParam,
            () => new HandProbability(40, 41, 1, 1)
        );

    }

    [Fact]
    public void InitializeObject_CopiesInDeckLargerThanDeck_ExpectFailure()
    {

        Assert.Throws<ArgumentOutOfRangeException>(
            CopiesInDeckParam,
            () => new HandProbability(40, 5, 41, 1)
        );

    }

    [Fact]
    public void InitializeObject_CopiesInHandLargerThanDeck_ExpectFailure()
    {

        Assert.Throws<ArgumentOutOfRangeException>(
            TargetCopiesInHandParam,
            () => new HandProbability(40, 5, 4, 41)
        );

    }

    [Fact]
    public void InitializeObject_CopiesInHandLargerThanCopiesInDeck_ExpectFailure()
    {

        Assert.Throws<ArgumentOutOfRangeException>(
            TargetCopiesInHandParam,
            () => new HandProbability(40, 5, 4, 5)
        );

    }

    [Fact]
    public void InitializeObject_CopiesInHandLargerThanHand_ExpectFailure()
    {

        Assert.Throws<ArgumentOutOfRangeException>(
            TargetCopiesInHandParam,
            () => new HandProbability(40, 5, 4, 6)
        );

    }

    [Fact]
    public void InitializeObject_ExpectSuccess()
        => Assert.NotNull(new HandProbability(40, 5, 4, 1));

    #endregion Initialization Tests

    #region Calculation Tests

    //we have to parse strings into decimals
    //xunit doesn't read the entire decimal into the variable
    //REEEEEEEEEEEEEEEEEEEEE

    [Theory]
    [InlineData(40, 5, 3, 1, "30.111336032388663967611336030")]
    [InlineData(40, 5, 6, 2, "13.641171535908378013641171540")]
    [InlineData(60, 5, 10, 1, "42.167809939811539368585109760")]
    [InlineData(52, 26, 5, 2, "32.513005202080832332933173270")]
    public void Calculate_ExactChance_ExpectSuccess(
        int deckSize,
        int handSize,
        int copiesInDeck,
        int copiesInHand,
        string expected
    )
    {

        var service = new HandProbability(deckSize, handSize, copiesInDeck, copiesInHand);
        var actual = service.GetExact();

        Assert.Equal(decimal.Parse(expected), actual);

    }

    [Theory]
    [InlineData(40, 5, 3, 1, "66.244939271255060728744939270")]
    [InlineData(40, 5, 6, 2, "84.57526352263194368457526352")]
    [InlineData(60, 5, 10, 1, "38.794385144626616219098300980")]
    [InlineData(52, 26, 5, 2, "17.486994797919167667066826730")]
    public void Calculate_GetLessChance_ExpectSuccess(
        int deckSize,
        int handSize,
        int copiesInDeck,
        int copiesInHand,
        string expected
    )
    {

        var service = new HandProbability(deckSize, handSize, copiesInDeck, copiesInHand);
        var actual = service.GetLess();

        Assert.Equal(decimal.Parse(expected), actual);

    }

    [Theory]
    [InlineData(40, 5, 3, 1, "96.35627530364372469635627530")]
    public void Calculate_GetLessOrEqualChance_ExpectSuccess(
        int deckSize,
        int handSize,
        int copiesInDeck,
        int copiesInHand,
        string expected
    )
    {

        var service = new HandProbability(deckSize, handSize, copiesInDeck, copiesInHand);
        var actual = service.GetLessOrEqual();

        Assert.Equal(decimal.Parse(expected), actual);

    }

    #endregion Calculation Tests

}