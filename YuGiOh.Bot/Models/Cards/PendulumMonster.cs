using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public class PendulumMonster : RegularMonster, IHasScale
    {

        private static readonly Color PendulumColor = new Color(150, 208, 189);

        public int PendulumScale { get; set; }

        protected override Color GetColor()
            => PendulumColor;

        protected override List<string> GetDescription()
            => base.GetDescription().With($"**Scale:** {PendulumScale}");

        protected override EmbedBuilder AddLore(EmbedBuilder body)
        {

            var effects = Lore?.StartsWith("Pendulum Effect") == true ? Lore?.Split("Monster Effect") : null;

            return effects == null ?
                base.AddLore(body) :
                body
                .AddField("Pendulum Effect", effects[0].Substring(15).Trim())
                .AddField($"[ {Types.Join(" / ")} ]", effects[1].Trim());

        }

    }
}
