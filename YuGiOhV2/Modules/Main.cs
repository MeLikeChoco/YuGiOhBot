using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Objects;
using YuGiOhV2.Objects.Banlist;
using YuGiOhV2.Objects.Criterion;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Main : CustomBase
    {

        public Cache Cache { get; set; }
        public Database Database { get; set; }
        public Web Web { get; set; }
        public Random Rand { get; set; }

        private Setting _setting;
        private bool _minimal;
        private CancellationTokenSource _cts;
        private IUserMessage _display;

        private PaginatedAppearanceOptions _aOptions => new PaginatedAppearanceOptions
        {

            DisplayInformationIcon = false,
            JumpDisplayOptions = JumpDisplayOptions.Never,
            FooterFormat = _setting.AutoDelete ? "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}" : "This embed will not be deleted! | Page {0}/{1}",
            Timeout = _setting.AutoDelete ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds(-1)

        };

        protected override void BeforeExecute(CommandInfo command)
        {

            _setting = Database.Settings[Context.Guild.Id];

            if (!(Context.Channel is SocketDMChannel))
                _minimal = _setting.Minimal;
            else
                _minimal = false;

            _cts = new CancellationTokenSource();

        }

        [Command("booster")]
        [Summary("Gets information on a booster pack!")]
        public async Task BoosterCommand([Remainder]string input)
        {

            if (Cache.BoosterPacks.TryGetValue(input, out var booster))
            {

                var cards = booster.Open();
                var cardNumberLength = 0;

                if (cards.All(card => card.CardNumber != null))
                    cardNumberLength = cards.Max(card => card.CardNumber.Length);

                var cardNameLength = cards.Max(card => card.Name.Length);
                var builder = new StringBuilder("```");

                foreach (var card in cards)
                    builder.AppendLine($"{card.CardNumber?.PadRight(cardNumberLength + 3)}{card.Name.PadRight(cardNameLength + 3)}{card.Rarity}");

                builder.Append("```");

                await ReplyAsync(builder.ToString());

            }
            else
                await NoResultError(input);

        }

        [Command("card"), Alias("c")]
        [Summary("Gets a card! No proper capitalization needed!")]
        public async Task CardCommand([Remainder]string input)
        {

            if (Cache.Cards.TryGetValue(input, out var embed))
                await SendCardEmbed(embed, _minimal);
            else
                await NoResultError(input);

        }

        [Command("random"), Alias("rcard", "r")]
        [Summary("Gets a random card!")]
        public async Task RandomCommand()
        {

            var embed = Cache.Cards.RandomSubset(1).First().Value;

            await SendCardEmbed(embed, _minimal);

        }

        [Command("search"), Alias("s")]
        [Summary("Returns results based on your input! No proper capitalization needed!")]
        public async Task SearchCommand([Remainder]string input)
        {

            var lower = input.ToLower();
            var cards = Cache.LowerToUpper.Where(kv => kv.Key.Contains(lower)).Select(kv => kv.Value);
            var amount = cards.Count();

            if (amount == 1)
                await CardCommand(cards.First());
            else if (amount != 0)
                await RecieveInput(amount, cards, _cts.Token);
            else
                await NoResultError(input);

        }

        [Command("archetype"), Alias("a")]
        [Summary("Returns results based on your input! No proper capitalization needed!")]
        public async Task ArchetypeCommand([Remainder]string input)
        {

            if (Cache.Archetypes.ContainsKey(input))
            {

                var cards = Cache.Archetypes[input];
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

            if (Cache.Images.ContainsKey(card))
            {

                using (Context.Channel.EnterTypingState())
                {

                    var link = Cache.Images[card];
                    var stream = await Web.GetStream(link);

                    await Context.Channel.SendFileAsync(stream, $"{card.ToLower().Replace(" ", "")}.png");

                }

            }
            else
                await NoResultError(card);

        }

        [Command("art")]
        [Summary("Returns the art of the card based on input! No proper capitalization needed!")]
        public async Task ArtCommand([Remainder]string card)
        {

            if (Cache.Passcodes.TryGetValue(card, out var passcode))
            {

                Stream stream;

                try
                {

                    stream = await GetArtGithub(passcode);

                }
                catch
                {

                    await ReplyAsync("There was a problem while retrieving the art, please try again later.");
                    return;

                }

                await UploadAsync(stream, $"{card}.png");

            }
            else
                await NoResultError(card);

        }

        [Command("price"), Alias("prices", "p")]
        [Summary("Returns the prices based on your input! No proper capitalization needed!")]
        public async Task PriceCommand([Remainder]string card)
        {

            if (Cache.LowerToUpper.ContainsKey(card))
            {

                using (Context.Channel.EnterTypingState())
                {

                    var name = Cache.LowerToUpper[card];
                    var response = await Web.GetPrices(name);
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
                    banlist = Cache.Banlist.OcgBanlist;
                    break;
                case "tcgadv":
                case "2":
                    banlist = Cache.Banlist.TcgAdvBanlist;
                    break;
                case "tcgtrad":
                case "3":
                    banlist = Cache.Banlist.TcgTradBanlist;
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
                .WithRandomColor()
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
                Color = Rand.NextColor(),
                Pages = GenDescriptions(cards),
                Options = _aOptions

            };

            var criteria = new BaseCriteria().AddCriterion(new IntegerCriteria(amount));

            _display = await PagedReplyAsync(paginator, false).ConfigureAwait(false);

            Context.Client.MessageDeleted += CheckMessage;

            var input = await NextMessageAsync(criteria, TimeSpan.FromSeconds(60));

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

        private Task CheckMessage(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {

            var deletedMsg = cache.Value;

            if (deletedMsg.Id == _display.Id)
                _cts.Cancel();

            Context.Client.MessageDeleted -= CheckMessage;

            return Task.CompletedTask;

        }

        private Task<Stream> GetArtGithub(string passcode)
        {

            var url = $"https://raw.githubusercontent.com/shadowfox87/YGOTCGOCGPics323x323/master/{passcode}.png";
            return Web.GetStream(url);

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
