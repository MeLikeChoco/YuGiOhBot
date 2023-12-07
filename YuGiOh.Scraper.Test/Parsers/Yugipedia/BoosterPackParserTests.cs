namespace YuGiOh.Scraper.Test.Parsers.Yugipedia;

public class BoosterPackParserTests
{

    [Fact]
    public async Task GetId_ExpectNotZero()
    {

        var parser = new TestBoosterPackParser("397914", "Breakers of Shadow");
        var actual = await parser.GetId();

        Assert.NotEqual(0, actual);

    }

    [Fact]
    public async Task GetName_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestBoosterPackParser("397914", "Breakers of Shadow");
        var actual = await parser.GetName();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetDates_ExpectNotEmpty()
    {

        var parser = new TestBoosterPackParser("574517", "Dark Neostorm");
        var actual = await parser.GetDates();

        Assert.NotEmpty(actual);

    }

    [Fact]
    public async Task GetCards_ExpectNotEmpty()
    {

        var parser = new TestBoosterPackParser("374280", "Premium Gold: Return of the Bling");
        var actual = await parser.GetCards();

        Assert.NotEmpty(actual);

    }

    [Fact]
    public async Task GetUrl_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestBoosterPackParser("543", "Metal Raiders");
        var actual = await parser.GetUrl();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

}