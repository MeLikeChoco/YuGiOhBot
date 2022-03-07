using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json.Linq;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Attributes;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Repositories.Interfaces;
using ContextType = Discord.Commands.ContextType;

namespace YuGiOh.Bot.Modules.Commands
{
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [RequireChannel(541938684438511616)]
    public class Dev : MainBase
    {

        private readonly InteractionService _interactionService;
        private readonly IYuGiOhRepository _yugiohRepo;

        public Dev(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand,
            InteractionService interactionService,
            IYuGiOhRepository yugiohRepo
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, rand)
        {
            _interactionService = interactionService;
            _yugiohRepo = yugiohRepo;
        }

        private static readonly DateTime _cutOffDate = new DateTime(2016, 1, 14);

        [Command("json")]
        public async Task JsonCommand([Remainder] string input)
        {

            var entity = await _yugiohRepo.GetCardAsync(input);
            var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            Logger.Info(json);

            await ReplyAsync("Json printed to console");

        }

        [Command("test")]
        public async Task TestCommand(bool isGlobal = false)
        {

            //var cmds = new[]
            //{

            //    new SlashCommandBuilder()
            //        .WithName("help")
            //        .WithDescription("Defacto help command")
            //        .AddOption("input", ApplicationCommandOptionType.String, "Specific cmd", isRequired: false, isAutocomplete: true)
            //        .Build(),
            //    new SlashCommandBuilder()
            //        .WithName("card")
            //        .WithDescription("Gets a card! No proper capitalization needed!")
            //        .AddOption("input", ApplicationCommandOptionType.String, "The card", isRequired: true, isAutocomplete: true)
            //        .Build(),
            //    new SlashCommandBuilder()
            //        .WithName("art")
            //        .WithDescription("Gets the art of a card based on input! No proper capitalization needed!")
            //        .AddOption("input", ApplicationCommandOptionType.String, "The card", isRequired: true, isAutocomplete: true)
            //        .Build(),
            //    new SlashCommandBuilder()
            //        .WithName("random")
            //        .WithDescription("Gets a random card!")
            //        .Build()

            //};

            var cmds = _interactionService.Modules.ToArray();

            try
            {

                //await InteractionService.AddModulesToGuildAsync(Context.Guild, true, InteractionService.Modules.ToArray());

                if (isGlobal)
                    //await Context.Client.Rest.CreateOrOverwriteBulkGlobalApplicationCommands(cmds);
                    await _interactionService.AddModulesGloballyAsync(true, cmds);
                else
                    //await Context.Guild.CreateOrOverwriteApplicationCommands(cmds);
                    await _interactionService.AddModulesToGuildAsync(Context.Guild, true, cmds);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, string.Empty);
            }

        }

        //[Command("guesst")]
        //public async Task GuessTCommand(double factor = 0.1, bool smoothing = true)
        //{

        //    var card = await YuGiOhDbService.GetRandomCardAsync();

        //    Console.WriteLine(card.Name);

        //    var url = $"{Constants.ArtBaseUrl}{card.Passcode}.{Constants.ArtFileType}";

        //    using var ogStream = await Web.GetStream(url);
        //    using var ogBitmap = new Bitmap(ogStream);
        //    using var newStream = new MemoryStream();

        //    Swirl(ogBitmap, factor, smoothing);
        //    ogBitmap.Save(newStream, System.Drawing.Imaging.ImageFormat.Png);

        //    ogStream.Seek(0, SeekOrigin.Begin);
        //    newStream.Seek(0, SeekOrigin.Begin);

        //    await UploadAsync(ogStream, "test.png");
        //    await UploadAsync(newStream, "test.png");

        //}

        //public struct FloatPoint
        //{
        //    public double X;
        //    public double Y;
        //}

        //public static bool Swirl(Bitmap b, double fDegree, bool bSmoothing /* default fDegree to .05 */)
        //{
        //    int nWidth = b.Width;
        //    int nHeight = b.Height;

