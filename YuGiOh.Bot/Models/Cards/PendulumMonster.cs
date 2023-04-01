using System.Collections.Generic;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public class PendulumMonster : RegularMonster, IHasScale
    {

        private static readonly Color PendulumColor = new(150, 208, 189);

        public int PendulumScale { get; set; }
        public string PendulumLore { get; set; }

        protected override Color GetColor()
            => PendulumColor;

        protected override List<string> GetDescription()
            => base.GetDescription().With($"**Scale:** {PendulumScale}");

        protected override EmbedBuilder AddLore(EmbedBuilder body)
        {

            if (!string.IsNullOrEmpty(PendulumLore))
                body.AddField("Pendulum Effect", PendulumLore);

            return base.AddLore(body);

        }

    }
}