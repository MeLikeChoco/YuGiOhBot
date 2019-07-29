using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2
{
    public static class Constants
    {

        public const string FandomUrl = "https://yugioh.fandom.com/";
        public const string FandomWikiUrl = "https://yugioh.fandom.com/wiki/";
        public const string YugiPediaUrl = "https://yugipedia.com/";
        public const string YugiPediaWikiUrl = "https://yugipedia.com/wiki/";
        public const string DatabaseName = "ygopedia";
        public static readonly string DatabasePath = $"Databases/{DatabaseName}.db";
        public static readonly string DatabaseString = $"Data Source = {DatabasePath}";
        public const string ArtBaseUrl = "https://raw.githubusercontent.com/moecube/yugioh-images/master/pics/";

    }
}
