using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Criteria;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Commands
{
    public class MainSearch : MainBase
    {

        private readonly IYuGiOhPricesService _yuGiOhPricesService;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly PaginatorFactory _paginatorFactory;

        private static CommandInfo _cardCommand;
        private static readonly object CardCmdLock = new();

        public MainSearch(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IYuGiOhPricesService yuGiOhPricesService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand,
            InteractiveService interactiveService,
            CommandService commandService,
            IServiceProvider services,
            PaginatorFactory paginatorFactory
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, rand, interactiveService)
        {
            _yuGiOhPricesService = yuGiOhPricesService;
            _commandService = commandService;
            _services = services;
            _paginatorFactory = paginatorFactory;
        }

        protected override void BeforeExecute(CommandInfo command)
        {

            base.BeforeExecute(command);

            Task.Run(() =>
            {

                lock (CardCmdLock)
                {
                    _cardCommand ??= _commandService.Commands.First(cmd => cmd.Name == Constants.CardCommand);
                }

            });

        }

        [Command("search"), Alias("s")]
        [Summary("Returns results based on your input! No proper capitalization needed!")]
        public async Task SearchCommand([Remainder] string input)
        {

            var cards = (await YuGiOhDbService.SearchCardsAsync(input)).ToImmutableArray();

            await DisplaySearch(cards, input, "cards");

        }

        [Command("archetype"), Alias("a")]
        [Summary("Returns cards in entered archetype! No proper capitalization needed!")]
        public async Task ArchetypeCommand([Remainder] string input)
        {

            var cards = (await YuGiOhDbService.GetCardsInArchetypeAsync(input)).ToImmutableArray();

            await DisplaySearch(cards, input, "archetypes");

        }

        [Command("support")]
        [Summary("Returns cards that support your input! No proper capitalization needed!")]
        public async Task SupportsCommand([Remainder] string input)
        {

            var cards = (await YuGiOhDbService.GetCardsInSupportAsync(input)).ToImmutableArray();

            await DisplaySearch(cards, input, "supports");

        }

        [Command("antisupport")]
        [Summary("Returns cards that support your input! No proper capitalization needed!")]
        public async Task AntiSupportsCommand([Remainder] string input)
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

            var color = Rand.NextColor();
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

                    str.AppendLine($"{counter}. {card}");
                    counter++;

                }

                descriptions.Add(str.ToString().Trim());

            }

            return descriptions;

        }

        private Task ExecuteCardCommand(string card)
        {

            Logger.Info("Executing card command from search module...");

            return _cardCommand!.ExecuteAsync(Context, new List<object>(1) { card }, null, _services);

        }

    }
}