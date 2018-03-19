using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Extensions
{
    public static class RandomExtensions
    {

        public static Color NextColor(this Random rand)
            => new Color(rand.Next(256), rand.Next(256), rand.Next(256));

    }
}
