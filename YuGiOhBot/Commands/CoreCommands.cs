using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Discord.Addons.InteractiveCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhBot.Services;
using YuGiOhBot.Services.CardObjects;
using YuGiOhBot.Core;

namespace YuGiOhBot.Commands
{
    public class CoreCommands : InteractiveModuleBase
    {

        private YuGiOhServices _yugiohService;
        private ChatService _chatService;

        public CoreCommands(YuGiOhServices yugiohServiceParams, ChatService chatServiceParams)
        {

            _yugiohService = yugiohServiceParams;
            _chatService = chatServiceParams;

        }

        [Command("card"), Alias("c")]
        [Summary("Returns information based on given card name")]
        public async Task CardCommand([Remainder]string cardName)
        {

            if (string.IsNullOrEmpty(cardName))
            {

                await ReplyAsync("There is no card with no name, unless it's the shadow realm. Everyone is just a soul there.");
                return;

            }

            if (CacheService.YuGiOhCardCache.TryGetValue(cardName.ToLower(), out EmbedBuilder eBuilder))
            {

                if(GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool minimal) && minimal)
                {

                    if (minimal)
                    {
                        string imgUrl = eBuilder.ImageUrl;

                        if (!string.IsNullOrEmpty(imgUrl))
                        {
                            eBuilder.ImageUrl = null;
                            eBuilder.ThumbnailUrl = imgUrl;
                        }
                    }

                }
                else
                {

                    string thumbUrl = eBuilder.ThumbnailUrl;

                    if (!string.IsNullOrEmpty(thumbUrl))
                    {
                        eBuilder.ThumbnailUrl = null;
                        eBuilder.ImageUrl = thumbUrl;
                    }

                }

                await ReplyAsync("", embed: eBuilder);
                return;

            }

            using (var typingState = Context.Channel.EnterTypingState())
            {

                YuGiOhCard card = await _yugiohService.GetCard(cardName);

                if (string.IsNullOrEmpty(card.Name))
                {

                    await ReplyAsync($"No card by the name {cardName} was found!");
                    typingState.Dispose();
                    return;

                }

                if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool minimal)) { }
                else minimal = false;