        //    FloatPoint[,] fp = new FloatPoint[nWidth, nHeight];
        //    Point[,] pt = new Point[nWidth, nHeight];

        //    Point mid = new Point();
        //    mid.X = nWidth / 2;
        //    mid.Y = nHeight / 2;

        //    double theta, radius;
        //    double newX, newY;

        //    for (int x = 0; x < nWidth; ++x)
        //        for (int y = 0; y < nHeight; ++y)
        //        {
        //            int trueX = x - mid.X;
        //            int trueY = y - mid.Y;
        //            theta = Math.Atan2((trueY), (trueX));

        //            radius = Math.Sqrt(trueX * trueX + trueY * trueY);

        //            newX = mid.X + (radius * Math.Cos(theta + fDegree * radius));
        //            if (newX > 0 && newX < nWidth)
        //            {
        //                fp[x, y].X = newX;
        //                pt[x, y].X = (int)newX;
        //            }
        //            else
        //                fp[x, y].X = pt[x, y].X = x;

        //            newY = mid.Y + (radius * Math.Sin(theta + fDegree * radius));
        //            if (newY > 0 && newY < nHeight)
        //            {
        //                fp[x, y].Y = newY;
        //                pt[x, y].Y = (int)newY;
        //            }
        //            else
        //                fp[x, y].Y = pt[x, y].Y = y;
        //        }

        //    if (bSmoothing)
        //        OffsetFilterAntiAlias(b, fp);
        //    else
        //        OffsetFilterAbs(b, pt);

        //    return true;
        //}

        //public static bool OffsetFilterAntiAlias(Bitmap b, FloatPoint[,] fp)
        //{
        //    Bitmap bSrc = (Bitmap)b.Clone();

        //    // GDI+ still lies to us - the return format is BGR, NOT RGB.
        //    BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //    BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //    int scanline = bmData.Stride;

        //    System.IntPtr Scan0 = bmData.Scan0;
        //    System.IntPtr SrcScan0 = bmSrc.Scan0;

        //    unsafe
        //    {
        //        byte* p = (byte*)(void*)Scan0;
        //        byte* pSrc = (byte*)(void*)SrcScan0;

        //        int nOffset = bmData.Stride - b.Width * 3;
        //        int nWidth = b.Width;
        //        int nHeight = b.Height;

        //        double xOffset, yOffset;

        //        double fraction_x, fraction_y, one_minus_x, one_minus_y;
        //        int ceil_x, ceil_y, floor_x, floor_y;
        //        Byte p1, p2;

        //        for (int y = 0; y < nHeight; ++y)
        //        {
        //            for (int x = 0; x < nWidth; ++x)
        //            {
        //                xOffset = fp[x, y].X;
        //                yOffset = fp[x, y].Y;

        //                // Setup

        //                floor_x = (int)Math.Floor(xOffset);
        //                floor_y = (int)Math.Floor(yOffset);
        //                ceil_x = floor_x + 1;
        //                ceil_y = floor_y + 1;
        //                fraction_x = xOffset - floor_x;
        //                fraction_y = yOffset - floor_y;
        //                one_minus_x = 1.0 - fraction_x;
        //                one_minus_y = 1.0 - fraction_y;

        //                if (floor_y >= 0 && ceil_y < nHeight && floor_x >= 0 && ceil_x < nWidth)
        //                {
        //                    // Blue

        //                    p1 = (Byte)(one_minus_x * (double)(pSrc[floor_y * scanline + floor_x * 3]) +
        //                        fraction_x * (double)(pSrc[floor_y * scanline + ceil_x * 3]));

        //                    p2 = (Byte)(one_minus_x * (double)(pSrc[ceil_y * scanline + floor_x * 3]) +
        //                        fraction_x * (double)(pSrc[ceil_y * scanline + 3 * ceil_x]));

        //                    p[x * 3 + y * scanline] = (Byte)(one_minus_y * (double)(p1) + fraction_y * (double)(p2));

        //                    // Green

