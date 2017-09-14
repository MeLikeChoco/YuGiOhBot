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
        
        [Command("search")]
        public async Task SearchCommand([Remainder]string search)
        {

            var lower = search.ToLower();
            var cards = _cache.Uppercase.Where(name => name.ToLower().Contains(lower));
            var amount = cards.Count();

            if (cards.Count() != 0)
                await RecieveInput(amount, cards);
            else
                await NoResultError(search);

        }

        [Command("archetype")]
        public async Task ArchetypeCommand([Remainder]string archetype)
        {

            if (_cache.Archetypes.ContainsKey(archetype))
            {

                var cards = _cache.Archetypes[archetype];
                var amount = cards.Count();

                await RecieveInput(amount, cards);

            }
            else
                await NoResultError(archetype);

        }

        public async Task RecieveInput(int amount, IEnumerable<string> cards)
        {

            if (amount > 50)
            {

                await TooManyError();
                return;

            }

            await ReplyAndDeleteAsync(GetFormattedList($"There are {amount} results based on your search!", cards), timeout: TimeSpan.FromSeconds(60));

            var input = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));

            if (int.TryParse(input.Content, out var selection) && (selection < amount || selection < 1))
                await CardCommand(cards.ElementAt(selection - 1));

        }

        public string GetFormattedList(string top, IEnumerable<string> cards)
        {

            var builder = new StringBuilder($"```top");
            var counter = 1;

            builder.AppendLine();

            foreach (var card in cards)
            {

                builder.AppendLine($"{counter}. {card}");
                counter++;

            }

            builder.AppendLine();
            builder.Append("Hit a number to see that result! Expires in 60 seconds!```");

            return builder.ToString();

        }

    }
}
