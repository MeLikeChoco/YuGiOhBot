using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Configuration : MainBase
    {

        [Command("autodelete")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Toggles auto delete for embeds (true/false)")]
        public async Task SetAutoDeleteCommand(string input)
        {

            if (bool.TryParse(input, out var delete))
            {

                try
                {

                    _guildConfig.AutoDelete = delete;

                    await GuildConfigDbService.UpdateGuildConfigAsync(_guildConfig);
                    await ReplyAsync($"Auto deletion of embeds has been set to: `{_guildConfig.AutoDelete}`");

                }
                catch (Exception e)
                {

                    AltConsole.Write("Error", "Configuration", "BAH GAWD SOMETHING WRONG WITH AUTODELETE", e);
                    await ReplyAsync("There was an error in setting autodelete. Please report error with `y!feedback <message>`");

                }

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
            => DisplaySetting("Auto Delete Embeds", _guildConfig.AutoDelete);

        [Command("guesstime")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets the amount of seconds for the guessing game!")]
        public async Task SetGuessTimeCommand(int seconds)
        {

            _guildConfig.GuessTime = seconds;

            await GuildConfigDbService.UpdateGuildConfigAsync(_guildConfig);
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
            => ReplyAsync($"**Guess time:** {_guildConfig.GuessTime} seconds");

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets the prefix for the bot in this guild!")]
        public async Task SetPrefixCommand([Remainder] string prefix)
        {

            try
            {

                _guildConfig.Prefix = prefix;

                await GuildConfigDbService.UpdateGuildConfigAsync(_guildConfig);
                await ReplyAsync($"Prefix has been set to `{prefix}`");

            }
            catch (Exception e)
            {

                AltConsole.Write("Error", "Configuration", "BAH GAWD SOMETHING WRONG WITH SETTING PREFIX", e);
                await ReplyAsync("There was an error in setting the prefix. Please report error with `y!feedback <message>`");

            }

        }

        [Command("prefix")]
        [RequireOwner]
        [Summary("Sets the prefix for the bot in this guild!")]
        public Task SetPrefixCommandOwner([Remainder] string prefix)
            => SetPrefixCommand(prefix);

        [Command("prefix")]
        [Summary("See the prefix the guild is using! Kinda useless tbh...")]
        public Task PrefixCommand()
            => DisplaySetting("Prefix", _guildConfig.Prefix);

        [Command("minimal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Set how much card info should be shown! (true/false)")]
        public async Task SetMinimalCommand([Remainder] string input)
        {

            if (bool.TryParse(input, out var minimal))
            {

                try
                {

                    _guildConfig.Minimal = minimal;

                    await GuildConfigDbService.UpdateGuildConfigAsync(_guildConfig);
                    await ReplyAsync($"Minimal has been set to `{minimal}`");

                }
                catch (Exception e)
                {

                    AltConsole.Write("Error", "Configuration", "BAH GAWD SOMETHING WRONG WITH SETTING MINIMAL", e);
                    await ReplyAsync("There was an error in setting the minimal setting. Please report error with `y!feedback <message>`");

                }

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
            => DisplaySetting("Minimal", _guildConfig.Minimal);

        [Command("inline")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Enable (true) or disable (false) inline search!")]
        public async Task InlineCommand(bool input)
        {

            if (_guildConfig.Inline != input)
            {

                try
                {

                    _guildConfig.Inline = input;

                    await GuildConfigDbService.UpdateGuildConfigAsync(_guildConfig);
                    await ReplyAsync($"Inline has been set to `{input}`");

                }
                catch (Exception e)
                {

                    AltConsole.Write("Error", "Configuration", "BAH GAWD SOMETHING WRONG WITH SETTING INLINE", e);
                    await ReplyAsync("There was an error in setting the minimal setting. Please report error with `y!feedback <message>`");

                }

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
            => ReplyAsync($"Inline search enabled: **{_guildConfig.Inline}**");

        private Task DisplaySetting(string setting, object value)
            => ReplyAsync($"**{setting}:** {value}");

        private Task TrueOrFalseMessage()
            => ReplyAsync("This command only accepts `true` or `false`!");

    }
}