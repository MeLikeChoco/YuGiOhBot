using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Objects.Attributes;

namespace YuGiOhV2.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireChannel(541938684438511616)]
    public class Dev : MainBase
    {

        [Command("booster")]
        [Summary("Gets information on a booster pack!")]
        public Task BoosterCommand([Remainder]string input)
        {

            if (Cache.BoosterPacks.TryGetValue(input, out var boosterPack))
            {

                var builder = new EmbedBuilder()
                    .WithAuthor(boosterPack.Name, url: boosterPack.Url)
                    .WithDescription($"**Amount:** {boosterPack.Cards.Length} cards")
                    .WithColor(Rand.NextColor())
                    .AddField("Release dates", boosterPack.ReleaseDates.Aggregate("", (current, kv) => $"{current}\n**{kv.Key}:** {kv.Value.ToString("MM/dd/yyyy")}"));

                foreach(var kv in boosterPack.RarityToCards)
                    builder.AddField(kv.Key, kv.Value.Aggregate("```", (current, next) => $"{current}\n{next}") + "```");

                return SendEmbed(builder);

            }
            else
                return NoResultError("booster packs", input);

        }

    }
}
