using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Criterion;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Commands
{
    public class MainSearch : MainBase
    {

        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;

        private static CommandInfo? _cardCommand;
        private static readonly object CardCmdLock = new();

        public MainSearch(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand,
            CommandService commandService,
            IServiceProvider services
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, rand)
        {
            _commandService = commandService;
            _services = services;
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

            var cards = await YuGiOhDbService.GetCardsInArchetypeAsync(input);

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

            var cards = await YuGiOhDbService.GetCardsFromAntisupportAsync(input).ContinueWith(result => result.Result.ToArray());

            if (cards.Any())
                await ReceiveInput(cards.Length, cards.Select(card => card.Name));
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