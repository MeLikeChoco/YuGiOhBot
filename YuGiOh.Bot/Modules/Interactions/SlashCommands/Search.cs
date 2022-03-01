using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Autocompletes;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Criterion;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Search : MainInteractionBase<SocketSlashCommand>
    {

        private readonly Random _random;

        public Search(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random random
        )
            : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web)
        {

            _random = random;

        }

        [SlashCommand("search", "Gets cards based on your input! No proper capitalization needed!")]
        public async Task SearchCommand([Autocomplete(typeof(CardAutocomplete))] [Summary(description: "The input")] string input)
        {

            await DeferAsync();

            var cards = (await YuGiOhDbService.SearchCardsAsync(input)).ToList();
            var amount = cards.Count;

            if (amount == 1)
                await SendCardEmbedAsync(cards.First().GetEmbedBuilder(), GuildConfig.Minimal);
            else if (amount != 0)
                await ReceiveInput(amount, cards);
            else
                await NoResultError(input);

        }

        [SlashCommand("archetype", "Gets cards in entered archetype! No proper capitalization needed!")]
        public async Task ArchetypeCommand([Autocomplete(typeof(ArchetypeAutocomplete))] [Summary(description: "The input")] string input)
        {

            var cards = (await YuGiOhDbService.GetCardsInArchetypeAsync(input)).ToList();

            if (cards.Any())
                await ReceiveInput(cards.Count, cards);
            else
                await NoResultError("archetypes", input);

        }

        private async Task ReceiveInput(int amount, IList<Card> cards)
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithName($"There are {amount} results from your search!");

            var paginator = new PaginatedMessage()
            {

                Author = author,
                Color = _random.NextColor(),
                Pages = GenDescriptions(cards.Select(card => card.Name)),
                Options = PagedOptions

            };

            var criteria = new BaseCriteria()
                .AddCriterion(new IntegerCriteria(amount));

            var display = await PagedComponentReplyAsync(paginator);
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

            if (
                token.IsCancellationRequested ||
                input is null ||
                !int.TryParse(input.Content, out var selection) ||
                selection < 0 ||
                selection > amount
            )
            {
                return;
            }

            await SendCardEmbedAsync(cards[selection - 1].GetEmbedBuilder(), GuildConfig.Minimal);

        }

        private static IEnumerable<string> GenDescriptions(IEnumerable<string> cards)
        {

            var groups = cards.Chunk(30);
            var descriptions = new List<string>(3);
            var counter = 1;

            foreach (var group in groups)
            {

                var str = new StringBuilder();

                foreach (var card in group)
                {

                    str.Append(counter).Append(". ").AppendLine(card);
                    counter++;

                }

                descriptions.Add(str.ToString().Trim());

            }

            return descriptions;

        }

    }
}