using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Criteria;
using YuGiOh.Bot.Modules.Interactions.Autocompletes;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Search : MainInteractionBase<SocketSlashCommand>
    {

        private readonly IYuGiOhPricesService _yuGiOhPricesService;
        private readonly Random _random;
        private readonly PaginatorFactory _paginatorFactory;

        public Search(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IYuGiOhPricesService yuGiOhPricesService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            InteractiveService interactiveService,
            Random random,
            PaginatorFactory paginatorFactory
        )
            : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, interactiveService)
        {

            _yuGiOhPricesService = yuGiOhPricesService;
            _random = random;
            _paginatorFactory = paginatorFactory;

        }

        public override async Task BeforeExecuteAsync(ICommandInfo command)
        {

            await base.BeforeExecuteAsync(command);

            await DeferAsync();

        }

        [SlashCommand("search", "Gets cards based on your input")]
        public async Task SearchCommand([Autocomplete(typeof(CardAutocomplete)), Summary(description: "The input")] string input)
        {

            var cards = (await YuGiOhDbService.SearchCardsAsync(input)).ToImmutableArray();

            await DisplaySearch(cards, input, "cards");

        }

        [SlashCommand("archetype", "Gets cards in entered archetype")]
        public async Task ArchetypeCommand([Autocomplete(typeof(ArchetypeAutocomplete)), Summary(description: "The input")] string input)
        {

            var cards = (await YuGiOhDbService.GetCardsInArchetypeAsync(input)).ToImmutableArray();

            await DisplaySearch(cards, input, "archetypes");

        }

        [SlashCommand("support", "Gets cards in entered support")]
        public async Task SupportCommand([Autocomplete(typeof(SupportAutocomplete)), Summary(description: "The input")] string input)
        {

            var cards = (await YuGiOhDbService.GetCardsInSupportAsync(input)).ToImmutableArray();

            await DisplaySearch(cards, input, "supports");

        }

        [SlashCommand("antisupport", "Gets cards in entered antisupport")]
        public async Task AntisupportCommand([Autocomplete(typeof(AntisupportAutocomplete)), Summary(description: "The input")] string input)
        {

            var cards = (await YuGiOhDbService.GetCardsInAntisupportAsync(input)).ToImmutableArray();

            await DisplaySearch(cards, input, "antisupports");

        }

        private async Task DisplaySearch(IList<Card> cards, string input, string objects = null)
        {

            var amount = cards.Count;

            if (amount == 1)
                await SendCardEmbedAsync(cards.First().GetEmbedBuilder(), GuildConfig.Minimal, _yuGiOhPricesService);
            else if (amount != 0)
                await ReceiveInput(amount, cards);
            else
                await NoResultError(objects, input);

        }

        private async Task ReceiveInput(int amount, IList<Card> cards)
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithName($"There are {amount} results from your search. Timeout in 60 seconds!");

            var color = _random.NextColor();
            var pageDescriptions = GenDescriptions(cards.Select(card => card.Name));
            var pages = pageDescriptions
                .Select(description =>
                    new PageBuilder()
                        .WithAuthor(author)
                        .WithDescription(description)
                        .WithColor(color)
                );

            var paginatorBuilder = _paginatorFactory
                .CreateStaticPaginatorBuilder(GuildConfig)
                .WithPages(pages);

            var cts = new CancellationTokenSource();
            var paginatorMessageTask = SendPaginatorAsync(paginatorBuilder.Build(), ct: cts.Token);

            Context.Client.MessageDeleted += CheckMessage;

            var input = await NextMessageAsync(
                new BaseCriteria(Context).AddCriteria(new IntegerCriteria(1, cards.Count)),
                TimeSpan.FromSeconds(60),
                cts.Token
            );

            if (
                cts.IsCancellationRequested ||
                !input.IsSuccess ||
                input.IsCanceled ||
                input.IsTimeout ||
                !int.TryParse(input.Value?.Content, out var selection) ||
                selection < 0 ||
                selection > amount
            )
            {

                Context.Client.MessageDeleted -= CheckMessage;

                return;

            }

            await SendCardEmbedAsync(cards[selection - 1].GetEmbedBuilder(), GuildConfig.Minimal, _yuGiOhPricesService);

            return;

            #region CheckMessage

            //cancel if pagination is deleted
            Task CheckMessage(Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel, ulong> _)
            {

                Task.Run(async () =>
                {

                    var paginatorMessage = await paginatorMessageTask;

                    if (cache.Id == paginatorMessage.Message.Id)
                        cts.Cancel();

                    Context.Client.MessageDeleted -= CheckMessage;

                }, cts.Token);

                return Task.CompletedTask;

            }

            #endregion CheckMessage

        }

        // private async Task ReceiveInput(int amount, IList<Card> cards)
        // {
        //
        //     var author = new EmbedAuthorBuilder()
        //         .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
        //         .WithName($"There are {amount} results from your search!");
        //
        //     var paginator = new PaginatedMessage()
        //     {
        //
        //         Author = author,
        //         Color = _random.NextColor(),
        //         Pages = GenDescriptions(cards.Select(card => card.Name)),
        //         Options = PagedOptions
        //
        //     };
        //
        //     var criteria = new BaseCriteria()
        //         .AddCriterion(new IntegerCriteria(amount));
        //
        //     var display = await PagedComponentReplyAsync(paginator);
        //     var cts = new CancellationTokenSource();
        //     var token = cts.Token;
        //
        //     #region CheckMessage
        //
        //     //cancel if pagination is deleted
        //     Task CheckMessage(Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel, ulong> _)
        //     {
        //
        //         if (cache.Id == display.Id)
        //             cts.Cancel();
        //
        //         Context.Client.MessageDeleted -= CheckMessage;
        //
        //         return Task.CompletedTask;
        //
        //     }
        //
        //     #endregion CheckMessage
        //
        //     Context.Client.MessageDeleted += CheckMessage;
        //
        //     var input = await NextMessageAsync(criteria, TimeSpan.FromSeconds(60), token);
        //
        //     if (
        //         token.IsCancellationRequested ||
        //         input is null ||
        //         !int.TryParse(input.Content, out var selection) ||
        //         selection < 0 ||
        //         selection > amount
        //     )
        //     {
        //         return;
        //     }
        //
        //     await SendCardEmbedAsync(cards[selection - 1].GetEmbedBuilder(), GuildConfig.Minimal);
        //
        // }

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