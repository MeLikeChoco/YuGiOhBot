using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuGiOhBot.Services;

namespace YuGiOhBot.Commands
{
    public class InfoCommands : ModuleBase
    {

        private GuildServices _guildService;

        public InfoCommands(GuildServices guildServiceParams)
        {

            _guildService = guildServiceParams;

        }

        [Command("prefix")]
        [Summary("Change command prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PrefixCommand([Remainder]string prefix)
        {

            await _guildService.SetPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"Prefix has been set to: {prefix}");

        }

        [Command("invite")]
        [Summary("Get invite link")]
        public async Task InviteCommand()
        {

            ulong id = Context.Client.GetApplicationInfoAsync().Result.Id;

            await ReplyAsync($"{Context.User.Mention} https://discordapp.com/oauth2/authorize?client_id={id}&scope=bot&permissions=0");

        }

        [Command("info")]
        [Summary("Returns info on the system the bot is on and the bot itself")]
        public async Task InfoCommand()
        {

            EmbedBuilder eBuilder;

            using (Context.Channel.EnterTypingState())
            {

                IApplication AppInfo = await Context.Client.GetApplicationInfoAsync();
                string ownerAvatarId = AppInfo.Owner.GetAvatarUrl();
                DateTimeOffset creationTime = AppInfo.CreatedAt;
                string botDescription = AppInfo.Description;
                string owner = AppInfo.Owner.Username;
                string botName = AppInfo.Name;
                string botAvatarId = Context.Client.CurrentUser.GetAvatarUrl();
                string framework = RuntimeInformation.FrameworkDescription;
                var architecture = RuntimeInformation.OSArchitecture.ToString();
                var ramUsage = ((Process.GetCurrentProcess().WorkingSet64 / 1024f) / 1024f).ToString();
                TimeSpan Uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
                string discordnetVersion = DiscordConfig.Version;

                var authorBuilder = new EmbedAuthorBuilder()
                {

                    Name = owner,
                    Url = "https://github.com/melikechoco",
                    IconUrl = ownerAvatarId

                };

                var footerBuilder = new EmbedFooterBuilder()
                {

                    IconUrl = "http://2static2.fjcdn.com/thumbnails/comments/Ahegao+_0eaf3dbc104f428d0d2c548c7a62c78b.jpg",
                    Text = $"A shitty bot made by {owner} aka MeLikeChoco"

                };

                eBuilder = new EmbedBuilder()
                {
                    Color = new Color(255, 128, 128),
                    Author = authorBuilder,
                    Description = $"**{botName}**\nThis was {owner}'s first major PUBLIC coding project.",
                    ThumbnailUrl = botAvatarId,
                    Footer = footerBuilder,
                    Timestamp = DateTimeOffset.Now
                };

                eBuilder.AddField(x =>
                {

                    x.Name = "When was this bot created?";
                    x.Value = creationTime.ToString();
                    x.IsInline = false;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "What is this thing?";
                    x.Value = botDescription;
                    x.IsInline = false;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "Can I see the source code?";
                    x.Value = "I suck at coding ;_;" +
                    "\npls no flame" +
                    "\nhttps://github.com/MeLikeChoco/YuGiOhBot";
                    x.IsInline = false;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "What language and API?";
                    x.Value = $"Bot is coded in C# with .NET Core using Discord.NET library {discordnetVersion}";
                    x.IsInline = false;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "Would you like a donation?";
                    x.Value = "I think about it, but it would mean nothing because if I wanted to stop working on it for no reason, " +
                    "your donations would go to waste.";
                    x.IsInline = false;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "Framework";
                    x.Value = framework;
                    x.IsInline = false;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "System Info";
                    x.Value = $"{OperatingSystem()} {architecture}";
                    x.IsInline = true;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "RAM Usage";
                    x.Value = $"{ramUsage} Megabytes";
                    x.IsInline = true;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "Uptime";
                    x.Value = $"{Uptime.Days} days {Uptime.Hours} hours {Uptime.Minutes} minutes {Uptime.Seconds} seconds";
                    x.IsInline = false;

                });

                eBuilder.AddField(x =>
                {

                    x.Name = "Support";
                    x.Value = "[Support Server](https://image.slidesharecdn.com/bitcoin-130620013853-phpapp01/95/bitcoin-digital-gold-52-638.jpg?cb=1371692421)" +
                    "\nOr you could use my [Github](https://github.com/MeLikeChoco/YuGiOhBot/issues)";
                    x.IsInline = false;

                });

            }

            await ReplyAsync(string.Empty, embed: eBuilder);

        }

        [Command("uptime")]
        [Summary("Returns how long the bot has remained up without stopping")]
        public async Task UptimeCommand()
        {

            TimeSpan Uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            await ReplyAsync($"The bot has been up for {Uptime.Days} days, {Uptime.Hours} hours, {Uptime.Minutes} minutes, and {Uptime.Seconds} seconds.");

        }

        [Command("ping")]
        [Summary("Returns ping between bot and guild")]
        public async Task PingCommang()
        {

            int latency = (Context.Client as DiscordSocketClient).Latency;
            await ReplyAsync($"The latency between the bot and guild is: **{latency} ms**");

        }

        public string OperatingSystem()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {

                return "Windows";

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {

                return "Linux";

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {

                return "OSX";

            }
            else
            {

                return "Some alien OS";

            }

        }
    }
}
