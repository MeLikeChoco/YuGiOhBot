using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot
{
    public static class Constants
    {

        public const string FandomUrl = "https://yugioh.fandom.com/";
        public const string FandomWikiUrl = "https://yugioh.fandom.com/wiki/";
        public const string YugiPediaUrl = "https://yugipedia.com/";
        public const string YugiPediaWikiUrl = "https://yugipedia.com/wiki/";

        public const string ArtBaseUrl = "https://storage.googleapis.com/ygoprodeck.com/pics_artgame/";
        public const string ArtFileType = "jpg";
        public const string BotsOnDiscordXyzUrl = "https://bots.ondiscord.xyz/bot-api/bots/{0}/guilds";
        public const string DiscordBotsGG = "https://discord.bots.gg/api/v1/bots/{0}/stats";
        //public const string BlueDiscordBotUrl = "https://discordbots.org/api/bots/{0}/stats";

        public const string DatabaseName = "ygopedia";
        public static readonly string DatabasePath = $"Databases/{DatabaseName}.db";
        public static readonly string DatabaseString = $"Data Source = {DatabasePath}";

        public const string CardCommand = "card";

        public const string UnixCpuUsageCmdArgs = "<(grep 'cpu ' /proc/stat) <(sleep 0.5 && grep 'cpu ' /proc/stat) | awk -v RS=\\\"\\\" '{print ($13-$2+$15-$4)*100/($13-$2+$15-$4+$16-$5)}'";

        public static bool IsDebug
        {
            get
            {
                var isDebug = false;
                GetIsDebug(ref isDebug);

                return isDebug;
            }
        }

        [Conditional("DEBUG")]
        private static void GetIsDebug(ref bool isDebug)
            => isDebug = true;

    }

}