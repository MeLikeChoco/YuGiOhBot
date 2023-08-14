using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Npgsql;
using NSubstitute;
using Xunit;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Common.Repositories;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Common.Test.Tests;

public class YuGiOhRepositoryTests
{

    // private const string GetArchetypesSql = "select archetypes.name from archetypes inner join card_to_archetypes cta on archetypes.id = cta.archetypesid inner join cards c on c.archetypes = cta.cardarchetypesid where c.id = @id";
    // private const string GetSupportsSql = "select supports.name from supports inner join card_to_supports cts on supports.id = cts.supportsid inner join cards c on cts.cardsupportsid = c.supports where c.id = @id";
    // private const string GetAntiSupportsSql = "select antisupports.name from antisupports inner join card_to_antisupports cta on antisupports.id = cta.antisupportsid inner join cards c on c.antisupports = cta.cardantisupportsid where c.id = @id";
    // private const string GetTranslationsSql = "select translations.* from translations inner join cards c on translations.cardid = c.id where c.id = @id";

    private readonly IYuGiOhRepository _yugiohRepo;

    public YuGiOhRepositoryTests()
    {

        var mockRepoConfig = Substitute.For<IYuGiOhRepositoryConfiguration>();

        mockRepoConfig.GetYuGiOhDbConnection().Returns(_ => new NpgsqlConnection(Config.Instance.GetDbConnectionStrings().YuGiOh));

        _yugiohRepo = new YuGiOhRepository(mockRepoConfig);

    }

    // [Theory]
    // [CardEntityData("InsertCardAsync_SimpleInsert_ExpectSuccess_Data.json")]
    // public async Task InsertCardAsync_SimpleInsert_ExpectSuccess(CardEntity expected)
    // {
    //
    //     await _yugiohRepo.InsertCardAsync(expected);
    //
    //     await using var connection = new NpgsqlConnection(Config.Instance.DbConnectionString.YuGiOh);
    //
    //     var actual = await connection.QuerySingleAsync<CardEntity>("select * from cards where id = @id", new { id = expected.Id });
    //
    //     AssertCardEntityEquality(expected, actual, false);
    //
    //     actual.Archetypes = await connection.QueryAsync<string>(GetArchetypesSql, new { id = actual.Id }).ContinueWith(result => result.Result.ToList());
    //
    //     expected.Archetypes.Should().BeEquivalentTo(actual.Archetypes);
    //
    //     actual.Supports = await connection.QueryAsync<string>(GetSupportsSql, new { id = actual.Id }).ContinueWith(result => result.Result.ToList());
    //
    //     expected.Supports.Should().BeEquivalentTo(actual.Supports);
    //
    //     actual.AntiSupports = await connection.QueryAsync<string>(GetAntiSupportsSql, new { id = actual.Id }).ContinueWith(result => result.Result.ToList());
    //
    //     expected.AntiSupports.Should().BeEquivalentTo(actual.AntiSupports);
    //
    //     actual.Translations = await connection.QueryAsync<TranslationEntity>(GetTranslationsSql, new { id = actual.Id }).ContinueWith(result => result.Result.ToList());
    //
    //     expected.Translations.Select(translation => translation.CardId).Should().BeEquivalentTo(actual.Translations.Select(translation => translation.CardId));
    //     expected.Translations.Select(translation => translation.Language).Should().BeEquivalentTo(actual.Translations.Select(translation => translation.Language));
    //     expected.Translations.Select(translation => translation.Name).Should().BeEquivalentTo(actual.Translations.Select(translation => translation.Name));
    //     expected.Translations.Select(translation => translation.Lore).Should().BeEquivalentTo(actual.Translations.Select(translation => translation.Lore));
    //
    //     await connection.ExecuteAsync("delete from cards where id = @id", new { id = actual.Id });
    //
    //     foreach (var archetype in expected.Archetypes)
    //         await connection.ExecuteAsync("delete from archetypes where name = @name", new { name = archetype });
    //
    //     foreach (var support in expected.Supports)
    //         await connection.ExecuteAsync("delete from supports where name = @name", new { name = support });
    //
    //     foreach (var antiSupport in expected.AntiSupports)
    //         await connection.ExecuteAsync("delete from antisupports where name = @name", new { name = antiSupport });
    //
    // }

    [Theory]
    [InlineData("Raidraptor - Revolution Falcon - Air Raid")]
    [InlineData("D/D/D Deviser King Deus Machinex")]
    [InlineData("Astrograph Sorcerer")]
    public async Task GetCardAsync_ExpectSuccess(string input)
    {

        var result = await _yugiohRepo.GetCardAsync(input);

        Assert.NotNull(result);

    }

