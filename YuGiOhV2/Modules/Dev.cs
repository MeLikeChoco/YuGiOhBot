using Discord;
using Discord.Commands;
using MoreLinq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Models.Attributes;
using YuGiOhV2.Models.BoosterPacks;
using YuGiOhV2.Models.Cards;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireChannel(541938684438511616)]
    public class Dev : MainBase
    {

        private static readonly DateTime _cutOffDate = new DateTime(2016, 1, 14);

        [Command("test")]
        public async Task TestCommand()
        {

            Environment.Exit(0);

        }

        [Command("buy"), Alias("b")]
        [Summary("Submits the decklist to massbuy on Tcgplayer!")]
        public async Task BuyCommand()
        {

            var attachments = Context.Message.Attachments;

            if (attachments.Count == 0)
                return;

            var file = attachments.FirstOrDefault(attachment => Path.GetExtension(attachment.Filename) == ".ydk");

            if (file == null)
            {

                await ReplyAsync("Invalid file provided! Must be a ydk or text file!");
                return;

            }

            var url = file.Url;
            string text;

            using (var stream = await Web.GetStream(url))
            {

                var buffer = new byte[stream.Length];

                await stream.ReadAsync(buffer, 0, (int)stream.Length);

                text = Encoding.UTF8.GetString(buffer);

            }

            var cards = text.Replace("#main", "")
                    .Replace("#extra", "")
                    .Replace("#created by ...", "")
                    .Replace("!side", "")
                    .Split('\n')
                    .Select(passcode => passcode.Trim())
                    .Where(passcode => !string.IsNullOrEmpty(passcode))
                    .Select(PasscodeToName)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .GroupBy(name => name)
                    .Aggregate(new StringBuilder(), (builder, group) => builder.Append("||").Append(Uri.EscapeDataString($"{group.Count()} {group.First()}")))
                    .ToString();

            url = $"http://store.tcgplayer.com/massentry?productline=YuGiOh&c={cards}";
            var response = await Web.Post("https://api-ssl.bitly.com/v4/shorten", $"{{\"long_url\": \"{url}\"}}", "Bearer", Cache.BitlyKey, Web.ContentType.Json);
            url = JObject.Parse(await response.Content.ReadAsStringAsync())["link"].Value<string>();

            await ReplyAsync(url);

        }

        private string PasscodeToName(string passcode)
        {

            Cache.PasscodeToName.TryGetValue(passcode, out var name);

            return name;

        }

        [Command("price"), Alias("prices", "p")]
        [Summary("Returns the prices based on your deck list from ygopro! No proper capitalization needed!")]
        public async Task DeckPriceCommand()
        {

            var attachments = Context.Message.Attachments;
            var file = attachments.FirstOrDefault(attachment => Path.GetExtension(attachment.Filename) == ".ydk");

            if (file != null)
            {

                var url = file.Url;
                string text;

                using (var stream = await Web.GetStream(url))
                {

                    var buffer = new byte[stream.Length];

                    await stream.ReadAsync(buffer, 0, (int)stream.Length);

                    text = Encoding.UTF8.GetString(buffer);

                }

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

    }
}