                await _chatService.SendCard(Context.Channel, card, minimal);

            }

        }

        [Command("lcard"), Alias("lc")]
        [Summary("Word position does not matter and will pull the first available result in the search")]
        public async Task LazyCardCommand([Remainder]string cardName)
        {

            if (string.IsNullOrEmpty(cardName))
            {

                await ReplyAsync("There are no cards with a lack of name.");
                return;

            }

            EmbedBuilder eBuilder;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                YuGiOhCard card = await _yugiohService.LazyGetCard(cardName);

                if (card.Name.Equals(string.Empty))
                {

                    await ReplyAsync($"No card by the name {cardName} was found!");
                    typingState.Dispose();
                    return;

                }

                if (CacheService.YuGiOhCardCache.TryGetValue(card.Name.ToLower(), out eBuilder))
                {

                    if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool min) && min)
                    {

                        if (min)
                        {
                            string imgUrl = eBuilder.ImageUrl;

                            if (!string.IsNullOrEmpty(imgUrl))
                            {
                                eBuilder.ImageUrl = null;
                                eBuilder.ThumbnailUrl = imgUrl;
                            }
                        }

                    }
                    else
                    {

                        string thumbUrl = eBuilder.ThumbnailUrl;

                        if (!string.IsNullOrEmpty(thumbUrl))
                        {
                            eBuilder.ThumbnailUrl = null;
                            eBuilder.ImageUrl = thumbUrl;
                        }

                    }

                    await ReplyAsync("", embed: eBuilder);
                    return;

                }

                if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool minimal)) { }
                else minimal = false;

                await _chatService.SendCard(Context.Channel, card, minimal);

            }

        }

        [Command("rcard"), Alias("r", "random", "rc")]
        [Summary("Returns a random card!")]
        public async Task RandomCardCommand()
        {

            EmbedBuilder eBuilder;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                YuGiOhCard card = await _yugiohService.GetRandomCard();

                await AltConsole.PrintAsync("Command", "Random Card", $"Got {card.Name}");

                if (CacheService.YuGiOhCardCache.TryGetValue(card.Name.ToLower(), out eBuilder))
                {

                    if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool min) && min)
                    {

                        if (min)
                        {
                            string imgUrl = eBuilder.ImageUrl;

                            if (!string.IsNullOrEmpty(imgUrl))
                            {
                                eBuilder.ImageUrl = null;
                                eBuilder.ThumbnailUrl = imgUrl;
                            }
                        }

                    }
                    else
                    {

                        string thumbUrl = eBuilder.ThumbnailUrl;

                        if (!string.IsNullOrEmpty(thumbUrl))
                        {
                            eBuilder.ThumbnailUrl = null;
                            eBuilder.ImageUrl = thumbUrl;
                        }

                    }

                }

                if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool minimal)) { }
                else minimal = false;

                await _chatService.SendCard(Context.Channel, card, minimal);

            }            

        }

        [Command("search", RunMode = RunMode.Async), Alias("s")]
        [Summary("Searches for cards based on name given")]
        public async Task CardSearchCommand([Remainder]string search)
        {

            if (string.IsNullOrEmpty(search))
            {

                await ReplyAsync("Please search for something or else I will search the purple realm.");
                return;

            }

            StringBuilder organizedResults;
            List<string> searchResults;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                //<card names>
                searchResults = await _yugiohService.SearchCards(search);

                if (searchResults.FirstOrDefault() == null)
                {

                    await ReplyAsync($"Nothing was found with the search of {search}!");
                    typingState.Dispose();
                    return;

                }
                else if (searchResults.Count > 50)
                {

                    await ReplyAsync($"Too many results were returned, please refine your search!");
                    typingState.Dispose();
                    return;

                }

                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
                organizedResults = new StringBuilder(str);
                var counter = 1;

                foreach (string card in searchResults)
                {

                    organizedResults.AppendLine($"{counter}. {card}");
                    counter++;

                }

                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");
                typingState.Dispose();
            }

            await ReplyAsync(organizedResults.ToString());

            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

            if (!int.TryParse(response.Content, out int searchNumber)) return;
            else if (searchNumber > searchResults.Count)
            {

                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
                return;

            }


            await CardCommand(searchResults[searchNumber - 1]);

        }

        [Command("lsearch", RunMode = RunMode.Async), Alias("ls")]
        [Summary("A lazy search of all cards that CONTAIN the words entered, it may be not be in any particular order")]
        public async Task LazySearchCommand([Remainder]string search)
        {

            if (string.IsNullOrEmpty(search))
            {

                await ReplyAsync("Please don't enter the Shadow Realm as a search.");
                return;

            }

            StringBuilder organizedResults;
            List<string> searchResults;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                //<card names>
                searchResults = await _yugiohService.LazySearchCards(search);

                if (searchResults.FirstOrDefault() == null)
                {

                    await ReplyAsync($"Nothing was found with the search of {search}!");
                    typingState.Dispose();
                    return;

                }
                else if (searchResults.Count > 50)
                {

                    await ReplyAsync($"Too many results were returned, please refine your search!");
                    typingState.Dispose();
                    return;

                }

                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
                organizedResults = new StringBuilder(str);
                var counter = 1;

                foreach (string card in searchResults)
                {

                    organizedResults.AppendLine($"{counter}. {card}");
                    counter++;

                }

                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");

            }

            await ReplyAsync(organizedResults.ToString());

            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

            if (!int.TryParse(response.Content, out int searchNumber)) return;
            else if (searchNumber > searchResults.Count)
            {

                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
                return;

            }

            await CardCommand(searchResults[searchNumber - 1]);

        }

        [Command("archetype", RunMode = RunMode.Async), Alias("a", "arch")]
        [Summary("Attempt to search all cards associated with searched archetype")]
        public async Task ArchetypeSearchCommand([Remainder]string archetype)
        {

            if (string.IsNullOrEmpty(archetype))
            {

                await ReplyAsync("There is no archetype with a blank name, unless it's Marik's fantasies.");
                return;

            }

            StringBuilder organizedResults;
            List<string> searchResults;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                searchResults = await _yugiohService.ArchetypeSearch(archetype);

                if (searchResults.Count == 0)
                {

                    await ReplyAsync($"Nothing was found with the search of {archetype}!");
                    typingState.Dispose();
                    return;

                }
                else if (searchResults.Count > 50)
                {

                    await ReplyAsync($"Too many results were returned, please refine your search!");
                    typingState.Dispose();
                    return;

                }

                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
                organizedResults = new StringBuilder(str);
                var counter = 1;

                foreach (string card in searchResults)
                {

                    organizedResults.AppendLine($"{counter}. {card}");
                    counter++;

                }

                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");

            }

            await ReplyAsync(organizedResults.ToString());

            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

            if (!int.TryParse(response.Content, out int searchNumber)) return;
            else if (searchNumber > searchResults.Count)
            {

                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
                return;

            }

            await CardCommand(searchResults[searchNumber - 1]);

        }

        [Command("banlist"), Alias("bans")]
        [Summary("Returns the current banlist")]
        public async Task BanListCommand(string format = "")
        {

            if (string.IsNullOrEmpty(format) || !int.TryParse(format, out int formatInt))
            {

                await ReplyAsync("That is not a valid format! ```http\n1 -> OCG\n2 -> TCG ADV\n3 -> TCG TRN```");
                return;

            }

            if (!CacheService.DMChannelCache.TryGetValue(Context.User.Id, out IDMChannel channel))
            {

                var dm = await Context.User.CreateDMChannelAsync();
                CacheService.DMChannelCache.TryAdd(Context.User.Id, dm);
                channel = dm;

            }

            await ReplyAsync("Sending your dms to the purple realm...");

            switch (formatInt)
            {

                case 1:
                    {

                        var forbidden = $"```OCG Forbidden\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Forbidden", out List<string> forbiddenList);
                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
                        forbidden += "```";

                        var limited = $"```OCG Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Limited", out List<string> limitedList);
                        limitedList.ForEach(card => limited += $"{card}\n");
                        limited += "```";

                        var semilimited = $"```OCG Semi-Limited\n";
                        _yugiohService.OcgBanList.TryGetValue("Semi-Limited", out List<string> semiList);
                        semiList.ForEach(card => semilimited += $"{card}\n");
                        semilimited += "```";

                        await channel.SendMessageAsync(forbidden);
                        await channel.SendMessageAsync(limited);
                        await channel.SendMessageAsync(semilimited);
                        break;

                    }
                case 2:
                    {

                        var forbidden = $"```TCG ADV Forbidden\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Forbidden", out List<string> forbiddenList);
                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
                        forbidden += "```";

                        var limited = $"```TCG ADV Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Limited", out List<string> limitedList);
                        limitedList.ForEach(card => limited += $"{card}\n");
                        limited += "```";

                        var semilimited = $"```TCG ADV Semi-Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Semi-Limited", out List<string> semiList);
                        semiList.ForEach(card => semilimited += $"{card}\n");
                        semilimited += "```";

                        await channel.SendMessageAsync(forbidden);
                        await channel.SendMessageAsync(limited);
                        await channel.SendMessageAsync(semilimited);
                        break;

                    }
                case 3:
                    {

                        var forbidden = $"```TCG Traditional Forbidden\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Forbidden", out List<string> forbiddenList);
                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
                        forbidden += "```";

                        var limited = $"```TCG Traditional Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Limited", out List<string> limitedList);
                        limitedList.ForEach(card => limited += $"{card}\n");
                        limited += "```";

                        var semilimited = $"```TCG Traditional Semi-Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Semi-Limited", out List<string> semiList);
                        semiList.ForEach(card => semilimited += $"{card}\n");
                        semilimited += "```";

                        await channel.SendMessageAsync(forbidden);
                        await channel.SendMessageAsync(limited);
                        await channel.SendMessageAsync(semilimited);
                        break;

                    }
                default:
                    break;

            }

        }

    }
}
