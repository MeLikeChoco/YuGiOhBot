using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Models.Parsers.Yugipedia;

namespace YuGiOh.Scraper.Test.Parsers.Yugipedia;

public class TestBoosterPackParser : BoosterPackParser, IDisposable
{

    public TestBoosterPackParser(string id, string name) : base(id, name)
    {
        BeforeParseAsync().GetAwaiter().GetResult();
    }
    
    private new Task BeforeParseAsync()
    {
        return base.BeforeParseAsync();
    }

    public new Task<int> GetId()
    {
        return base.GetId();
    }

    public new Task<string> GetName()
    {
        return base.GetName();
    }

    public new Task<List<BoosterPackDateEntity>> GetDates()
    {
        return base.GetDates();
    }

    public new Task<List<BoosterPackCardEntity>> GetCards()
    {
        return base.GetCards();
    }

    public new Task<string> GetUrl()
    {
        return base.GetUrl();
    }

    public new Task<bool> GetOcgExists()
    {
        return base.GetOcgExists();
    }

    public new Task<bool> GetTcgExists()
    {
        return base.GetTcgExists();
    }

    public void Dispose()
    {
        AfterParseAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

}