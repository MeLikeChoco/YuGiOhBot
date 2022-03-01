using System;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules
{
    public abstract class MainBase : CustomBase
    {

        protected ILogger Logger { get; }
        protected Cache Cache { get; }
        protected IYuGiOhDbService YuGiOhDbService { get; }
        protected IGuildConfigDbService GuildConfigDbService { get; }
        protected Web Web { get; }
        protected Random Rand { get; }

        protected GuildConfig GuildConfig { get; set; }

        protected PaginatedAppearanceOptions PagedOptions => new()
        {

            DisplayInformationIcon = false,
            JumpDisplayOptions = JumpDisplayOptions.Never,
            FooterFormat = GuildConfig.AutoDelete ? "Enter a number to see that result! Expires in 60 seconds! | Page {0}/{1}" : "This embed will not be deleted! | Page {0}/{1}",
            Timeout = GuildConfig.AutoDelete ? TimeSpan.FromSeconds(60) : TimeSpan.FromMilliseconds(-1)

        };

        protected MainBase(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand
        )
        {

            Logger = loggerFactory.CreateLogger(GetType().Name);
            Cache = cache;
            YuGiOhDbService = yuGiOhDbService;
            GuildConfigDbService = guildConfigDbService;
            Web = web;
            Rand = rand;

        }

        protected override void BeforeExecute(CommandInfo command)
        {

            GuildConfig = Context.Channel is not SocketDMChannel ? GuildConfigDbService.GetGuildConfigAsync(Context.Guild.Id).GetAwaiter().GetResult() : GuildConfigDbService.GetGuildConfigAsync(0).GetAwaiter().GetResult();

        }

    }
}