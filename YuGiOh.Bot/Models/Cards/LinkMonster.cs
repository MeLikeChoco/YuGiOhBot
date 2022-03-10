using System.Collections.Generic;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public class LinkMonster : Monster, IHasLink, IHasAtk
    {

        private static readonly Color LinkColor = new Color(0, 0, 139);

        public int Link { get; set; }
        public string[] LinkArrows { get; set; }
        public string Atk { get; set; }

        protected override Color GetColor()
            => LinkColor;

        protected override List<string> GetDescription()
            => base.GetDescription()
                .With($"**Links:** {Link}")
                .With($"**Link Markers:** {LinkArrows.Join(", ")}");

        protected override EmbedBuilder AddAdditionalFields(EmbedBuilder body)
            => base.AddAdditionalFields(
                body
                    .AddField("ATK", Atk, true)
                    .AddField("\u200b", "\u200b", true)
                    .AddField("\u200b", "\u200b", true)
            );

    }
}