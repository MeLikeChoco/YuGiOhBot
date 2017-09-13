using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Main : CustomBase
    {

        private Cache _cache;
        private Database _db;
        private bool _minimal;

        public Main(Cache cache, Database db)
        {

            _cache = cache;
            _db = db;

        }

        protected override void BeforeExecute(CommandInfo command)
        {
            
            _minimal = _db.Settings[Context.Guild.Id].Minimal;

        }

        [Command("card")]
        public async Task CardCommand([Remainder]string name)
        {

            if (_cache.Cards.TryGetValue(name.ToLower(), out var embed))
                await SendEmbed(embed, _minimal);
            else
                await NoResultError(name);

        }

    }
}
