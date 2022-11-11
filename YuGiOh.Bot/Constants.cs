using System;
using System.Diagnostics;

namespace YuGiOh.Bot
{
    public static class Constants
    {

        public static class Url
        {
            
            public const string FandomUrl = "https://yugioh.fandom.com/";
            public const string FandomWikiUrl = "https://yugioh.fandom.com/wiki/";
            public const string YugiPediaUrl = "https://yugipedia.com/";
            public const string YugiPediaWikiUrl = "https://yugipedia.com/wiki/";


            public const string ArtBaseUrl = "https://images.ygoprodeck.com/images/cards_cropped/";
            public const string BotsOnDiscordXyzUrl = "https://bots.ondiscord.xyz/bot-api/bots/{0}/guilds";
            public const string DiscordBotsGGUrl = "https://discord.bots.gg/api/v1/bots/{0}/stats";
            //public const string BlueDiscordBotUrl = "https://discordbots.org/api/bots/{0}/stats";
            
        }

        public const string ArtFileType = "jpg";

        public const string CardCommand = "card";
        public const string UnixCpuUsageCmdArgs = "<(grep 'cpu ' /proc/stat) <(sleep 0.5 && grep 'cpu ' /proc/stat) | awk -v RS=\\\"\\\" '{print ($13-$2+$15-$4)*100/($13-$2+$15-$4+$16-$5)}'";

        private const string YuGiOhEnvironmentKey = "YUGIOH_ENV";
        public static string YuGiOhEnvironment { get; } = Environment.GetEnvironmentVariable(YuGiOhEnvironmentKey);

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
        // ReSharper disable once RedundantAssignment
        private static void GetIsDebug(ref bool isDebug)
            => isDebug = true;

    }

}