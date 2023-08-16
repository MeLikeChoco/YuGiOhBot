using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands;

[RequireUserPermission(GuildPermission.Administrator), RequireOwner]
public class Configuration : MainInteractionBase<SocketSlashCommand>
{

    public Configuration(
        ILoggerFactory loggerFactory,
        Cache cache,
        IYuGiOhDbService yuGiOhDbService,
        IGuildConfigDbService guildConfigDbService,
        Web web,
        InteractiveService interactiveService
    ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, interactiveService) { }

    [SlashCommand("autodelete", "Toggles auto delete for embeds (true/false)")]
    public async Task AutoDeleteCommand([Summary(description: "true/false")] bool? input = null)
    {

        if (input is not null)
        {

            GuildConfig.AutoDelete = input.Value;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await RespondAsync($"Auto deletion of embeds has been set to: `{GuildConfig.AutoDelete}`");

        }
        else
            await DisplaySetting("Auto Delete Embeds", GuildConfig.AutoDelete);

    }

    [SlashCommand("guesstime", "Sets the amount of seconds for the guessing game!")]
    public async Task GuessTimeCommand([Summary(description: "The amount of time in seconds")] int? input = null)
    {

        if (input is not null)
        {

            GuildConfig.GuessTime = input.Value;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await RespondAsync($"The time for guess game has been set to `{input}` seconds!");

        }
        else
            await DisplaySetting("Guess Time", TimeSpan.FromSeconds(GuildConfig.GuessTime).ToPrettyString());

    }

    [SlashCommand("hangmantime", "Sets the amount of seconds for the hangman game!")]
    public async Task HangmanTimeCommand([Summary(description: "The amount of time in seconds")] int? input = null)
    {

        if (input is not null)
        {

            GuildConfig.HangmanTime = input.Value;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await RespondAsync($"The time for the hangman game has been set to `{input}` seconds!");

        }
        else
            await DisplaySetting("Hangman Time", TimeSpan.FromSeconds(GuildConfig.HangmanTime).ToPrettyString());

    }

    [SlashCommand("hangmanallowwords", "Enables or disables multi-character input for hangman")]
    public async Task HangmanWordsCommand([Summary(description: "true/false")] bool? input = null)
    {

        if (input is not null)
        {

            GuildConfig.HangmanAllowWords = input.Value;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await RespondAsync($"The ability for multi-character input has been {(GuildConfig.HangmanAllowWords ? "enabled" : "disabled")}");

        }
        else
            await DisplaySetting("Hangman multi-character input enabled", GuildConfig.HangmanAllowWords);

    }

    [SlashCommand("prefix", "Sets the command prefix for the bot in this guild!")]
    public async Task PrefixCommand([Summary(description: "The prefix the command will use")] string input = null)
    {

        if (!string.IsNullOrWhiteSpace(input))
        {

            GuildConfig.Prefix = input;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await RespondAsync($"Prefix has been set to `{input}`");

        }
        else
        {
            await DisplaySetting("Prefix", GuildConfig.Prefix);
            await ReplyAsync("This setting is now obsolete. Avoid using text-based commands.");
        }

    }

    [SlashCommand("minimal", "Sets how much card info should be shown!")]
    public async Task MinimalCommand([Summary(description: "true/false")] bool? input = null)
    {

        if (input is not null)
        {

            GuildConfig.Minimal = input.Value;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await RespondAsync($"Minimal has been set to `{input}`");

        }
        else
            await DisplaySetting("Minimal card info", GuildConfig.Minimal);

    }

    [SlashCommand("inline", "Enable or disable inline search")]
    public async Task InlineCommand([Summary(description: "true/false")] bool? input = null)
    {

        if (input is not null)
        {

            GuildConfig.Inline = input.Value;

            await GuildConfigDbService.UpdateGuildConfigAsync(GuildConfig);
            await RespondAsync($"Inline search has been set to `{input}`");

        }
        else
            await DisplaySetting("Inline search enabled", GuildConfig.Minimal);

    }

    private Task DisplaySetting(string setting, object value)
        => RespondAsync($"**{setting}:** {value}");

    private Task TrueOrFalseMessage()
        => RespondAsync("This command only accepts `true` or `false`!");

}