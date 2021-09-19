using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using YuGiOh.Bot.Extensions;

//This class exists because Xyz Pendulum monsters are Xyz monsters first and Pendulum monsters second
//Xyz Pendulum monsters are basically Xyz monsters with pendulum scales, they don't have levels that pendulum monsters have

namespace YuGiOh.Bot.Models.Cards
{
    public class XyzPendulum : Xyz, IHasScale
    {

        public int PendulumScale { get; set; }

        protected override List<string> GetDescription()
            => base.GetDescription().With($"**Scale:** {PendulumScale}");

        protected override EmbedBuilder AddLore(EmbedBuilder body)
        {

            var effects = Lore?.StartsWith("Pendulum Effect") == true ? Lore?.Split("Monster Effect") : null;

            return effects is null ?
                base.AddLore(body) :
                body
                .AddField("Pendulum Effect", effects[0].Substring(15).Trim())
                .AddField($"[ {Types.Join(" / ")} ]", effects[1].Trim());

        }

    }
}
