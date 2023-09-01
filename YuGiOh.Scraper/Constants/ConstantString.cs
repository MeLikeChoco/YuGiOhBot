namespace YuGiOh.Scraper.Constants;

public static class ConstantString
{

    public const string YugipediaModuleName = "yugipedia";
    public const string FandomModuleName = "fandom";

    public const string RushDuelCardExceptionMessage = "This is a Rush Duel card.";
    public const string SkillCardExceptionMessage = "This is a Skill card.";

    public const string HtmlTagRegex = "<[^>]*>";
    public const string YugipediaUrl = "https://yugipedia.com/";
    public const string YuGiOhFandomUrl = "https://yugioh.fandom.com/";

    // public static readonly string BaseUrl = Constant.ModuleToBaseUrl[Options.GetInstance(new OptionsArgs()).Module];
    public const string MediaWikiParseIdUrl = "api.php?action=parse&format=json&prop=text&formatversion=2&pageid={0}";
    public const string MediaWikiParseNameUrl = "api.php?action=parse&format=json&prop=text&formatversion=2&page={0}";
    public const string CmcontinueQuery = "&cmcontinue={0}";
    public const string MediaWikiIdUrl = "?curid={0}";

    //Categories
    public const string MediaWikiAllCards = "api.php?action=query&format=json&list=categorymemebers&cmtitle=Category%3AAll_cards&cmlimit=50000";
    public const string MediaWikiDuelMonstersCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ADuel_Monsters_cards";

    public const string MediaWikiTcgCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATCG_cards&cmlimit=50000";
    public const string MediaWikiOcgCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AOCG_cards&cmlimit=50000";
    public const string MediaWikiSkillCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ASkill_Cards&cmlimit=50000";
    public const string MediaWikiTokenCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATokens&cmlimit=50000";
    public const string MediaWikiCounterCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ACounters&cmlimit=50000";
    public const string MediaWikiAnimeCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AAnime_cards&cmlimit=50000";

    public const string MediaWikiTcgPacks = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATCG_Booster_Packs";
    public const string MediaWikiOcgPacks = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AOCG_Booster_Packs";

}