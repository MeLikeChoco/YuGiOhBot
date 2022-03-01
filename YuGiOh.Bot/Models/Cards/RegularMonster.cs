using System.Collections.Generic;
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
            => Lore is null ? base.AddLore(body) : body.AddField($"[ {Types.Join(" / ")} ]", Lore);

        protected override EmbedBuilder AddAdditionalFields(EmbedBuilder body)
            => base.AddAdditionalFields(
                body
                    .AddField("ATK", string.IsNullOrWhiteSpace(Atk) ? UnknownValue : Atk, true)
                    .AddField("DEF", string.IsNullOrWhiteSpace(Def) ? UnknownValue : Def, true)
                    .AddField("\u200b", "\u200b", true)
            );

    }
}