using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Configuration : CustomBase
    {

        private Database _db;
        private Setting _setting;

        public Configuration(Database db)
        {

            _db = db;

        }

        protected override void BeforeExecute(CommandInfo command)
        {
            _setting = _db.Settings[Context.Guild.Id];
        }

        [Command("prefix")]
        public async Task SetPrefixCommand([Remainder]string prefix)
        {

            try
            {

                await _db.SetPrefix(Context.Guild.Id, prefix);
                await ReplyAsync($"Prefix has been set to `{prefix}`");

            }
            catch (Exception e)
            {

                AltConsole.Print("Error", "Prefix", "BAH GAWD SOMETHING WRONG WITH SETTING PREFIX", e);
                await ReplyAsync("There was an error in setting the prefix. Please report error with `y!feedback <message>`");

            }

        }

        [Command("prefix")]
        public async Task PrefixCommand()
            => await ReplyAsync($"**Prefix:** {_setting.Prefix}");

    }
}
