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

        public static readonly HtmlParser HtmlParser = new HtmlParser();
        public static readonly IBrowsingContext Context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        public static readonly ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 };
        public const string YuGiOhWikiaUrl = "http://yugioh.fandom.com/";
        public const string YuGiOhWikiaTcgCards = "api/v1/Articles/List?category=TCG_cards&limit=20000&namespaces=0";
        public const string YuGiOhWikiaOcgCards = "api/v1/Articles/List?category=OCG_cards&limit=20000&namespaces=0";
        public const string YuGiOhWikiaTcgPacks = "api/v1/Articles/List?category=TCG_Booster_Packs&limit=20000&namespaces=0";
        public const string YuGiOhWikiaOcgPacks = "api/v1/Articles/List?category=OCG_Booster_Packs&limit=20000&namespaces=0";
        public const string YuGiPediaBaseUrl = "https://yugipedia.com/";
        public const string YuGiPediaUrl = "https://yugipedia.com/wiki/";
        public const string YuGiPediaTcgCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATCG_cards&cmlimit=500";
        public const string YuGiPediaOcgCards = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AOCG_cards&cmlimit=500";
        public const string YuGiPediaTcgPacks = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATCG_Booster_Packs&cmlimit=500";
        public const string YuGiPediaOcgPacks = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AOCG_Booster_Packs&cmlimit=500";
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
