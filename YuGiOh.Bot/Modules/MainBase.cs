using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules
{
    public class MainBase : CustomBase
    {

        public Cache Cache { get; set; }
        public IYuGiOhDbService YuGiOhDbService { get; set; }
        public IGuildConfigDbService GuildConfigDbService { get; set; }
        public Web Web { get; set; }
        public Random Rand { get; set; }

        protected bool _minimal;
        protected GuildConfig _guildConfig;

        protected PaginatedAppearanceOptions PagedOptions => new()
        {

            DisplayInformationIcon = false,
            JumpDisplayOptions = JumpDisplayOptions.Never,
            FooterFormat = _guildConfig.AutoDelete ? "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}" : "This embed will not be deleted! | Page {0}/{1}",
            Timeout = _guildConfig.AutoDelete ? TimeSpan.FromSeconds(60) : TimeSpan.FromMilliseconds(-1)

        };

        protected override void BeforeExecute(CommandInfo command)
        {

            Task.Run(async () => _guildConfig = Context.Channel is not SocketDMChannel ? await GuildConfigDbService.GetGuildConfigAsync(Context.Guild.Id) : await GuildConfigDbService.GetGuildConfigAsync(0)).GetAwaiter().GetResult();

        }

    }
}
