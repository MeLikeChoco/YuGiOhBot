using Discord;
using System;

namespace YuGiOhV2.Extensions
{
    public static class RandomExtensions
    {

        public static Color GetColor(this Random rand)
            => new Color(rand.Next(256), rand.Next(256), rand.Next(256));

    }
}
