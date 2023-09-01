using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Models.Parsers.Yugipedia;

namespace YuGiOh.Scraper.Test.Parsers.Yugipedia;

public class TestCardParser : CardParser, IDisposable
{

    public TestCardParser(string id, string name) : base(id, name)
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

    public new Task<string> GetRealName()
    {
        return base.GetRealName();
    }

    public new Task<string> GetCardType()
    {
        return base.GetCardType();
    }

    public new Task<string> GetProperty()
    {
        return base.GetProperty();
    }

    public new Task<string> GetTypes()
    {
        return base.GetTypes();
    }

    public new Task<string> GetAttribute()
    {
        return base.GetAttribute();
    }

    public new Task<string> GetMaterials()
    {
        return base.GetMaterials();
    }

    public new Task<string> GetLore()
    {
        return base.GetLore();
    }

    public new Task<string> GetPendulumLore()
    {
        return base.GetPendulumLore();
    }

    public new Task<List<TranslationEntity>> GetTranslations()
    {
        return base.GetTranslations();
    }

    public new Task<List<string>> GetArchetypes()
    {
        return base.GetArchetypes();
    }

    public new Task<List<string>> GetSupports()
    {
        return base.GetSupports();
    }

    public new Task<List<string>> GetAntiSupports()
    {
        return base.GetAntiSupports();
    }

    public new Task<int> GetLinkCount()
    {
        return base.GetLinkCount();
    }

    public new Task<string> GetLinkArrows()
    {
        return base.GetLinkArrows();
    }

    public new Task<string> GetAtk()
    {
        return base.GetAtk();
    }

    public new Task<string> GetDef()
    {
        return base.GetDef();
    }

    public new Task<int> GetLevel()
    {
        return base.GetLevel();
    }

    public new Task<int> GetPendulumScale()
    {
        return base.GetPendulumScale();
    }

    public new Task<int> GetRank()
    {
        return base.GetRank();
    }

    public new Task<bool> GetTcgExists()
    {
        return base.GetTcgExists();
    }

    public new Task<bool> GetOcgExists()
    {
        return base.GetOcgExists();
    }

    public new Task<string> GetImgLink()
    {
        return base.GetImgLink();
    }

    public new Task<string> GetUrl()
    {
        return base.GetUrl();
    }

    public new Task<string> GetPasscode()
    {
        return base.GetPasscode();
    }

    public new Task<string> GetOcgStatus()
    {
        return base.GetOcgStatus();
    }

    public new Task<string> GetTcgAdvStatus()
    {
        return base.GetTcgAdvStatus();
    }

    public new Task<string> GetTcgTrnStatus()
    {
        return base.GetTcgTrnStatus();
    }

    public new Task<string> GetCardTrivia()
    {
        return base.GetCardTrivia();
    }

    public void Dispose()
    {
        AfterParseAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}