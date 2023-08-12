using NSubstitute;
using YuGiOh.Scraper.Models.ParserOptions;
using YuGiOh.Scraper.Models.Parsers.Fandom;

namespace YuGiOh.Scraper.Test.Parsers.Fandom;

public class CardParserTests
{

    private readonly Options _options;

    public CardParserTests()
    {

        var optionsArgs = Substitute.For<IOptionsArgs>();

        optionsArgs.GetOptionsArgs().Returns(_ => "-m fandom".Split(' '));

        _options = Options.GetInstance(optionsArgs);

    }

    [Theory]
    [InlineData("652607", "Abyss Actor - Curtain Raiser")]
    [InlineData("167976", "Advance Force")]
    public async Task ParseAsync_NotNull_ExpectSuccess(string id, string name)
    {

        var parser = new CardParser(name, id, _options);

        Assert.NotNull(await parser.ParseAsync());

    }

    [Theory]
    [InlineData("652607", "Abyss Actor - Curtain Raiser")]
    public async Task ParseAsync_CardType_ExpectSuccess(string id, string name)
    {

        var parser = new CardParser(name, id, _options);
        var entity = await parser.ParseAsync();

        Assert.NotNull(entity.CardType);
        Assert.NotEmpty(entity.CardType);

    }

    [Fact]
    public async Task ParseAsync_ExpectPendulumLore_ExpectSuccess()
    {

        var parser = new CardParser("Abyss Actor - Curtain Raiser", "652607", _options);
        var entity = await parser.ParseAsync();

        Assert.NotNull(entity.PendulumLore);
        Assert.NotEmpty(entity.PendulumLore);

    }

}