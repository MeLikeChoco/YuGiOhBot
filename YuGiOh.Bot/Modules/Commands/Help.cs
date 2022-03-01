using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Commands
{
    public class Help : MainBase
    {

        private readonly CommandService _commandService;
        private readonly Config _config;

        private IEnumerable<CommandInfo> _commands;

        private PaginatedAppearanceOptions AOptions => new()
        {

            JumpDisplayOptions = JumpDisplayOptions.Never,
            DisplayInformationIcon = false,
            FooterFormat = GuildConfig.AutoDelete ? "This message will be deleted in 3 minutes! | Page {0}/{1}" : "This message will not be deleted! | Page {0}/{1}",
            Timeout = GuildConfig.AutoDelete ? TimeSpan.FromMinutes(3) : TimeSpan.FromMilliseconds(-1)

        };

        public Help(
            ILoggerFactory loggerFactory,
            Cache cache,
            IYuGiOhDbService yuGiOhDbService,
            IGuildConfigDbService guildConfigDbService,
            Web web,
            Random rand,
            CommandService commandService,
            Config config
        ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web, rand)
        {
            _commandService = commandService;
            _config = config;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            base.BeforeExecute(command);
            _commands = _commandService.Commands.Where(CheckPrecond);
        }

        [Command("help")]
        [Summary("Get help on commands based on input!")]
        public Task SpecificHelpCommand([Remainder] string input)
        {

            var commands = _commands.Where(cmdInfo => cmdInfo.Name == input || cmdInfo.Aliases.Contains(input));

            if (!commands.Any())
                return NoResultError("commands", input);

            var str = new StringBuilder("```fix\n");
            var cmdStrings = commands
                .Select(cmdInfo => $"{cmdInfo.Name} {cmdInfo.Parameters.Select(param => $"<{param.Name}>").Join(' ')}\n{cmdInfo.Summary}")
                .Distinct();

            cmdStrings.ToList().ForEach(line => str.AppendLine($"{line}\n"));
            str.Append("```");

            return ReplyAsync(str.ToString());

        }

        [Command("help")]
        [Summary("Defacto help command!")]
        public Task HelpCommand()
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithName("Click for support guild/server!")
                .WithUrl(_config.GuildInvite);

            var paginatedMessage = new PaginatedMessage()
            {

                Author = author,
                Color = Rand.NextColor(),
                Options = AOptions

            };

            paginatedMessage.Pages = _commands
                .Select(cmdInfo => $"**Command:** {cmdInfo.Name} {cmdInfo.Parameters.Select(param => $"<{param.Name}>").Join(' ')}\n{cmdInfo.Summary}")
                .Distinct()
                .Chunk(5)
                .Select(group => group.Join("\n\n"));

            return PagedReplyAsync(paginatedMessage);

        }

        public bool CheckPrecond(CommandInfo command)
            => command.CheckPreconditionsAsync(Context).GetAwaiter().GetResult().IsSuccess;

    }
}