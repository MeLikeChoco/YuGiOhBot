﻿using System.Collections.Generic;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public class Xyz : Monster, IHasRank, IHasMaterials, IHasAtk, IHasDef
    {

        private const string UnknownValue = "???";
        private static readonly Color XyzColor = new(0, 0, 1);

        public int Rank { get; set; }
        public string Materials { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }

        protected override Color GetColor()
            => XyzColor;

        protected override List<string> GetDescription()
            => base.GetDescription().With($"**Rank:** {Rank}");

        protected override EmbedBuilder AddAdditionalFields(EmbedBuilder body)
            => base.AddAdditionalFields(
                body
                    .AddField("ATK", string.IsNullOrWhiteSpace(Atk) ? UnknownValue : Atk, true)
                    .AddField("DEF", string.IsNullOrWhiteSpace(Def) ? UnknownValue : Def, true)
                    .AddField("\u200b", "\u200b", true)
            );

    }
}