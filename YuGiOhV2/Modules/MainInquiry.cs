﻿using Discord;
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
using YuGiOhV2.Objects.Banlist;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class MainInquiry : MainBase
    {

        [Command("booster")]
        [Summary("Gets information on a booster pack!")]
        public Task BoosterCommand([Remainder]string input)
        {

            if (Cache.BoosterPacks.TryGetValue(input, out var boosterPack))
            {

                var builder = new EmbedBuilder()
                    .WithAuthor(boosterPack.Name, url: boosterPack.Url)
                    .WithDescription($"**Amount:** {boosterPack.Cards.Length} cards");

                return SendEmbed(builder);

            }
            else
                return NoResultError("booster packs", input);

        }

        //[Command("booster")]
        //[Summary("Gets information on a booster pack!")]
        //public Task BoosterCommand([Remainder]string input)
        //{

        //    if (Cache.BoosterPacks.TryGetValue(input, out var booster))
        //    {

        //        var cards = booster.Open();
        //        var cardNumberLength = 0;

        //        if (cards.All(card => card.CardNumber != null))
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

        [Command("card"), Alias("c")]
        [Summary("Gets a card! No proper capitalization needed!")]
        public Task CardCommand([Remainder]string input)
        {

            if (Cache.Embeds.TryGetValue(input, out var embed))
                return SendCardEmbed(embed, _minimal, Web);
            else
                return NoResultError(input);

        }

        [Command("random"), Alias("rcard", "r")]
        [Summary("Gets a random card!")]
        public Task RandomCommand()
        {

            var embed = Cache.Embeds.RandomSubset(1).First().Value;

            return SendCardEmbed(embed, _minimal, Web);

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

        //this is so ghetto
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

            if (banlist.Forbidden.Any())
                await DirectMessageAsync("", FormatBanlist("Forbidden", banlist.Forbidden));

            await DirectMessageAsync("", FormatBanlist("Limited", banlist.Limited));
            await DirectMessageAsync("", FormatBanlist("Semi-Limited", banlist.SemiLimited));

        }

        public Embed FormatBanlist(string status, IEnumerable<string> cards)
        {

            var descBuilder = new StringBuilder();
            var cardStack = new Stack<string>(cards);
            var counter = 0;

            while (cardStack.Any())
            {

                var card = cardStack.Peek();
                counter += card.Length + 2;

                if (counter >= 2048)
                    break;

                descBuilder.AppendLine(card);
                cardStack.Pop();

            }

            var body = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder().WithName(status))
                .WithRandomColor()
                .WithDescription(descBuilder.ToString());

            while (cardStack.Any())
            {

                counter = 0;
                var valueBuilder = new StringBuilder();

                do
                {

                    var card = cardStack.Peek();
                    counter += card.Length + 2;

                    if (counter >= 1024)
                        break;

                    valueBuilder.AppendLine(card);
                    cardStack.Pop();

                } while (cardStack.Any());

                body.AddField("cont.", valueBuilder.ToString());

            }

            return body.Build();

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