using System.Linq;
using YuGiOh.Bot.Models.BoosterPacks;

namespace YuGiOh.Bot.Extensions
{
    public static class RarityExtensions
    {

        public static string ToDisplayString(this Rarity rarity)
        {

            var attribute = rarity.GetType().GetMember(rarity.ToString()).First().GetCustomAttributes(typeof(RarityAttribute), false).FirstOrDefault() as RarityAttribute;

            return attribute?.Name ?? rarity.ToString();

        }

    }
}
