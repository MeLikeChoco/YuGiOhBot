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
using MoreLinq;

namespace YuGiOhBot.Commands
{
    public class CoreCommands : InteractiveModuleBase<SocketCommandContext>
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

            if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool isMinimal)) { }
            else isMinimal = false;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                if(CacheService.CardCache.TryGetValue(cardName.ToLower(), out EmbedBuilder eBuilder))
                {

                    await ReplyAsync("", embed: await _chatService.AddPriceAndImage(eBuilder, isMinimal));

                }
                else
                {

                    await ReplyAsync($"No card by the name {cardName} was found!");
                    return;

                }

            }

        }

        [Command("lcard"), Alias("lc")]
        [Summary("Word position does not matter and will pull the first available result in the search")]
        public async Task LazyCardCommand([Remainder]string cardName)
        {

            using (var typingState = Context.Channel.EnterTypingState())
            {

                EmbedBuilder eBuilder = _yugiohService.LazyGetCard(cardName.ToLower());

                if (eBuilder == null)
                {

                    await ReplyAsync($"No card by the name {cardName} was found!");
                    typingState.Dispose();
                    return;

                }

                if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool isMinimal)) { }
                else isMinimal = false;

                await ReplyAsync("", embed: await _chatService.AddPriceAndImage(eBuilder, isMinimal));

            }

        }

        [Command("rcard"), Alias("r", "random", "rc")]
        [Summary("Returns a random card!")]
        public async Task RandomCardCommand()
        {

            using (var typingState = Context.Channel.EnterTypingState())
            {

                EmbedBuilder eBuilder = _yugiohService.GetRandomCard();

                await AltConsole.PrintAsync("Command", "Random Card", $"Got {eBuilder.Title}");

                if (GuildServices.MinimalSettings.TryGetValue(Context.Guild.Id, out bool isMinimal)) { }
                else isMinimal = false;

                await ReplyAsync("", embed: await _chatService.AddPriceAndImage(eBuilder, isMinimal));

            }            

        }

        [Command("search", RunMode = RunMode.Async), Alias("s")]
        [Summary("Searches for cards based on name given")]
        public async Task CardSearchCommand([Remainder]string search)
        {

            StringBuilder organizedResults;
            List<string> searchResults;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                //<card names>
                searchResults = _yugiohService.SearchCards(search.ToLower());

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
                searchResults = _yugiohService.LazySearchCards(search);

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

            var channel = await Context.User.CreateDMChannelAsync();

            await ReplyAsync("Sending your dms to the purple realm...");

            switch (formatInt)
            {

                case 1:
                    {

                        var forbidden = $"```OCG Forbidden\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Forbidden", out HashSet<string> forbiddenList);
                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
                        forbidden += "```";

                        var limited = $"```OCG Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Limited", out HashSet<string> limitedList);
                        limitedList.ForEach(card => limited += $"{card}\n");
                        limited += "```";

                        var semilimited = $"```OCG Semi-Limited\n";
                        _yugiohService.OcgBanList.TryGetValue("Semi-Limited", out HashSet<string> semiList);
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
                        _yugiohService.OcgBanList.TryGetValue("Forbidden", out HashSet<string> forbiddenList);
                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
                        forbidden += "```";

                        var limited = $"```TCG ADV Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Limited", out HashSet<string> limitedList);
                        limitedList.ForEach(card => limited += $"{card}\n");
                        limited += "```";

                        var semilimited = $"```TCG ADV Semi-Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Semi-Limited", out HashSet<string> semiList);
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
                        _yugiohService.OcgBanList.TryGetValue("Forbidden", out HashSet<string> forbiddenList);
                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
                        forbidden += "```";

                        var limited = $"```TCG Traditional Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Limited", out HashSet<string> limitedList);
                        limitedList.ForEach(card => limited += $"{card}\n");
                        limited += "```";

                        var semilimited = $"```TCG Traditional Semi-Limited\n\n";
                        _yugiohService.OcgBanList.TryGetValue("Semi-Limited", out HashSet<string> semiList);
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
