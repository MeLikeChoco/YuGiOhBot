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
        public const string BaseUrl = "http://yugioh.fandom.com/";
        public const string TcgCards = "api/v1/Articles/List?category=TCG_cards&limit=20000&namespaces=0";
        public const string OcgCards = "api/v1/Articles/List?category=OCG_cards&limit=20000&namespaces=0";
        public const string TcgPacks = "api/v1/Articles/List?category=TCG_Booster_Packs&limit=20000&namespaces=0";
        public const string OcgPacks = "api/v1/Articles/List?category=OCG_Booster_Packs&limit=20000&namespaces=0";
        public const string DbName = "ygo.db";
        public readonly static string DbPath = DbName;
        public readonly static string ConnectionString = $"Data Source = {DbName}";
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
            "'Url' TEXT " +
            ")";
        public const string CreateCardErrorTableSql = "CREATE TABLE 'Errors' (" +
            "'Name' TEXT, " +
            "'Exception' TEXT, " +
            "'InnerException' TEXT, " +
            "'Url' TEXT, " +
            "'Type' TEXT " +
            ")";

    }
}
