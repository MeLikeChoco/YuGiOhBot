using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper
{
    public static class ScraperConstants
    {

        private static readonly DefaultHttpRequester Requester = new DefaultHttpRequester("Chrome/74.0");

        public static readonly HtmlParser HtmlParser = new HtmlParser();
        public static readonly IBrowsingContext Context = BrowsingContext.New(Configuration.Default.WithDefaultLoader().With(Requester));
        public static readonly ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        public static readonly ParallelOptions SerialOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
        public const string HtmlTagRegex = "<[^>]*>";
        public const string YuGiOhWikiaUrl = "https://yugioh.fandom.com/";
        public const string YuGiPediaUrl = "https://yugipedia.com/";
        public const string MediaWikiAllCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ADuel_Monsters_cards&cmlimit=500";
        public const string MediaWikiTcgCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATCG_cards&cmlimit=500";
        public const string MediaWikiOcgCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AOCG_cards&cmlimit=500";
        public const string MediaWikiTcgPacks = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATCG_Booster_Packs&cmlimit=500";
        public const string MediaWikiOcgPacks = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AOCG_Booster_Packs&cmlimit=500";
        public const string MediaWikiParseIdUrl = "api.php?action=parse&format=json&prop=text&formatversion=2&pageid=";
        public const string MediaWikiParseNameUrl = "api.php?action=parse&format=json&prop=text&formatversion=2&page=";

        public const string CreateCardTableSql = "CREATE TABLE 'Cards'(" +
            "'Name' TEXT, " +
            "'RealName' TEXT, " +
            "'Passcode' TEXT, " +
            "'CardType' TEXT, " +
            "'Property' TEXT, " +
            "'Level' INTEGER, " +
            "'PendulumScale' INTEGER, " +
            "'Rank' INTEGER, " +
            "'Link' INTEGER, " +
            "'LinkArrows' TEXT, " +
            "'Types' TEXT, " +
            "'Attribute' TEXT, " +
            "'Materials' TEXT, " +
            "'Lore' TEXT, " +
            "'Atk' TEXT, " +
            "'Def' TEXT, " +
            "'Archetype' TEXT, " +
            "'Supports' TEXT, " +
            "'AntiSupports' TEXT, " +
            "CardTrivia TEXT, " +
            "'OcgExists' INTEGER, " +
            "'TcgExists' INTEGER, " +
            "'OcgStatus' TEXT, " +
            "'TcgAdvStatus' TEXT, " +
            "'TcgTrnStatus' TEXT, " +
            "'Img' TEXT, " +
            "'Url' TEXT " +
            ")";

        public const string CreateBoosterPackTableSql = "CREATE TABLE 'BoosterPacks'(" +
            "'Name' TEXT, " +
            "'Dates' TEXT, " +
            "'Cards' TEXT, " +
            "'Url' TEXT, " +
            "'TcgExists' INTEGER, " +
            "'OcgExists' INTEGER " +
            ")";

        public const string CreateErrorTable = "CREATE TABLE 'Errors' (" +
            "'Name' TEXT, " +
            "'Exception' TEXT, " +
            "'InnerException' TEXT, " +
            "'Url' TEXT, " +
            "'Type' TEXT " +
            ")";

    }
}
