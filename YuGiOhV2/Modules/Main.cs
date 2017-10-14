using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Objects.Banlist;
using YuGiOhV2.Objects.Criterion;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Main : CustomBase
    {

        private Cache _cache;
        private Database _db;
        private Web _web;
        private Random _rand;
        private bool _minimal;
        private BaseCriteria _criteria;
        private CancellationTokenSource _cts;
        private IUserMessage _display;

        private static readonly PaginatedAppearanceOptions AOptions = new PaginatedAppearanceOptions
        {

            DisplayInformationIcon = false,
            Timeout = TimeSpan.FromSeconds(60),
            JumpDisplayOptions = JumpDisplayOptions.Never,
            FooterFormat = "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}"            

        };

        public Main(Cache cache, Database db, Web web, Random rand)
        {

            _cache = cache;
            _db = db;
            _web = web;
            _rand = rand;
            _criteria = new BaseCriteria();
            _cts = new CancellationTokenSource();

        }

        protected override void BeforeExecute(CommandInfo command)
        {

            if (!(Context.Channel is SocketDMChannel))
                _minimal = _db.Settings[Context.Guild.Id].Minimal;
            else
                _minimal = false;

        }

        [Command("card")]
        [Summary("Gets a card! No proper capitalization needed!")]
        public async Task CardCommand([Remainder]string input)
        {

            if (_cache.Cards.TryGetValue(input, out var embed))
                await SendCardEmbed(embed, _minimal);
            else
                await NoResultError(input);

        }

        [Command("random"), Alias("rcard", "r")]
        [Summary("Gets a random card!")]
        public async Task RandomCommand()
        {

            var embed = _cache.Cards.RandomSubset(1).First().Value;

            await SendCardEmbed(embed, _minimal);

        }

        [Command("search"), Alias("s")]
        [Ratelimit(1, 0.084, Measure.Minutes)]
        [Summary("Returns results based on your input! No proper capitalization needed!")]
        public async Task SearchCommand([Remainder]string input)
        {

            var lower = input.ToLower();
            var cards = _cache.LowerToUpper.Where(kv => kv.Key.Contains(lower)).Select(kv => kv.Value);
            var amount = cards.Count();

            if (amount == 1)
                await CardCommand(cards.First());
            else if (amount != 0)
                await RecieveInput(amount, cards, _cts.Token);
            else
                await NoResultError(input);

        }

        [Command("archetype"), Alias("a")]
        [Ratelimit(1, 0.084, Measure.Minutes)]
        [Summary("Returns results based on your input! No proper capitalization needed!")]
        public async Task ArchetypeCommand([Remainder]string input)
        {

            if (_cache.Archetypes.ContainsKey(input))
            {

                var cards = _cache.Archetypes[input];
                var amount = cards.Count();

                await RecieveInput(amount, cards, _cts.Token);

            }
            else
                await NoResultError(input);
            
        }

        [Command("image"), Alias("i", "img")]
        [Summary("Returns image of the card based on your input! No proper capitalization needed!")]
        public async Task ImageCommand([Remainder]string card)
        {

            if (_cache.Images.ContainsKey(card))
            {

                using (Context.Channel.EnterTypingState())
                {

                    var link = _cache.Images[card];
                    var stream = await _web.GetStream(link);

                    await Context.Channel.SendFileAsync(stream, $"{card.ToLower().Replace(" ", "")}.png");

                }

            }
            else
                await NoResultError(card);

        }

        [Command("price"), Alias("prices", "p")]
        [Summary("Returns the prices based on your input! No proper capitalization needed!")]
        public async Task PriceCommand([Remainder]string card)
        {

            if (_cache.LowerToUpper.ContainsKey(card))
            {

                using (Context.Channel.EnterTypingState())
                {

                    var name = _cache.LowerToUpper[card];
                    var response = await _web.GetPrices(name);
                    var data = response.Data.Where(d => string.IsNullOrEmpty(d.PriceData.Message)).ToList();

                    var author = new EmbedAuthorBuilder()
                        .WithIconUrl("https://vignette1.wikia.nocookie.net/yugioh/images/8/82/PotofGreed-TF04-JP-VG.jpg/revision/latest?cb=20120829225457")
                        .WithName("YuGiOh Prices")
                        .WithUrl($"https://yugiohprices.com/card_price?name={Uri.EscapeDataString(card)}");

                    var body = new EmbedBuilder()
                        .WithAuthor(author)
                        .WithColor(new Color(33, 108, 42))
                        .WithCurrentTimestamp();

                    if (data.Count > 25)
                    {

                        body.WithDescription("**There are more than 25 results! Due to that, only the first 25 results are shown!**");
                        data = data.GetRange(0, 25);

                    }

                    foreach (var datum in data)
                    {

                        var prices = datum.PriceData.Data.Prices;

                        body.AddField(datum.Name,
                            $"**Rarity:** {datum.Rarity}\n" +
                            $"**Low:** ${prices.Low.ToString("0.00")}\n" +
                            $"**High:** ${prices.High.ToString("0.00")}\n" +
                            $"**Average:** ${prices.Average.ToString("0.00")}", true);

                    }

                    await SendEmbed(body);

                }

            }
            else
                await NoResultError(card);

        }

        [Command("banlist")]
        [Summary("Get the banlist of a specified format! OCG or 1, TCGADV or 2, TCGTRAD or 3")]
        public async Task BanlistCommand([Remainder]string format)
        {

            IFormat banlist;

            switch (format.ToLower())
            {

                case "ocg":
                case "1":
                    banlist = _cache.Banlist.OcgBanlist;
                    break;
                case "tcgadv":
                case "2":
                    banlist = _cache.Banlist.TcgAdvBanlist;
                    break;
                case "tcgtrad":
                case "3":
                    banlist = _cache.Banlist.TcgTradBanlist;
                    break;
                default:
                    await ReplyAsync("The valid formats are OCG or 1, TCGADV or 2, TCGTRAD or 3!");
                    return;

            }

            await DirectMessageAsync("", FormatBanlist("Forbidden", banlist.Forbidden));
            await DirectMessageAsync("", FormatBanlist("Limited", banlist.Limited));
            await DirectMessageAsync("", FormatBanlist("Semi-Limited", banlist.SemiLimited));

        }

        public Embed FormatBanlist(string status, IEnumerable<string> cards)
        {

            var author = new EmbedAuthorBuilder()
                .WithName(status);

            var body = new EmbedBuilder()
                .WithAuthor(author)
                .WithColor(_rand.GetColor())
                .WithDescription(cards.Aggregate((sentence, next) => $"{sentence}\n{next}"));

            return body.Build();

        }

        public async Task RecieveInput(int amount, IEnumerable<string> cards, CancellationToken token)
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithName($"There are {amount} results from your search!");

            var paginator = new PaginatedMessage()
            {

                Author = author,
                Color = _rand.GetColor(),
                Pages = GenDescriptions(cards),
                Options = AOptions
                
            };

            _criteria.AddCriterion(new IntegerCriteria(amount));

            _display = await PagedReplyAsync(paginator, false).ConfigureAwait(false);

            Context.Client.MessageDeleted += CheckMessage;

            var input = await NextMessageAsync(_criteria, TimeSpan.FromSeconds(60));

            if (token.IsCancellationRequested)
                return;

            var selection = int.Parse(input.Content);

            await CardCommand(cards.ElementAt(selection - 1));

        }

        public IEnumerable<string> GenDescriptions(IEnumerable<string> cards)
        {

            var groups = cards.Batch(30);
            var descriptions = new List<string>(3);
            var counter = 1;

            foreach(var group in groups)
            {

                var str = "";

                foreach(var card in group)
                {

                    str += $"{counter}. {card}\n";
                    counter++;

                }

                descriptions.Add(str.Trim());

            }

            return descriptions;

        }

        private Task CheckMessage(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {

            var deletedMsg = cache.Value;

            if (deletedMsg.Id == _display.Id)
                _cts.Cancel();

            Context.Client.MessageDeleted -= CheckMessage;

            return Task.CompletedTask;

        }

        //public string GetFormattedList(IEnumerable<string> cards, string top = null)
        //{

        //    if (string.IsNullOrEmpty(top))
        //        top = $"There are {cards.Count()} results based on your search!\n";

        //    var builder = new StringBuilder($"```{top}");
        //    var counter = 1;

        //    builder.AppendLine();

        //    foreach (var card in cards)
        //    {

        //        builder.AppendLine($"{counter}. {card}");
        //        counter++;

        //    }

        //    builder.AppendLine();
        //    builder.Append("Hit a number to see that result! Expires in 60 seconds!```");

        //    return builder.ToString();

        //}

    }
}
