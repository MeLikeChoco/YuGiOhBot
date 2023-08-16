using System;
using Discord;

namespace YuGiOh.Bot.Extensions
{
    public static class RandomExtensions
    {

        public static Color NextColor(this Random rand)
            => new Color(rand.Next(256), rand.Next(256), rand.Next(256));

    }
}