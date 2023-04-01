using System.Collections.Generic;
using Discord;
using YuGiOh.Bot.Extensions;

//This class exists because Xyz Pendulum monsters are Xyz monsters first and Pendulum monsters second
//Xyz Pendulum monsters are basically Xyz monsters with pendulum scales, they don't have levels that pendulum monsters have

namespace YuGiOh.Bot.Models.Cards
{
    public class XyzPendulum : Xyz, IHasScale
    {

        public int PendulumScale { get; set; }
        public string PendulumLore { get; set; }

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