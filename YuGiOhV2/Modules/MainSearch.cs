using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Models;
using YuGiOhV2.Models.Criterion;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class MainSearch : MainBase
    {

        public CommandService CommandService { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        [Command("search"), Alias("s")]
        [Summary("Returns results based on your input! No proper capitalization needed!")]
        public Task SearchCommand([Remainder]string input)
        {

            var lower = input.ToLower();
            var cards = Cache.LowerToUpper.Where(kv => kv.Key.Contains(lower)).Select(kv => kv.Value);
            var amount = cards.Count();

            if (amount == 1)
                return CardCommand(cards.First());
            else if (amount != 0)
                return RecieveInput(amount, cards);
            else
                return NoResultError(input);

        }

        [Command("archetype"), Alias("a")]
        [Summary("Returns cards in entered archetype! No proper capitalization needed!")]
        public Task ArchetypeCommand([Remainder]string input)
        {

            if (Cache.Archetypes.TryGetValue(input, out var cards))
                return RecieveInput(cards.Count, cards);
            else
                return NoResultError("archetypes", input);

        }

        [Command("supports")]
        [Summary("Returns cards that support your input! No proper capitalization needed!")]
        public Task SupportsCommand([Remainder]string input)
        {

            if (Cache.Supports.TryGetValue(input, out var cards))
                return RecieveInput(cards.Count, cards);
            else
                return NoResultError("supports", input);

        }

        [Command("antisupports")]
        [Summary("Returns cards that support your input! No proper capitalization needed!")]
        public Task AntiSupportsCommand([Remainder]string input)
        {

            if (Cache.AntiSupports.TryGetValue(input, out var cards))
                return RecieveInput(cards.Count, cards);
            else
                return NoResultError("anti-supports", input);

        }

        public async Task RecieveInput(int amount, IEnumerable<string> cards)
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithName($"There are {amount} results from your search!");

            var paginator = new PaginatedMessage()
            {

                Author = author,
                Color = Rand.NextColor(),
                Pages = GenDescriptions(cards),
                Options = PagedOptions

            };

            var criteria = new BaseCriteria()
                .AddCriterion(new IntegerCriteria(amount));

            var display = await PagedReplyAsync(paginator);
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            #region CheckMessage
            //cancel if pagination is deleted
            Task CheckMessage(Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel, ulong> _)
            {

                if (cache.Id == display.Id)
                    cts.Cancel();

                Context.Client.MessageDeleted -= CheckMessage;

                return Task.CompletedTask;

            }
            #endregion CheckMessage

            Context.Client.MessageDeleted += CheckMessage;

            var input = await NextMessageAsync(criteria, TimeSpan.FromSeconds(60), token);

            if (!token.IsCancellationRequested && int.TryParse(input.Content, out var selection) && selection > 0 && selection <= cards.Count())
                await CardCommand(cards.ElementAt(selection - 1));

        }

        public IEnumerable<string> GenDescriptions(IEnumerable<string> cards)
        {

            var groups = cards.Batch(30);
            var descriptions = new List<string>(3);
            var counter = 1;

            foreach (var group in groups)
            {

                var str = new StringBuilder();

                foreach (var card in group)
                {

                    str.AppendLine($"{counter}. {card}");
                    counter++;

                }

                descriptions.Add(str.ToString().Trim());

            }

            return descriptions;

        }

        private Task CardCommand(string card)
        {

            AltConsole.Write("Info", "Command", "Executing card command from search module...");

            return CommandService.Commands.First(command => command.Name == "card").ExecuteAsync(Context, new List<object>(1) { card }, null, ServiceProvider);

        }

    }
}