        //                    p1 = (Byte)(one_minus_x * (double)(pSrc[floor_y * scanline + floor_x * 3 + 1]) +
        //                        fraction_x * (double)(pSrc[floor_y * scanline + ceil_x * 3 + 1]));

        //                    p2 = (Byte)(one_minus_x * (double)(pSrc[ceil_y * scanline + floor_x * 3 + 1]) +
        //                        fraction_x * (double)(pSrc[ceil_y * scanline + 3 * ceil_x + 1]));

        //                    p[x * 3 + y * scanline + 1] = (Byte)(one_minus_y * (double)(p1) + fraction_y * (double)(p2));

        //                    // Red

        //                    p1 = (Byte)(one_minus_x * (double)(pSrc[floor_y * scanline + floor_x * 3 + 2]) +
        //                        fraction_x * (double)(pSrc[floor_y * scanline + ceil_x * 3 + 2]));

        //                    p2 = (Byte)(one_minus_x * (double)(pSrc[ceil_y * scanline + floor_x * 3 + 2]) +
        //                        fraction_x * (double)(pSrc[ceil_y * scanline + 3 * ceil_x + 2]));

        //                    p[x * 3 + y * scanline + 2] = (Byte)(one_minus_y * (double)(p1) + fraction_y * (double)(p2));
        //                }
        //            }
        //        }
        //    }

        //    b.UnlockBits(bmData);
        //    bSrc.UnlockBits(bmSrc);

        //    return true;
        //}

        //public static bool OffsetFilterAbs(Bitmap b, Point[,] offset)
        //{
        //    Bitmap bSrc = (Bitmap)b.Clone();

        //    // GDI+ still lies to us - the return format is BGR, NOT RGB.
        //    BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //    BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //    int scanline = bmData.Stride;

        //    System.IntPtr Scan0 = bmData.Scan0;
        //    System.IntPtr SrcScan0 = bmSrc.Scan0;

        //    unsafe
        //    {
        //        byte* p = (byte*)(void*)Scan0;
        //        byte* pSrc = (byte*)(void*)SrcScan0;

        //        int nOffset = bmData.Stride - b.Width * 3;
        //        int nWidth = b.Width;
        //        int nHeight = b.Height;

        //        int xOffset, yOffset;

        //        for (int y = 0; y < nHeight; ++y)
        //        {
        //            for (int x = 0; x < nWidth; ++x)
        //            {
        //                xOffset = offset[x, y].X;
        //                yOffset = offset[x, y].Y;

        //                if (yOffset >= 0 && yOffset < nHeight && xOffset >= 0 && xOffset < nWidth)
        //                {
        //                    p[0] = pSrc[(yOffset * scanline) + (xOffset * 3)];
        //                    p[1] = pSrc[(yOffset * scanline) + (xOffset * 3) + 1];
        //                    p[2] = pSrc[(yOffset * scanline) + (xOffset * 3) + 2];
        //                }

        //                p += 3;
        //            }
        //            p += nOffset;
        //        }
        //    }

        //    b.UnlockBits(bmData);
        //    bSrc.UnlockBits(bmSrc);

        //    return true;
        //}

        //private Image<Rgba32> ApplySwirl(Image<Rgba32> img, double factor)
        //{

        //    var width = img.Width;
        //    var height = img.Height;

        //    var cX = (double)width / 2f;
        //    var cY = (double)height / 2f;

        //    var newImg = new Image<Rgba32>(img.Width, img.Height);

        //    Parallel.For(0, height, y =>
        //    {

        //        var relY = cY - y;//relX and relY are uv respectively

        //        Parallel.For(0, width, x =>
        //        {

        //            var relX = x - cX;
        //            double ogAngle;

        //            if (relX != 0)
        //            {

        //                ogAngle = Math.Atan(Math.Abs(relY) / Math.Abs(relX));

        //                if (relX > 0 && relY < 0)
        //                    ogAngle = (2f * Math.PI) - ogAngle;
        //                else if (relX <= 0 && relY >= 0)
        //                    ogAngle = Math.PI - ogAngle;
        //                else if (relX <= 0 && relY < 0)
        //                    ogAngle += Math.PI;

