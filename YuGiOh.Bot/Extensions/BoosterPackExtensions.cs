using System;
using System.Globalization;
using System.Linq;
using YuGiOh.Bot.Models.BoosterPacks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Bot.Extensions
{
    public static class BoosterPackExtensions
    {

        public static BoosterPack ToModel(this BoosterPackEntity entity)
        {

            var boosterpack = new BoosterPack
            {
                
                Name = entity.Name,
                Url = entity.Url,
                OcgEcists = entity.OcgExists,
                TcgExists = entity.TcgExists,
                Dates = entity.Dates.Select(dateEntity => dateEntity.ToModel()).ToList(),
                Cards = entity.Cards.Select(cardEntity => cardEntity.ToModel()).ToList()
                
            };

            return boosterpack;

        }

        private static BoosterPackDate ToModel(this BoosterPackDateEntity entity)
        {

            var date = new BoosterPackDate { Name = entity.Name };

            try
            {
                date.Date = DateTime.ParseExact(entity.Date, "MMMM d, yyyy", new CultureInfo("en-US"));
            }
            catch (FormatException)
            {
                try
                {
                    date.Date = DateTime.ParseExact(entity.Date, "MMMM, yyyy", new CultureInfo("en-US"));
                }
                catch (FormatException)
                {
                    try
                    {
                        date.Date = DateTime.ParseExact(entity.Date, "MMMM yyyy", new CultureInfo("en-US"));
                    }
                    catch (FormatException)
                    {
                        date.Date = DateTime.ParseExact(entity.Date, "yyyy", new CultureInfo("en-US"));
                    }
                }
            }

            return date;

        }

        private static BoosterPackCard ToModel(this BoosterPackCardEntity entity)
        {

            return new BoosterPackCard
            {
                Name = entity.Name,
                Rarities = entity.Rarities
            };

        }

    }
}