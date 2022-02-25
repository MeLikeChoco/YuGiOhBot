using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Models.Autocompletes;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands
{
    public class Help : MainInteractionBase<SocketSlashCommand>
    {

        private readonly CommandHelpService _cmdHelpService;
        private readonly Config _config;
        private readonly Random _random;

        private PaginatedAppearanceOptions AOptions => new()
        {

            JumpDisplayOptions = JumpDisplayOptions.Never,
            DisplayInformationIcon = false,
            FooterFormat = GuildConfig.AutoDelete ? "This message will be deleted in 3 minutes! | Page {0}/{1}" : "This message's input will expire in 3 minutes! | Page {0}/{1}",
            Timeout = TimeSpan.FromMinutes(3),
            ShouldDeleteOnTimeout = GuildConfig.AutoDelete

        };

        public Help(
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            CommandService cmdService,
            InteractionService interactionService,
            CommandHelpService cmdHelpService,
            Config config,
            Random random
        ) : base(cache, yuGiOhDbService, guildConfigDbService, web)
        {

            _cmdHelpService = cmdHelpService;
            _config = config;
            _random = random;

        }

        [SlashCommand("help", "The defacto help command")]
        public Task GenericHelpCommand([Autocomplete(typeof(CommandAutocomplete))] string input = null)
            => string.IsNullOrEmpty(input) ? HelpCommand() : HelpCommand(input);

        private Task HelpCommand(string input)
        {

            //IEnumerable<object> cmds = CmdService.Commands.Where(cmdInfo => cmdInfo.Name == input || cmdInfo.Aliases.Contains(input));
            //cmds = cmds.Concat(InteractionService.SlashCommands.Where(cmdInfo => cmdInfo.Name == input));

            var cmds = _cmdHelpService.GetCmds(Context.User, input);

            if (!cmds.Any())
                return NoResultError("commands", input);

            var cmdStrings = cmds
                .Select(cmd
                    => cmd switch
                    {
                        CommandInfo cmdInfo => $"{GuildConfig.Prefix}{cmdInfo.Name} {cmdInfo.Parameters.Select(param => $"<{param.Name}>").Join(' ')}\n{cmdInfo.Summary}",
                        SlashCommandInfo slashCmdInfo => $"/{slashCmdInfo.Name} {slashCmdInfo.Parameters.Select(param => param.IsRequired ? $"<{param.Name}>" : $"<optional: {param.Name}>").Join(' ')}\n{slashCmdInfo.Description}",
                        _ => ""
                    }
                )
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
                .WithUrl(_config.GuildInvite);

            var paginatedMessage = new PaginatedMessage
            {

                Author = author,
                Color = _random.NextColor(),
                Options = AOptions,
                Pages = _cmdHelpService.GetCmds(Context.User)
                    .Select(cmd =>
                    {

                        string str = null;

                        switch (cmd)
                        {

                            case CommandInfo cmdInfo:
                            {

                                str = $"**Command:** {GuildConfig.Prefix}{cmdInfo.Name} {cmdInfo.Parameters.Select(param => $"<{param.Name}>").Join(' ')}";

                                if (!string.IsNullOrEmpty(cmdInfo.Summary))
                                    str += $"\n{cmdInfo.Summary}";

                                break;

                            }

                            case SlashCommandInfo slashCmdInfo:
                            {

                                str = $"**Command:** /{slashCmdInfo.Name} ";
                                str += slashCmdInfo.Parameters
                                    .Select(param => param.IsRequired ? $" <{param.Name}> " : $" <optional: {param.Name}> ")
                                    .Join(' ');

                                if (!string.IsNullOrEmpty(slashCmdInfo.Description))
                                    str += $"\n{slashCmdInfo.Description}";

                                break;

                            }

                        }

                        return str;

                    })
                    .Distinct()
                    .OrderBy(str => str)
                    .Chunk(5)
                    .Select(group => @group.Join("\n\n"))
            };

            return PagedComponentReplyAsync(paginatedMessage);

        }

    }
}