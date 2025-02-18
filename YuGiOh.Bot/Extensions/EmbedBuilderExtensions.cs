using System;
using System.Threading.Tasks;
using Discord;
using YuGiOh.Bot.Models.Deserializers;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Extensions
{
    public static class EmbedBuilderExtensions
    {

        private static readonly Random Rand = new();

        public static EmbedBuilder WithRandomColor(this EmbedBuilder builder)
            => builder.WithColor(Rand.NextColor());

        public static Task<EmbedBuilder> WithCardPrices(
            this EmbedBuilder embed,
            bool minimal,
            IYuGiOhPricesService yugiohPricesService,
            TimeSpan? searchTime = null
        )
        {

            if (searchTime is not null)
            {

                var time = searchTime.Value.TotalSeconds;
                var rounded = Math.Round(time > 1 ? time : time * 1000, 5, MidpointRounding.ToEven).ToString("0.00000");

                embed.Footer.WithText($"Search time: {rounded} {(time > 1 ? "seconds" : "milliseconds")}");

            }

            if (minimal)
            {
            
                embed.ThumbnailUrl = embed.ImageUrl;
                embed.ImageUrl = null;
            
            }
            else
            {

                embed.AddField("Prices", "Unfortunately, prices are unavailable for now.");
            
                // string realName;
                //
                // if (embed.Description.Contains("Real Name"))
                // {
                //
                //     var indexOne = embed.Description.IndexOf(':');
                //     var indexTwo = embed.Description.IndexOf("**Format", StringComparison.OrdinalIgnoreCase);
                //     realName = embed.Description.Substring(indexOne, indexTwo).Trim();
                //
                // }
                // else
                //     realName = embed.Author.Name;

                // var response = await web.GetPrices((string) embed.Author.Name, realName);
                //
                // if (response?.Data is not null)
                // {
                //
                //     IEnumerable<Datum> prices;
                //
                //     if (response.Data.Count >= 4)
                //     {
                //
                //         embed.AddField("Prices", "**Showing the first 3 prices due to too many to show**");
                //
                //         prices = response.Data.Take(3);
                //
                //     }
                //     else
                //         prices = response.Data;
                //
                //     embed = prices.Aggregate(embed, (current, info)
                //         => string.IsNullOrEmpty(info.PriceData.Message) ? current.AddPriceShort(info) : current.AddField(info.Name, info.PriceData.Message)
                //     );
                //
                // }
                // else
                //     embed.AddField("Prices", "**No prices to show for this card at this time!**");

            }

            return Task.FromResult(embed);

        }

        private static EmbedBuilder AddPriceShort(this EmbedBuilder body, Datum info, bool isInline = false)
        {

            return body.AddField(info.Name,
                $"Rarity: {info.Rarity}\n" +
                $"Lowest Price: {info.PriceData.Data.Prices.Low:0.00}", isInline);

        }

        public static EmbedBuilder AddPrice(this EmbedBuilder body, Datum info, bool isInline)
        {

            var prices = info.PriceData.Data.Prices;

            //separate lines of concatenation for organization and readability
            return body.AddField(info.Name,
                $"**Print Tag:** {info.PrintTag}\n" +
                $"**Rarity:** {info.Rarity}\n" +
                $"**Low:** ${prices.Low:0.00}\n" +
                $"**High:** ${prices.High:0.00}\n" +
                $"**Average:** ${prices.Average:0.00}", isInline);

        }

    }
}