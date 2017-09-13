using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
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

            if (!(Context.Channel is SocketDMChannel))
                _minimal = _db.Settings[Context.Guild.Id].Minimal;
            else
                _minimal = false;

        }

        [Command("card")]
        public async Task CardCommand([Remainder]string name)
        {

            if (_cache.Cards.TryGetValue(name, out var embed))
                await SendEmbed(embed, _minimal);
            else
                await NoResultError(name);

        }

        [Command("random")]
        public async Task RandomCommand()
        {

            var embed = _cache.Cards.RandomSubset(1).First().Value;

            await SendEmbed(embed, _minimal);

        }

        [Command("archetype")]
        public async Task ArchetypeCommand()
        {



        }

    }
}