    [Theory]
    [InlineData("book")]
    [InlineData("ghost")]
    [InlineData("dragon")]
    public async Task SearchCardsAsync_ExpectNotEmpty_ExpectSuccess(string input)
    {

        var result = await _yugiohRepo.SearchCardsAsync(input);

        Assert.NotEmpty(result);

    }

    [Theory]
    [InlineData("number 1")]
    [InlineData("Red")]
    [InlineData("plANeT")]
    public async Task SearchAnimeCardsAsync_ExpectNotEmpty_ExpectSuccess(string input)
    {

        var result = await _yugiohRepo.SearchAnimeCardsAsync(input);

        Assert.NotEmpty(result);

    }

    [Theory]
    [InlineData("black assault the rain")]
    [InlineData("blue dragon white eyes")]
    [InlineData("le an at l te")]
    public async Task GetCardsContainsAllAsync_ExpectNotEmpty_ExpectSuccess(string input)
    {

        var result = await _yugiohRepo.GetCardsContainsAllAsync(input);

        Assert.NotEmpty(result);

    }

    [Theory]
    [InlineData("awakening of the possdessed - nefariouser archfiend")]
    [InlineData("carpiponjca, mystical beast of the forest")]
    [InlineData("Karakuri Steel SahOgun mdl 00x \"Bureido\"")]
    public async Task GetCardFuzzyAsync_ExpectSuccess(string input)
    {

        var result = await _yugiohRepo.GetCardFuzzyAsync(input);

        Assert.NotNull(result);

    }

    [Fact]
    public async Task GetRandomCardAsync_ExpectSuccess()
    {

        var result = await _yugiohRepo.GetRandomCardAsync();

        Assert.NotNull(result);

    }

    [Fact]
    public async Task GetRandomMonsterAsync_ExpectSuccess()
    {

        var result = await _yugiohRepo.GetRandomMonsterAsync();

        Assert.NotNull(result);

    }

    [Theory]
    [InlineData("elemental hero")]
    [InlineData("Infernity")]
    [InlineData("SyNcHrOn")]
    public async Task GetCardsInArchetype_ExpectNotEmpty_ExpectSuccess(string input)
    {

        var results = await _yugiohRepo.GetCardsInArchetypeAsync(input);

        Assert.NotEmpty(results);

    }

    [Theory]
    [InlineData("dark")]
    [InlineData("cYbErSe")]
    [InlineData("Hamon, Lord of Striking Thunder")]
    public async Task GetCardsInSupport_ExpectNotEmpty_ExpectSuccess(string input)
    {

        var results = await _yugiohRepo.GetCardsInSupportAsync(input);

        Assert.NotEmpty(results);

    }

    [Theory]
    [InlineData("Synchro Monster")]
    [InlineData("flIp moNsTeR")]
    [InlineData("equip card")]
    public async Task GetCardsInAntisupport_ExpectNotEmpty_ExpectSuccess(string input)
    {

        var results = await _yugiohRepo.GetCardsInAntisupportAsync(input);

        Assert.NotEmpty(results);

    }

    [Theory]
    [InlineData("number")]
    [InlineData("PeRFoRmApAL")]
    [InlineData("dragon")]
    public async Task GetCardsAutocompleteAsync_TypingSimulation_ExpectSuccess(string input)
    {

        for (var i = 0; i < input.Length; i++)
        {

            var parameter = input[..(i + 1)];
            var names = await _yugiohRepo.GetCardsAutocompleteAsync(parameter);

            AssertCollectionStartsWithThenOrdered(names, parameter);

        }

    }

    [Theory]
    [InlineData("genex")]
    [InlineData("WaRrioR")]
    [InlineData("drAiN")]
    public async Task GetArchetypesAutocompleteAsync_TypingSimulation_ExpectSuccess(string input)
    {

        for (var i = 0; i < input.Length; i++)
        {

            var parameter = input[..(i + 1)];
            var names = await _yugiohRepo.GetArchetypesAutocompleteAsync(parameter);

            AssertCollectionStartsWithThenOrdered(names, parameter);

        }

    }

    [Theory]
    [InlineData("FIEND")]
    [InlineData("Fusion Material")]
    [InlineData("number 39: utopia")]
    public async Task GetSupportsAutocompleteAsync_TypingSimulation_ExpectSuccess(string input)
    {

        for (var i = 0; i < input.Length; i++)
        {

            var parameter = input[..(i + 1)];
            var names = await _yugiohRepo.GetSupportsAutocompleteAsync(parameter);

            AssertCollectionStartsWithThenOrdered(names, parameter);

        }

    }

    [Theory]
    [InlineData("kaiju")]
    [InlineData("Dinosaur")]
    [InlineData("fISh")]
    public async Task GetAntisupportsAutocompleteAsync_TypingSimulation_ExpectSuccess(string input)
    {

        for (var i = 0; i < input.Length; i++)
        {

            var parameter = input[..(i + 1)];
            var names = await _yugiohRepo.GetAntisupportsAutocompleteAsync(parameter);

            AssertCollectionStartsWithThenOrdered(names, parameter);

        }

    }

