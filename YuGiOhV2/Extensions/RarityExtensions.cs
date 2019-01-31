using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects.BoosterPacks;

namespace YuGiOhV2.Extensions
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
