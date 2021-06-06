using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Models;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class MainBase : CustomBase
    {

        public Cache Cache { get; set; }
        public Database Database { get; set; }
        public Web Web { get; set; }
        public Random Rand { get; set; }

        protected Setting _setting;
        protected bool _minimal;

        protected PaginatedAppearanceOptions PagedOptions => new()
        {

            DisplayInformationIcon = false,
            JumpDisplayOptions = JumpDisplayOptions.Never,
            FooterFormat = _setting.AutoDelete ? "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}" : "This embed will not be deleted! | Page {0}/{1}",
            Timeout = _setting.AutoDelete ? TimeSpan.FromSeconds(60) : TimeSpan.FromMilliseconds(-1)

        };

        protected override void BeforeExecute(CommandInfo command)
        {


            if (!(Context.Channel is SocketDMChannel))
            {

                _setting = Database.Settings[Context.Guild.Id];
                _minimal = _setting.Minimal;

            }
            else
                _minimal = false;

        }

    }
}
