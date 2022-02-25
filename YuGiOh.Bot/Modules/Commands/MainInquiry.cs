using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MoreLinq;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Services;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Bot.Modules.Commands
{
    public class MainInquiry : MainBase
    {

        //[Command("booster")]
        //[Summary("Gets information on a booster pack!")]
        //public Task BoosterCommand([Remainder]string input)
        //{

        //    if (Cache.BoosterPacks.TryGetValue(input, out var booster))
        //    {

        //        var cards = booster.Open();
        //        var cardNumberLength = 0;

        //        if (cards.All(card => card.CardNumber is not null))
        //            cardNumberLength = cards.Max(card => card.CardNumber.Length);

        //        var cardNameLength = cards.Max(card => card.Name.Length);
        //        var builder = new StringBuilder("```");

        //        foreach (var card in cards)
        //            builder.AppendLine($"{card.CardNumber?.PadRight(cardNumberLength + 3)}{card.Name.PadRight(cardNameLength + 3)}{card.Rarity}");

        //        builder.Append("```");

        //        return ReplyAsync(builder.ToString());

        //    }
        //    else
        //        return NoResultError(input);

        //}

        [Command(Constants.CardCommand), Alias("c")]
        [Summary("Gets a card! No proper capitalization needed!")]
        public async Task CardCommand([Remainder] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await SendCardEmbedAsync(card.GetEmbedBuilder(), _guildConfig.Minimal, Web);
            else
                await NoResultError(input);

            //if (Cache.NameToCard.TryGetValue(input, out var card))
            //    return SendCardEmbed(card.GetEmbedBuilder(), _minimal, Web);
            //else
            //    return NoResultError(input);

        }

        [Command("random"), Alias("rcard", "r")]
        [Summary("Gets a random card!")]
        public async Task RandomCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await SendCardEmbedAsync(card.GetEmbedBuilder(), _guildConfig.Minimal, Web);

        }

        [Command("image"), Alias("i", "img")]
        [Summary("Returns image of the card based on your input! No proper capitalization needed!")]
        public async Task ImageCommand([Remainder] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await UploadImage(card.Name, card.Img);
            else
                await NoResultError(input);

        }

        [Command("randomimage"), Alias("rimage", "ri")]
        [Summary("Gets random card image!")]
        public async Task RandomImageCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await UploadImage(card.Name, card.Img);

        }

        [Command("art")]
        [Summary("Returns the art of the card based on input! No proper capitalization needed!")]
        public async Task ArtCommand([Remainder] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await UploadImage(card.Name, GetArtUrl(card.Passcode));
            else
                await NoResultError(input);

        }

        [Command("randomart"), Alias("rart", "ra", "rat")]
        [Summary("Gets random card art!")]
        public async Task RandomCardArtCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await UploadImage(card.Name, GetArtUrl(card.Passcode));

        }

        private async Task UploadImage(string name, string url)
        {

            try
            {

                using (Context.Channel.EnterTypingState())
                {

                    var stream = await Web.GetStream(url);

                    await UploadAsync(stream, $"{Uri.EscapeDataString(name)}.png");

                }

            }
            catch
            {

                await ReplyAsync("There was a problem while uploading the image, please try again later.");
                return;

            }

        }

        //this is pretty wack
        [Command("price"), Alias("prices", "p")]
        [Summary("Returns the prices based on your input! No proper capitalization needed!")]
        public async Task PriceCommand([Remainder] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
            {

                if (!card.TcgExists)
                {

                    await ReplyAsync("Card does not exist in TCG therefore no price can be determined for this card currently!");
                    return;

                }

                using (Context.Channel.EnterTypingState())
                {

                    var response = await Web.GetPrices(card.Name) ?? await Web.GetPrices(card.RealName);

                    if (response is null)
                    {

                        await ReplyAsync($"There was an error in retrieving the prices for \"{input}\". Please try again later.");
                        return;

                    }

                    var data = response.Data.Where(d => string.IsNullOrEmpty(d.PriceData.Message)).ToList();

                    var author = new EmbedAuthorBuilder()
                        .WithIconUrl("https://vignette1.wikia.nocookie.net/yugioh/images/8/82/PotofGreed-TF04-JP-VG.jpg/revision/latest?cb=20120829225457")
                        .WithName("YuGiOh Prices")
                        .WithUrl($"https://yugiohprices.com/card_price?name={Uri.EscapeDataString(input)}");

                    var body = new EmbedBuilder()
                        .WithAuthor(author)
                        .WithColor(new Color(33, 108, 42))
                        .WithCurrentTimestamp();

                    if (data.Count > 25)
                    {

                        body.WithDescription("**There are more than 25 results! Due to that, only the first 25 results are shown!**");
                        data = data.GetRange(0, 25);

                    }

                    body = data.Aggregate(body, (current, datum) => current.AddPrice(datum, true));

                    await SendEmbedAsync(body);

                }

            }
            else
                await NoResultError(input);

        }

        [Command("banlist")]
        [Summary("Get the banlist of a specified format! OCG or 1, TCGADV or 2, TCGTRAD or 3")]
        public async Task BanlistCommand([Remainder] string input)
        {

            BanlistFormats format;

            switch (input.ToLower())
            {

                case "ocg":
                case "1":
                    format = BanlistFormats.OCG;
                    break;
                case "tcg":
                case "2":
                    format = BanlistFormats.TCG;
                    break;
                case "tcgtrad":
                case "trad":
                case "3":
                    format = BanlistFormats.TRAD;
                    break;
                default:
                    format = BanlistFormats.TCG;
                    break;

            }

            var banlist = await YuGiOhDbService.GetBanlistAsync(format);

            if (banlist.Forbidden.Any())
                await DirectMessageAsync(FormatBanlist("Forbidden", banlist.Forbidden));

            if (banlist.Limited.Any())
                await DirectMessageAsync(FormatBanlist("Limited", banlist.Limited));

            if (banlist.SemiLimited.Any())
                await DirectMessageAsync(FormatBanlist("Semi-Limited", banlist.SemiLimited));

            //BanlistFormat banlist;

            //switch (format.ToLower())
            //{

            //    case "ocg":
            //    case "1":
            //        banlist = Cache.Banlist.OcgBanlist;
            //        break;
            //    case "tcgadv":
            //    case "2":
            //        banlist = Cache.Banlist.TcgAdvBanlist;
            //        break;
            //    case "tcgtrad":
            //    case "3":
            //        banlist = Cache.Banlist.TcgTradBanlist;
            //        break;
            //    default:
            //        await ReplyAsync("The valid formats are OCG or 1, TCGADV or 2, TCGTRAD or 3!");
            //        return;

            //}

            //if (banlist.Forbidden.Any())
            //    await DirectMessageAsync("", FormatBanlist("Forbidden", banlist.Forbidden));

            //await DirectMessageAsync("", FormatBanlist("Limited", banlist.Limited));
            //await DirectMessageAsync("", FormatBanlist("Semi-Limited", banlist.SemiLimited));

        }

        public Embed FormatBanlist(string status, IEnumerable<string> cards)
        {

            var descBuilder = new StringBuilder();
            var cardQueue = new Queue<string>(cards);
            var counter = 0;

            while (cardQueue.Count > 0)
            {

                var card = cardQueue.Peek();
                counter += card.Length + 2;

                if (counter >= 2048)
                    break;

                descBuilder.AppendLine(card);
                cardQueue.Dequeue();

            }

            var body = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder().WithName(status))
                .WithRandomColor()
                .WithDescription(descBuilder.ToString());

            while (cardQueue.Count > 0)
            {

                counter = 0;
                var valueBuilder = new StringBuilder();

                do
                {

                    var card = cardQueue.Peek();
                    counter += card.Length + 2;

                    if (counter >= 1024)
                        break;

                    valueBuilder.AppendLine(card);
                    cardQueue.Dequeue();

                } while (cardQueue.Count > 0);

                body.AddField("cont.", valueBuilder.ToString());

            }

            return body.Build();

        }

        private Task<Stream> GetArt(string passcode)
        {

            var url = $"{Constants.ArtBaseUrl}{passcode}.{Constants.ArtFileType}";
            return Web.GetStream(url);

        }

        private string GetArtUrl(string passcode)
            => $"{Constants.ArtBaseUrl}{passcode}.{Constants.ArtFileType}";

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
