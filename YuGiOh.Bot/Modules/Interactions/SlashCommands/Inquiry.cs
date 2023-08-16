using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Modules.Interactions.Autocompletes;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Inquiry : MainInteractionBase<SocketSlashCommand>
    {

        public Inquiry(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            InteractiveService interactiveService
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, interactiveService) { }

        [SlashCommand(Constants.CardCommand, "Gets a card")]
        public async Task CardCommand([Autocomplete(typeof(CardAutocomplete)), Summary(description: "The card")] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await SendCardEmbedAsync(card.GetEmbedBuilder(), GuildConfig.Minimal);
            else
                await NoResultError(input);

        }

        [SlashCommand("random", "Gets a random card")]
        public async Task RandomCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await SendCardEmbedAsync(card.GetEmbedBuilder(), GuildConfig.Minimal);

        }

        [SlashCommand("image", "Gets the image of the card based input")]
        public async Task ImageCommand([Autocomplete(typeof(CardAutocomplete)), Summary(description: "The card")] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await UploadImage(card.Name, card.Img);
            else
                await NoResultError(input);

        }

        [SlashCommand("randomimage", "Gets a random card image")]
        public async Task RandomImageCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await UploadImage(card.Name, card.Img);

        }

        [SlashCommand("art", "Gets the art of a card based on input")]
        public async Task ArtCommand([Autocomplete(typeof(CardAutocomplete)), Summary(description: "The card")] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await UploadImage(card.Name, card.GetArtUrl());
            else
                await NoResultError(input);

        }

        [SlashCommand("randomart", "Gets a random card art")]
        public async Task RandomArtCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await UploadImage(card.Name, card.GetArtUrl());

        }

        private async Task UploadImage(string name, string url)
        {

            try
            {

                Log("Attempting to upload {CardName}...", name);

                var stream = await Web.GetStream(url);

                await UploadAsync(stream, $"{Uri.EscapeDataString(name)}.png");

                Log("Uploaded {CardName}", name);

            }
            catch
            {

                const string response = "There was a problem while uploading the image, please try again later.";

                if (IsDeferred)
                    await FollowupAsync(response);
                else
                    await RespondAsync(response);

            }

        }

        [SlashCommand("price", "Returns the prices of a card based on your input")]
        public async Task PriceCommand([Autocomplete(typeof(CardAutocomplete)), Summary(description: "The card")] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
            {

                if (!card.TcgExists)
                {

                    await RespondAsync("Card does not exist in TCG therefore no price can be determined for this card currently!");
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

        [SlashCommand("banlist", "Get the banlist of a specified format")]
        public async Task BanlistCommand(Banlist input)
        {

            var format = input switch
            {
                Banlist.Ocg => BanlistFormats.OCG,
                Banlist.TcgAdv => BanlistFormats.TCG,
                Banlist.TcgTrad => BanlistFormats.TRAD,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
            };

            var banlist = await YuGiOhDbService.GetBanlistAsync(format);

            if (banlist.Forbidden.Any())
                await DirectMessageAsync(FormatBanlist("Forbidden", banlist.Forbidden));

            if (banlist.Limited.Any())
                await DirectMessageAsync(FormatBanlist("Limited", banlist.Limited));

            if (banlist.SemiLimited.Any())
                await DirectMessageAsync(FormatBanlist("Semi-Limited", banlist.SemiLimited));

            await RespondAsync("The banlist has been directly messaged to you");

        }

        private static Embed FormatBanlist(string status, IEnumerable<string> cards)
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

    }

    public enum Banlist
    {

        [ChoiceDisplay("OCG")]
        Ocg,

        [ChoiceDisplay("TCG-Regular")]
        TcgAdv,

        [ChoiceDisplay("TCG-Traditional")]
        TcgTrad

    }
}