using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Models.Autocompletes;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Inquiry : MainInteractionBase<SocketSlashCommand>
    {

        public Inquiry(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web) { }

        [SlashCommand(Constants.CardCommand, "Gets a card! No proper capitalization needed!")]
        public async Task CardCommand([Autocomplete(typeof(CardAutocomplete))] [Summary(description: "The card")] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await SendCardEmbedAsync(card.GetEmbedBuilder(), GuildConfig.Minimal);
            else
                await NoResultError(input);

        }

        [SlashCommand("random", "Gets a random card!")]
        public async Task RandomCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await SendCardEmbedAsync(card.GetEmbedBuilder(), GuildConfig.Minimal);

        }

        [SlashCommand("image", "Gets the image of the card based input! No proper capitalization needed!")]
        public async Task ImageCommand([Autocomplete(typeof(CardAutocomplete))] [Summary(description: "The card")] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await UploadImage(card.Name, card.Img);
            else
                await NoResultError(input);

        }

        [SlashCommand("randomimage", "Gets a random card image!")]
        public async Task RandomImageCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await UploadImage(card.Name, card.Img);

        }

        [SlashCommand("art", "Gets the art of a card based on input! No proper capitalization needed!")]
        public async Task ArtCommand([Autocomplete(typeof(CardAutocomplete))] [Summary(description: "The card")] string input)
        {

            var card = await YuGiOhDbService.GetCardAsync(input);

            if (card is not null)
                await UploadImage(card.Name, card.GetArtUrl());
            else
                await NoResultError(input);

        }

        [SlashCommand("randomart", "Gets a random card art!")]
        public async Task RandomArtCommand()
        {

            var card = await YuGiOhDbService.GetRandomCardAsync();

            await UploadImage(card.Name, card.GetArtUrl());

        }

        private async Task UploadImage(string name, string url)
        {

            try
            {

                Log($"Attempting to upload \"{name}\"...");

                var stream = await Web.GetStream(url);

                await UploadAsync(stream, $"{Uri.EscapeDataString(name)}.png");

                Log($"Uploaded \"{name}\"");

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

    }
}