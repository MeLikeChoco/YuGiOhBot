using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Commands
{
    [RequireContext(ContextType.Guild)]
    public class Configuration : MainBase
    {

        public Configuration(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, rand) { }

        [Command("autodelete")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Toggles auto delete for embeds (true/false)")]
        public async Task SetAutoDeleteCommand(string input)
        {

            if (bool.TryParse(input, out var delete))
            {

                GuildConfig.AutoDelete = delete;

                await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
                await ReplyAsync($"Auto deletion of embeds has been set to: `{GuildConfig.AutoDelete}`");

            }
            else
                await TrueOrFalseMessage();

        }

        [Command("autodelete")]
        [RequireOwner]
        [Summary("Toggles auto delete for embeds (true/false)")]
        public Task SetAutoDeleteCommandOwner(string setting)
            => SetAutoDeleteCommand(setting);

        [Command("autodelete")]
        [Summary("Gets the auto delete setting")]
        public Task GetAutoDeleteCommand()
            => DisplaySetting("Auto Delete Embeds", GuildConfig.AutoDelete);

        [Command("guesstime")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets the amount of seconds for the guessing game!")]
        public async Task SetGuessTimeCommand(int seconds)
        {

            GuildConfig.GuessTime = seconds;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await ReplyAsync($"The time for guess game has been set to `{seconds}` seconds!");

        }

        [Command("guesstime")]
        [RequireOwner]
        [Summary("Sets the amount of seconds for the guessing game!")]
        public Task SetGuessTimeCommandOwner(int seconds)
            => SetGuessTimeCommand(seconds);

        [Command("guesstime")]
        [Summary("Get the amount of seconds for the guessing game!")]
        public Task GetGuessTimeCommand()
            => DisplaySetting("Guess Time", TimeSpan.FromSeconds(GuildConfig.GuessTime).ToPrettyString());

        [Command("hangmantime")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets the amount of seconds for hangman!")]
        public async Task SetHangmanTimeCommand(int seconds)
        {

            GuildConfig.HangmanTime = seconds;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await ReplyAsync($"The time for hangman has been set to `{seconds}` seconds!");

        }

        [Command("hangmantime")]
        [RequireOwner]
        [Summary("Sets the amount of seconds for hangman!")]
        public Task SetHangmanTimeCommandOwner(int seconds)
            => SetHangmanTimeCommand(seconds);

        [Command("hangmantime")]
        [Summary("Get the amount of time for hangman game!")]
        public Task GetHangmanTimeCommand()
            => DisplaySetting("Hangman Time", TimeSpan.FromSeconds(GuildConfig.HangmanTime).ToPrettyString());

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets the prefix for the bot in this guild!")]
        public async Task SetPrefixCommand([Remainder] string prefix)
        {

            GuildConfig.Prefix = prefix;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await ReplyAsync($"Prefix has been set to `{prefix}`");

        }

        [Command("prefix")]
        [RequireOwner]
        [Summary("Sets the prefix for the bot in this guild!")]
        public Task SetPrefixCommandOwner([Remainder] string prefix)
            => SetPrefixCommand(prefix);

        [Command("prefix")]
        [Summary("See the prefix the guild is using! Kinda useless tbh...")]
        public Task PrefixCommand()
            => DisplaySetting("Prefix", GuildConfig.Prefix);

        [Command("minimal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Set how much card info should be shown! (true/false)")]
        public async Task SetMinimalCommand([Remainder] string input)
        {

            if (bool.TryParse(input, out var minimal))
            {

                GuildConfig.Minimal = minimal;

                await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
                await ReplyAsync($"Minimal has been set to `{minimal}`");

            }
            else
                await TrueOrFalseMessage();

        }

        [Command("minimal")]
        [RequireOwner]
        [Summary("Set how much card info should be shown! (true/false)")]
        public Task MinimalCommandOwner(string input)
            => SetMinimalCommand(input);

        [Command("minimal")]
        [Summary("Check how much card info is shown!")]
        public Task MinimalCommand()
            => DisplaySetting("Minimal", GuildConfig.Minimal);

        [Command("inline")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Enable (true) or disable (false) inline search!")]
        public async Task InlineCommand(bool input)
        {

            if (GuildConfig.Inline != input)
            {

                GuildConfig.Inline = input;

                await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
                await ReplyAsync($"Inline has been set to `{input}`");

            }

        }

        [Command("inline")]
        [RequireOwner]
        [Summary("Enable (true) or disable (false) inline search!")]
        public Task InlineCommandOwner(bool input)
            => InlineCommand(input);

        [Command("inline")]
        [Summary("Check if inline search is disabled!")]
        public Task InlineCommandOwner()
            => ReplyAsync($"Inline search enabled: **{GuildConfig.Inline}**");

        [Command("hangmanallowwords")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Enable (true) or disable (false) multi-character input for hangman")]
        public async Task HangmanWordsCommand(bool input)
        {

            GuildConfig.HangmanAllowWords = input;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await ReplyAsync($"The ability for multi-character input has been {(GuildConfig.HangmanAllowWords ? "enabled" : "disabled")}");

        }

        [Command("hangmanallowwords")]
        [RequireOwner]
        [Summary("Enable (true) or disable (false) multi-character input for hangman")]
        public Task HangmanWordsCommandOwner(bool input)
            => HangmanWordsCommand(input);

        [Command("hangmanallowwords")]
        [Summary("Check if multi-character input is allowed for hangman")]
        public Task HangmanWordsCommand()
            => DisplaySetting("Hangman Words", GuildConfig.HangmanAllowWords);

        private Task DisplaySetting(string setting, object value)
            => ReplyAsync($"**{setting}:** {value}");

        private Task TrueOrFalseMessage()
            => ReplyAsync("This command only accepts `true` or `false`!");

    }
}