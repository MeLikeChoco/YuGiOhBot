using Discord;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Models.Deserializers;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Extensions
{
    public static class EmbedBuilderExtensions
    {

        static Random _rand = new Random();

        public static EmbedBuilder WithRandomColor(this EmbedBuilder builder)
            => builder.WithColor(_rand.NextColor());

        public static async Task<EmbedBuilder> WithCardPrices(this EmbedBuilder embed, bool minimal, Web web, TimeSpan? searchTime = null)
        {

            TimeSpan time;

            if (searchTime is not null)
            {

                time = searchTime.Value;
                var rounded = Math.Round(time.TotalSeconds, 5, MidpointRounding.ToEven).ToString("0.00000");

                embed.Footer.WithText($"Search time: {rounded} seconds");

            }

            if (minimal)
            {

                embed.ThumbnailUrl = embed.ImageUrl;
                embed.ImageUrl = null;

            }
            else
            {

                string realName;

                if (embed.Description.Contains("Real Name"))
                {

                    var indexOne = embed.Description.IndexOf(':');
                    var indexTwo = embed.Description.IndexOf("**Format");
                    realName = embed.Description.Substring(indexOne, indexTwo).Trim();

                }
                else
                    realName = embed.Author.Name;

                var response = await web.GetPrices((string)embed.Author.Name, realName);

                if (response?.Data is not null)
                {

                    IEnumerable<Datum> prices;

                    if (response.Data.Count >= 4)
                    {

                        embed.AddField("Prices", "**Showing the first 3 prices due to too many to show**");

                        prices = response.Data.Take(3);

                    }
                    else
                        prices = response.Data;

                    foreach (Datum info in prices)
                    {

                        if (string.IsNullOrEmpty(info.PriceData.Message))
                            EmbedBuilderExtensions.AddPriceShort(embed, (Datum)info);
                        else
                            embed.AddField(info.Name, info.PriceData.Message);

                    }

                }
                else
                    embed.AddField("Prices", "**No prices to show for this card at this time!**");

            }

            return embed;

        }

        public static EmbedBuilder AddPriceShort(this EmbedBuilder body, Datum info, bool isInline = false)
        {

            return body.AddField(info.Name,
                                $"Rarity: {info.Rarity}\n" +
                                $"Lowest Price: {info.PriceData.Data.Prices.Low.ToString("0.00")}", isInline);

        }

        public static EmbedBuilder AddPrice(this EmbedBuilder body, Datum info, bool isInline)
        {

            var prices = info.PriceData.Data.Prices;

            return body.AddField(info.Name,
                            $"**Print Tag:** {info.PrintTag}\n" +
                            $"**Rarity:** {info.Rarity}\n" +
                            $"**Low:** ${prices.Low.ToString("0.00")}\n" +
                            $"**High:** ${prices.High.ToString("0.00")}\n" +
                            $"**Average:** ${prices.Average.ToString("0.00")}", isInline);

        }

    }
}
