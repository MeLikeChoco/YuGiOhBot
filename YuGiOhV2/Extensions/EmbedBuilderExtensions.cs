using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Extensions
{
    public static class EmbedBuilderExtensions
    {

        static Random _rand = new Random();

        public static EmbedBuilder WithRandomColor(this EmbedBuilder builder)
            => builder.WithColor(_rand.NextColor());

    }
}
