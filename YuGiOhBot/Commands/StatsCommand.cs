using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhBot.Attributes;

namespace YuGiOhBot.Commands
{
    public class StatsCommand : ModuleBase
    {

        [Command("stats")]
        [Summary("Prints the stats of the bot")]
        [Cooldown(10)]
        public async Task StatCommand()
        {

            IReadOnlyCollection<SocketGuild> guilds = (Context.Client as DiscordSocketClient).Guilds;
            int guildCount = guilds.Count;
            int textChannels = guilds.Sum(guild => guild.TextChannels.Count);
            int voiceChannels = guilds.Sum(guild => guild.VoiceChannels.Count);
            string largestGuild = guilds.MaxBy(guild => guild.Users.Count).Name;
            string guildMostChannels = guilds.MaxBy(guild => guild.Channels.Count).Name;
            string guildMostRoles = guilds.MaxBy(guild => guild.Roles.Count).Name;
            int largestGuildCount = guilds.Max(guild => guild.Users.Count);
            int averageUsers = (int)guilds.Average(guild => guild.Users.Count);
            int roles = guilds.Sum(guild => guild.Roles.Count);
            int dmchannels = (Context.Client as DiscordSocketClient).DMChannels.Count;
            DateTime oldestGuild = guilds.Min(guild => guild.CreatedAt).Date;
            
            HashSet<SocketUser> nonDuplicateUsers = new HashSet<SocketUser>();
            guilds.ToList().ForEach(guild => guild.Users.ToList().ForEach(user => nonDuplicateUsers.Add(user)));
            var r = new Random();

            IApplication appInfo = await Context.Client.GetApplicationInfoAsync();

            var authorBuilder = new EmbedAuthorBuilder
            {

                Name = appInfo.Name,
                IconUrl = appInfo.IconUrl,
                Url = "https://github.com/MeLikeChoco/YuGiOhBot",

            };

            var footerBuilder = new EmbedFooterBuilder
            {

                Text = $"Stat info called by {Context.User.Username}",

            };

            var eBuilder = new EmbedBuilder
            {

                Author = authorBuilder,
                Color = new Color((byte)r.Next(1, 256), (byte)r.Next(1, 256), (byte)r.Next(1, 256)),
                Title = "YuGiOhBot v1.1.0",
                Description = "*Stats for the bot!*",
                Footer = footerBuilder,
                Timestamp = DateTime.Now,

            };

            eBuilder.AddInlineField("Guilds", guildCount);
            eBuilder.AddInlineField("Text Channels", textChannels);
            eBuilder.AddInlineField("Voice Channels", voiceChannels);
            eBuilder.AddInlineField("Guild with most channels", guildMostChannels);
            eBuilder.AddInlineField("Roles", roles);
            eBuilder.AddInlineField("Guild with most roles", guildMostRoles);
            eBuilder.AddInlineField("Users", nonDuplicateUsers.Count);
            eBuilder.AddInlineField("Largest Guild", largestGuild);
            eBuilder.AddInlineField("Users in largest guild", largestGuildCount);
            eBuilder.AddInlineField("DM Channels", dmchannels);

            await ReplyAsync("", embed: eBuilder);

        }

    }
}
