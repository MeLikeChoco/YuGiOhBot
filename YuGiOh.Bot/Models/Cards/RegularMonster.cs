using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public class RegularMonster : Monster, IHasLevel, IHasAtk, IHasDef
    {

        private const string UnknownValue = "???";

        public int Level { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }

        protected override List<string> GetDescription()
            => Level >= 0 ? base.GetDescription().With($"**Level:** {Level}") : base.GetDescription();

        protected override EmbedBuilder AddLore(EmbedBuilder body)
            => Lore == null ? base.AddLore(body) : body.AddField($"[ {Types.Join(" / ")} ]", Lore);

        protected override EmbedBuilder AddAdditionalFields(EmbedBuilder body)
            => base.AddAdditionalFields(
                body
                .AddField("ATK", string.IsNullOrEmpty(Atk) ? UnknownValue : Atk, true)
                .AddField("DEF", string.IsNullOrEmpty(Def) ? UnknownValue : Def, true)
                );

    }
}
