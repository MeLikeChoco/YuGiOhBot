using System.Threading.Tasks;
using Dapper;
using Dommel;
using FluentAssertions;
using Moq;
using Npgsql;
using Xunit;
using YuGiOh.Common.Interfaces;
using YuGiOh.Common.Models;
using YuGiOh.Common.Repositories;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Common.Test;

public class GuildConfigsRepository
{

    private const decimal ExpectedId = 11101000101001;

    private readonly IGuildConfigRepository _guildConfigRepo;

    private static GuildConfigEntity Expected => new GuildConfigEntity
    {
        Id = ExpectedId,
        Prefix = ":)",
        Minimal = false,
        GuessTime = 1,
        AutoDelete = true,
        Inline = false,
        HangmanTime = 10000000,
        HangmanAllowWords = false
    };

    public GuildConfigsRepository()
    {

        var mockRepoConfig = new Mock<IGuildConfigConfiguration>();

        mockRepoConfig
            .Setup(repoConfig => repoConfig.GetGuildConfigConnection())
            .Returns(() => new NpgsqlConnection(Config.Instance.GetDbConnectionStrings().Guilds));

        _guildConfigRepo = new GuildConfigRepository(mockRepoConfig.Object);

    }

    [Fact]
    public async Task GetGuildConfigAsync_ExpectSuccess()
    {

        var actual = await _guildConfigRepo.GetGuildConfigAsync((ulong) ExpectedId);

        Expected.Should().BeEquivalentTo(actual);

    }

    [Fact]
    public async Task InsertGuildConfigAsync_ExpectSuccess()
    {

        var expected = new GuildConfigEntity { Id = 1337420 }; //HAHa FUNNy NUmbeR *insert mocking spongebob meme

        await _guildConfigRepo.InsertGuildConfigAsync(expected);

        await using var connection = new NpgsqlConnection(Config.Instance.GetDbConnectionStrings().Guilds);

        var actual = await connection.ExecuteScalarAsync<decimal>("select id from configs where id = @id", new { id = expected.Id });

        Assert.Equal(expected.Id, actual);

        await connection.DeleteAsync(expected);

    }

    [Fact]
    public async Task UpdateGuildConfigAsync_ExpectSuccess()
    {

        var expected = Expected;

        expected.Prefix = ":(";
        expected.GuessTime = 10000000;
        expected.HangmanTime = 0;

        await _guildConfigRepo.UpdateGuildConfigAsync(expected);

        await using var connection = new NpgsqlConnection(Config.Instance.GetDbConnectionStrings().Guilds);
        //dont use deconstruction for intent/readability
        // ReSharper disable once UseDeconstruction
        var actual = await connection.QuerySingleAsync<(string prefix, int guessTime, int hangmanTime)>("select prefix, guesstime, hangmantime from configs where id = @id", new { id = expected.Id });

        Assert.Equal(expected.Prefix, actual.prefix);
        Assert.Equal(expected.GuessTime, actual.guessTime);
        Assert.Equal(expected.HangmanTime, actual.hangmanTime);

        //reset record in database
        await connection.UpdateAsync(Expected);

    }

    [Fact]
    public async Task GuildConfigExistsAsync_ExpectSuccess()
    {

        var exists = await _guildConfigRepo.GuildConfigExistsAsync((ulong) ExpectedId);

        Assert.True(exists);

    }

}