        //            }
        //            else
        //            {

        //                if (relY >= 0)
        //                    ogAngle = 0.5 * Math.PI;
        //                else
        //                    ogAngle = 1.5 * Math.PI;

        //            }

        //            var radius = Math.Sqrt((relX * relX) + (relY * relY));
        //            var newAngle = ogAngle + (1 / ((factor * radius) + (4f / Math.PI)));
        //            var newX = (int)Math.Floor((radius * Math.Cos(newAngle)) + 0.5);
        //            var newY = (int)Math.Floor((radius * Math.Sin(newAngle)) + 0.5);

        //            newX = (int)(newX + cX);
        //            newY += (int)(newY + cY);
        //            newY = height - newY;

        //            newX = Math.Clamp(newX, 0, width - 1);
        //            newY = Math.Clamp(newY, 0, height - 1);

        //            newImg[x, y] = img[newX, newY];

        //        });

        //    });

        //    return newImg;

        //}

        [Command("buy"), Alias("b")]
        [Discord.Commands.Summary("Submits the decklist to massbuy on Tcgplayer!")]
        public async Task BuyCommand()
        {

            var attachments = Context.Message.Attachments;

            if (attachments.Count == 0)
                return;

            var file = attachments.FirstOrDefault(attachment => Path.GetExtension(attachment.Filename) == ".ydk");

            if (file is null)
            {

                await ReplyAsync("Invalid file provided! Must be a ydk or text file!");
                return;

            }

            var url = file.Url;
            string text;

            await using (var stream = await Web.GetStream(url))
            {

                var buffer = new byte[stream.Length];

                await stream.ReadAsync(buffer.AsMemory(0, (int) stream.Length));

                text = Encoding.UTF8.GetString(buffer);

            }

            var cards = text.Replace("#main", "")
                .Replace("#extra", "")
                .Replace("#created by ...", "")
                .Replace("!side", "")
                .Split('\n')
                .Select(passcode => passcode.Trim())
                .Where(passcode => !string.IsNullOrEmpty(passcode))
                .Select(async passcode => await YuGiOhDbService.GetNameWithPasscodeAsync(passcode))
                .Select(task => task.Result)
                .Where(name => !string.IsNullOrEmpty(name))
                .GroupBy(name => name)
                .Aggregate(new StringBuilder(), (builder, group) => builder.Append("||").Append(Uri.EscapeDataString($"{group.Count()} {group.First()}")))
                .ToString();

            url = $"http://store.tcgplayer.com/massentry?productline=YuGiOh&c={cards}";
            var response = await Web.Post("https://api-ssl.bitly.com/v4/shorten", $"{{\"long_url\": \"{url}\"}}", "Bearer", Config.Instance.Tokens.Bitly);
            url = JObject.Parse(await response.Content.ReadAsStringAsync())["link"].Value<string>();

            await ReplyAsync(url);

        }

        [Command("price"), Alias("prices", "p")]
        [Discord.Commands.Summary("Returns the prices based on your deck list from ygopro! No proper capitalization needed!")]
        public async Task DeckPriceCommand()
        {

            var attachments = Context.Message.Attachments;
            var file = attachments.FirstOrDefault(attachment => Path.GetExtension(attachment.Filename) == ".ydk");

            if (file is not null)
            {

                var url = file.Url;
                string text;

                using (var stream = await Web.GetStream(url))
                {

                    var buffer = new byte[stream.Length];

                    await stream.ReadAsync(buffer, 0, (int) stream.Length);

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
            var name = await YuGiOhDbService.GetNameWithPasscodeAsync(passcode);

            if (name is not null)
            {

                var response = await Web.GetResponseMessage(Constants.FandomWikiUrl + passcode);
                name = response.RequestMessage.RequestUri.Segments.Last().Replace('_', ' ');
                name = WebUtility.UrlDecode(name);

            }

            return (name, group.Count(), double.Epsilon);

        }

    }
}