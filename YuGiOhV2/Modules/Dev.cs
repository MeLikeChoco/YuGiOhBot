using Discord;
using Discord.Commands;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Models.Attributes;

namespace YuGiOhV2.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireChannel(541938684438511616)]
    public class Dev : MainBase
    {

        private static readonly DateTime _cutOffDate = new DateTime(2016, 1, 14);

        [Command("price"), Alias("prices", "p")]
        [Summary("Returns the prices based on your deck list from ygopro! No proper capitalization needed!")]
        public async Task DeckPriceCommand()
        {

            var attachments = Context.Message.Attachments;
            var file = attachments.FirstOrDefault(attachment => Path.GetExtension(attachment.Filename) == ".ydk");

            if (file != null)
            {

                var url = file.Url;
                var stream = await Web.GetStream(url);
                var buffer = new byte[stream.Length];

                await stream.ReadAsync(buffer, 0, (int)stream.Length);

                var text = Encoding.UTF8.GetString(buffer);
                var passcodes = text.Replace("#main", "")
                    .Replace("#extra", "")
                    .Replace("#created by ...", "")
                    .Replace("!side", "")
                    .Split('\n')
                    .Select(passcode => passcode.Trim())
                    .Where(passcode => !string.IsNullOrEmpty(passcode))
                    .ToArray();
                //var passcodes = text
                //    .Split('\n')
                //    .Select(passcode => passcode.Trim())
                //    .ToArray();
                //var main = GetSection(passcodes, "#main", "#extra");
                //var extra = GetSection(passcodes, "#extra", "!side");
                //var side = GetSection(passcodes, "!side", null);

                if (passcodes.Any())
                {

                    var tasks = passcodes
                        .Where(name => name != "YuGiOh Wikia!")
                        .GroupBy(passcode => passcode)
                        .Select(GetName);
                    //var tasks = main.GroupBy(passcode => passcode).Select(GetName);
                    //tasks = tasks.Concat(extra.GroupBy(passcode => passcode).Select(GetName));
                    //tasks = tasks.Concat(extra.GroupBy(passcode => passcode).Select(GetName));

                    var cards = await Task.WhenAll(tasks);

                    await ReplyAsync($"```{cards.Aggregate("", (current, next) => $"{current}\n{next}")}```");

                }
                else
                    await NoResultError("cards", file.Filename);

            }
            else
                await NoResultError("ydk files");

        }

        private string[] GetSection(string[] deck, string startSection, string endSection)
        {

            var startIndex = Array.IndexOf(deck, startSection) + 1;
            var endIndex = string.IsNullOrEmpty(endSection) ? deck.Length - 1 : Array.IndexOf(deck, endSection);
            var count = endIndex - startIndex;

            return deck.Slice(startIndex, count).ToArray();

        }

        private async Task<(string name, int count, double price)> GetName(IGrouping<string, string> group)
        {

            var passcode = group.First();

            if (!Cache.PasscodeToName.TryGetValue(passcode, out var name))
            {
                
                name = (await Web.GetResponseMessage($"{Constants.FandomWikiUrl}{passcode}")).RequestMessage.RequestUri.Segments.Last().Replace('_', ' ');
                name = WebUtility.UrlDecode(name);

            }

            return (name, group.Count(), double.Epsilon);

        }

        [Command("booster")]
        [Summary("Gets information on a booster pack!")]
        public Task BoosterCommand([Remainder]string input)
        {



            if (Cache.BoosterPacks.TryGetValue(input, out var boosterPack))
            {

                var builder = new EmbedBuilder()
                    .WithAuthor(boosterPack.Name, url: boosterPack.Url)
                    .WithDescription($"**Amount:** {boosterPack.Cards.Length} cards")
                    .WithColor(Rand.NextColor())
                    .AddField("Release dates", boosterPack.ReleaseDates.Aggregate("", (current, kv) => $"{current}\n**{kv.Key}:** {kv.Value.ToString("MM/dd/yyyy")}"));

                foreach (var kv in boosterPack.RarityToCards)
                    builder.AddField(kv.Key, kv.Value.Aggregate("```", (current, next) => $"{current}\n{next}") + "```");

                return SendEmbed(builder);

            }
            else
                return NoResultError("booster packs", input);

        }

        [Command("open")]
        public Task OpenCommand([Remainder]string input)
        {

            if (Cache.BoosterPacks.TryGetValue(input, out var boosterPack))
            {

                var cards = new Dictionary<string, string>(9);
                var randoms = new List<int>(9);
                var commonCards = boosterPack.Commons.Length;
                var builder = new StringBuilder("```fix\n");
                int index;

                for (int i = 0; i < 7; i++)
                {

                    do
                        index = Rand.Next(commonCards);
                    while (randoms.Contains(index));

                    cards.Add(boosterPack.Commons[index], "Common");
                    randoms.Add(index);

                }

                var superRare = boosterPack.Foils.RandomSubset(1, Rand).FirstOrDefault();
                cards.Add(boosterPack.Rares.RandomSubset(1, Rand).FirstOrDefault(), "Rare");
                cards.Add(superRare.Value.RandomSubset(1, Rand).FirstOrDefault(), superRare.Key);

                foreach (var card in cards)
                {

                    builder.AppendLine($"Name: {card.Key}");
                    builder.AppendLine($"Rarity: {card.Value}");
                    builder.AppendLine();

                }

                builder.Append("```");

                return ReplyAsync(builder.ToString());

            }
            else
                return NoResultError("booster packs", input);

        }

    }
}
