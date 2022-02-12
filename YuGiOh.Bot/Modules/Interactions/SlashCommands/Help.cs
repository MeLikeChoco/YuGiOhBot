using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MoreLinq;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Autocompleters;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Help : MainInteractionBase<SocketSlashCommand>
    {

        public CommandService CmdService { get; set; }
        public InteractionService InteractionService { get; set; }
        public CommandHelpService CmdHelpService { get; set; }
        public Config Config { get; set; }
        public Random Random { get; set; }

        private PaginatedAppearanceOptions AOptions => new()
        {

            JumpDisplayOptions = JumpDisplayOptions.Never,
            DisplayInformationIcon = false,
            FooterFormat = _guildConfig.AutoDelete ? "This message will be deleted in 3 minutes! | Page {0}/{1}" : "This message's input will expire in 3 minutes! | Page {0}/{1}",
            Timeout = TimeSpan.FromMinutes(3),
            ShouldDeleteOnTimeout = _guildConfig.AutoDelete

        };

        [SlashCommand("help", "The defacto help command")]
        public Task GenericHelpCommand([Autocomplete(typeof(CommandAutocomplete))] string input = null)
        {

            if (string.IsNullOrEmpty(input))
                return HelpCommand();
            else
                return HelpCommand(input);

        }

        private Task HelpCommand(string input)
        {

            //IEnumerable<object> cmds = CmdService.Commands.Where(cmdInfo => cmdInfo.Name == input || cmdInfo.Aliases.Contains(input));
            //cmds = cmds.Concat(InteractionService.SlashCommands.Where(cmdInfo => cmdInfo.Name == input));

            var cmds = CmdHelpService.GetCmds(Context.User, input);

            if (!cmds.Any())
                return NoResultError("commands", input);

            var cmdStrings = cmds
                .Select(cmd =>
                {

                    if (cmd is CommandInfo cmdInfo)
                        return $"{_guildConfig.Prefix}{cmdInfo.Name} {cmdInfo.Parameters.Select(param => $"<{param.Name}>").Join(' ')}\n{cmdInfo.Summary}";
                    else if (cmd is SlashCommandInfo slashCmdInfo)
                        return $"/{slashCmdInfo.Name} {slashCmdInfo.Parameters.Select(param => param.IsRequired ? $"<{param.Name}>" : $"<optional: {param.Name}>").Join(' ')}\n{slashCmdInfo.Description}";
                    else
                        return "";

                })
                .Distinct()
                .OrderBy(str => str);

            var strBuilder = cmdStrings.Aggregate(new StringBuilder("```fix").AppendLine(), (accumulator, cmdStr) => accumulator.AppendLine(cmdStr).AppendLine());
            strBuilder.Append("```");

            return RespondAsync(strBuilder.ToString());

        }

        private Task HelpCommand()
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithName("Click for support guild/server")
                .WithUrl(Config.GuildInvite);

            var paginatedMessage = new PaginatedMessage()
            {

                Author = author,
                Color = Random.NextColor(),
                Options = AOptions

            };

            paginatedMessage.Pages = CmdHelpService.GetCmds(Context.User)
                .Select(cmd =>
                {

                    string str = null;

                    if (cmd is CommandInfo cmdInfo)
                    {

                        str = $"**Command:** {_guildConfig.Prefix}{cmdInfo.Name} {cmdInfo.Parameters.Select(param => $"<{param.Name}>").Join(' ')}";

                        if (!string.IsNullOrEmpty(cmdInfo.Summary))
                            str += $"\n{cmdInfo.Summary}";

                    }
                    else if (cmd is SlashCommandInfo slashCmdInfo)
                    {

                        str = $"**Command:** /{slashCmdInfo.Name} ";
                        str += slashCmdInfo.Parameters
                            .Select(param => param.IsRequired ? $" <{param.Name}> " : $" <optional: {param.Name}> ")
                            .Join(' ');

                        if (!string.IsNullOrEmpty(slashCmdInfo.Description))
                            str += $"\n{slashCmdInfo.Description}";

                    }

                    return str;

                })
                .Distinct()
                .OrderBy(str => str)
                .Batch(5)
                .Select(group => group.Join("\n\n"));

            return PagedComponentReplyAsync(paginatedMessage);

        }

    }
}