    [Theory]
    [InlineData("dragon")]
    [InlineData("Harpie")]
    [InlineData("mAgiCiAN")]
    public async Task GetAnimeCardsAutocompleteAsync_TypingSimulation_ExpectSuccess(string input)
    {

        for (var i = 0; i < input.Length; i++)
        {

            var parameter = input[..(i + 1)];
            var names = await _yugiohRepo.GetAnimeCardsAutocompleteAsync(parameter);

            AssertCollectionStartsWithThenOrdered(names, parameter);

        }

    }

    [Fact]
    public async Task GetNameWithPasscodeAsync_ExpectSuccess()
    {

        const string passcode = "11759079";
        const string expected = "Dual Avatar Feet - Kokoku";
        var actual = await _yugiohRepo.GetNameWithPasscodeAsync(passcode);

        Assert.Equal(expected, actual);

    }

    [Fact]
    public async Task GetImageLinkAsync_NotNullOrEmpty_ExpectSuccess()
    {

        const string name = "Crystal Girl";
        var link = await _yugiohRepo.GetImageLinkAsync(name);

        Assert.True(!string.IsNullOrWhiteSpace(link));

    }

    [Theory]
    [InlineData(BanlistFormats.OCG)]
    [InlineData(BanlistFormats.TCG)]
    public async Task GetBanlistAsync_ExpectSuccess(BanlistFormats format)
    {

        var result = await _yugiohRepo.GetBanlistAsync(format);

        Assert.NotEmpty(result.Forbidden);
        Assert.NotEmpty(result.Limited);
        Assert.NotEmpty(result.SemiLimited);

    }

    // [Fact]
    // public async Task GetBanlistAsync_ExpectTradSemiLimitedNotEmpty_ExpectSuccess()
    // {
    //
    //     var result = await _yugiohRepo.GetBanlistAsync(BanlistFormats.TRAD);
    //
    //     Assert.NotEmpty(result.Limited);
    //
    // }

    [Theory]
    [InlineData("Duelist")]
    [InlineData("tOUrNaMenT")]
    [InlineData("premium")]
    public async Task GetBoosterPacksAutocompleteAsync_TypingSimulation_ExpectSuccess(string input)
    {

        for (var i = 0; i < input.Length; i++)
        {

            var parameter = input[..(i + 1)];
            var names = await _yugiohRepo.GetBoosterPacksAutocompleteAsync(parameter);

            AssertCollectionStartsWithThenOrdered(names, parameter);

        }

    }

    private static void AssertCardEntityEquality(CardEntity expected, CardEntity actual, bool compareCollections = true)
    {

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.RealName, actual.RealName);
        Assert.Equal(expected.Lore, actual.Lore);
        Assert.Equal(expected.Atk, actual.Atk);
        Assert.Equal(expected.Def, actual.Def);
        Assert.Equal(expected.Link, actual.Link);
        Assert.Equal(expected.LinkArrows, actual.LinkArrows);
        Assert.Equal(expected.Level, actual.Level);
        Assert.Equal(expected.PendulumScale, actual.PendulumScale);
        Assert.Equal(expected.Rank, actual.Rank);
        Assert.Equal(expected.TcgExists, actual.TcgExists);
        Assert.Equal(expected.OcgExists, actual.OcgExists);
        Assert.Equal(expected.Img, actual.Img);
        Assert.Equal(expected.Url, actual.Url);
        Assert.Equal(expected.Passcode, actual.Passcode);

        if (!compareCollections)
            return;

        expected.Archetypes.Should().BeEquivalentTo(actual.Archetypes);
        expected.Supports.Should().BeEquivalentTo(actual.Supports);
        expected.AntiSupports.Should().BeEquivalentTo(actual.AntiSupports);

    }

    //Make sure collection starts with strings that start with "startsWith" then ensure the rest of the collection contains "startsWith"
    //We cannot order it in C# then assert because the order is based on the database/locale
    //ex. Debian and Alpine operate with different locale systems, so both will have different outcomes based on their string comparisons
    //To separate environment and code, we just make sure the collection starts with strings that start with "startsWith"
    private static void AssertCollectionStartsWithThenOrdered(IEnumerable<string> names, string startsWith)
    {

        var shouldStartWith = true;

        foreach (var name in names)
        {

            if (!shouldStartWith)
                Assert.Contains(startsWith, name, StringComparison.OrdinalIgnoreCase);
            else
            {

                try
                {
                    Assert.StartsWith(startsWith, name, StringComparison.OrdinalIgnoreCase);
                }
                catch (Exception)
                {
                    shouldStartWith = false;
                }

            }

        }

    }

}