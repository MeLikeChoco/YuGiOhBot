using System.Collections.Generic;
using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Scraper.Models.Parsers;

public abstract class BaseCardParser : ICanParse<CardEntity>
{

    protected virtual async Task BeforeParseAsync() { }
    protected virtual async Task AfterParseAsync() { }

    public virtual async Task<CardEntity> ParseAsync()
    {

        await BeforeParseAsync();

        var card = new CardEntity
        {

            Id = await GetId(),
            Name = await GetName(),
            RealName = await GetRealName(),
            CardType = await GetCardType(),
            Property = await GetProperty(),
            Types = await GetTypes(),
            Attribute = await GetAttribute(),
            Materials = await GetMaterials(),
            Lore = await GetLore(),
            PendulumLore = await GetPendulumLore(),
            Translations = await GetTranslations(),
            Archetypes = await GetArchetypes(),
            Supports = await GetSupports(),
            AntiSupports = await GetAntiSupports(),
            Link = await GetLinkCount(),
            LinkArrows = await GetLinkArrows(),
            Atk = await GetAtk(),
            Def = await GetDef(),
            Level = await GetLevel(),
            PendulumScale = await GetPendulumScale(),
            Rank = await GetRank(),
            TcgExists = await GetTcgExists(),
            OcgExists = await GetOcgExists(),
            Img = await GetImgLink(),
            Url = await GetUrl(),
            Passcode = await GetPasscode(),
            OcgStatus = await GetOcgStatus(),
            TcgAdvStatus = await GetTcgAdvStatus(),
            TcgTrnStatus = await GetTcgTrnStatus(),
            CardTrivia = await GetCardTrivia()

        };

        await AfterParseAsync();

        return card;

    }

    protected abstract Task<int> GetId();
    protected abstract Task<string> GetName();
    protected abstract Task<string> GetRealName();
    protected abstract Task<string> GetCardType();
    protected abstract Task<string> GetProperty();
    protected abstract Task<string> GetTypes();
    protected abstract Task<string> GetAttribute();
    protected abstract Task<string> GetMaterials();
    protected abstract Task<string> GetLore();
    protected abstract Task<string> GetPendulumLore();
    protected abstract Task<List<TranslationEntity>> GetTranslations();
    protected abstract Task<List<string>> GetArchetypes();
    protected abstract Task<List<string>> GetSupports();
    protected abstract Task<List<string>> GetAntiSupports();
    protected abstract Task<int> GetLinkCount();
    protected abstract Task<string> GetLinkArrows();
    protected abstract Task<string> GetAtk();
    protected abstract Task<string> GetDef();
    protected abstract Task<int> GetLevel();
    protected abstract Task<int> GetPendulumScale();
    protected abstract Task<int> GetRank();
    protected abstract Task<bool> GetTcgExists();
    protected abstract Task<bool> GetOcgExists();
    protected abstract Task<string> GetImgLink();
    protected abstract Task<string> GetUrl();
    protected abstract Task<string> GetPasscode();
    protected abstract Task<string> GetOcgStatus();
    protected abstract Task<string> GetTcgAdvStatus();
    protected abstract Task<string> GetTcgTrnStatus();
    protected abstract Task<string> GetCardTrivia();

}