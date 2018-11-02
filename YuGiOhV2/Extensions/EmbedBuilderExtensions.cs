using Discord;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects.Deserializers;
using YuGiOhV2.Services;

namespace YuGiOhV2.Extensions
{
    public static class EmbedBuilderExtensions
    {

        static Random _rand = new Random();

        public static EmbedBuilder WithRandomColor(this EmbedBuilder builder)
            => builder.WithColor(_rand.NextColor());

        public static async Task<EmbedBuilder> WithPrices(this EmbedBuilder embed, bool minimal, Web web, TimeSpan? searchTime = null)
        {

            var clone = embed.DeepClone();
            TimeSpan time;

            if (searchTime != null)
            {

                time = searchTime.Value;
                var rounded = Math.Round(time.TotalSeconds, 5, MidpointRounding.ToEven).ToString("0.00000");

                clone.Footer.WithText($"Search time: {rounded} seconds");

            }

            if (minimal)
            {

                clone.ThumbnailUrl = clone.ImageUrl;
                clone.ImageUrl = null;

            }
            else
            {

                string realName;

                if (clone.Description.Contains("Real Name"))
                {

                    var indexOne = clone.Description.IndexOf(':');
                    var indexTwo = clone.Description.IndexOf("**Format");
                    realName = clone.Description.Substring(indexOne, indexTwo).Trim();

                }
                else
                    realName = clone.Author.Name;

                var response = await web.GetPrices(clone.Author.Name, realName);

                if (response.Data != null)
                {

                    IEnumerable<Datum> prices;

                    if (response.Data.Count >= 4)
                    {

                        clone.AddField("Prices", "**Showing the first 3 prices due to too many to show**");

                        prices = response.Data.Take(3);

                    }
                    else
                        prices = response.Data;

                    foreach (Datum info in prices)
                    {

                        if (string.IsNullOrEmpty(info.PriceData.Message))
                        {

                            clone.AddField(info.Name,
                                $"Rarity: {info.Rarity}\n" +
                                $"Average Price: {info.PriceData.Data.Prices.Average.ToString("0.00")}");

                        }
                        else
                            clone.AddField(info.Name, info.PriceData.Message);

                    }

                }
                else
                    clone.AddField("Prices", "**No prices to show for this card!**");

            }

            return clone;

        }

    }
}
