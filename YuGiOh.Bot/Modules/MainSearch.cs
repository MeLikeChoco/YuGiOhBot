using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using MoreLinq;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Criterion;

namespace YuGiOh.Bot.Modules
{
    public class MainSearch : MainBase
    {

        public CommandService CommandService { get; set; }
        public IServiceProvider Services { get; set; }

        [Command("search"), Alias("s")]
        [Summary("Returns results based on your input! No proper capitalization needed!")]
        public async Task SearchCommand([Remainder] string input)
        {

            var cards = await YuGiOhDbService.SearchCardsAsync(input);
            var amount = cards.Count();

            if (amount == 1)
                await ExecuteCardCommand(cards.First().Name);
            else if (amount != 0)
                await ReceiveInput(amount, cards.Select(card => card.Name));
            else
                await NoResultError(input);

        }

        [Command("archetype"), Alias("a")]
        [Summary("Returns cards in entered archetype! No proper capitalization needed!")]
        public async Task ArchetypeCommand([Remainder] string input)
        {

            var cards = await YuGiOhDbService.GetCardsInArchetype(input);

            if (cards.Any())
                await ReceiveInput(cards.Count(), cards.Select(card => card.Name));
            else
                await NoResultError("archetypes", input);

        }

        [Command("supports")]
        [Summary("Returns cards that support your input! No proper capitalization needed!")]
        public async Task SupportsCommand([Remainder] string input)
        {

            var cards = await YuGiOhDbService.GetCardsFromSupportAsync(input);

            if (cards.Any())
                await ReceiveInput(cards.Count(), cards.Select(card => card.Name));
            else
                await NoResultError("supports", input);

        }

        [Command("antisupports")]
        [Summary("Returns cards that support your input! No proper capitalization needed!")]
        public async Task AntiSupportsCommand([Remainder] string input)
        {

            var cards = await YuGiOhDbService.GetCardsFromAntisupportAsync(input);

            if (cards.Any())
                await ReceiveInput(cards.Count(), cards.Select(card => card.Name));
            else
                await NoResultError("antisupports", input);

        }

        public async Task ReceiveInput(int amount, IEnumerable<string> cards)
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

            if (!token.IsCancellationRequested && input is not null && int.TryParse(input.Content, out var selection) && selection > 0 && selection <= cards.Count())
                await ExecuteCardCommand(cards.ElementAt(selection - 1));

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

        private Task ExecuteCardCommand(string card)
        {

            AltConsole.Write("Info", "Command", "Executing card command from search module...");

            return CommandService.Commands.First(command => command.Name == "card").ExecuteAsync(Context, new List<object>(1) { card }, null, Services);

        }

    }
}
