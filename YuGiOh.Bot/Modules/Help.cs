using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Modules
{
    public class Help : CustomBase
    {
        
        public CommandService Commands { get; set; }
        public Random Rand { get; set; }
        public Services.Database Database { get; set; }
        public Config Config { get; set; }

        private Setting _setting;
        private IEnumerable<CommandInfo> _commands;

        private PaginatedAppearanceOptions _aOptions => new PaginatedAppearanceOptions()
        {

            JumpDisplayOptions = JumpDisplayOptions.Never,
            DisplayInformationIcon = false,
            FooterFormat = _setting.AutoDelete ? "This message will be deleted in 3 minutes! | Page {0}/{1}" : "This message will not be deleted! | Page {0}/{1}",
            Timeout = _setting.AutoDelete ? TimeSpan.FromMinutes(3) : TimeSpan.FromMilliseconds(-1)

        };

        protected override void BeforeExecute(CommandInfo command)
        {

            _setting = Database.Settings[Context.Guild.Id];
            _commands = Commands.Commands.Where(CheckPrecond);

        }

        [Command("help")]
        [Summary("Get help on commands based on input!")]
        public Task SpecificHelpCommand([Remainder]string input)
        {

            var commands = _commands.Where(cmdInfo => cmdInfo.Name == input || cmdInfo.Aliases.Contains(input));

            if(!commands.Any())
                return NoResultError("commands", input);

            var str = new StringBuilder($"```fix\n");
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
                .WithName($"Click for support guild/server!")
                .WithUrl(Config.GuildInvite);

            var paginatedMessage = new PaginatedMessage()
            {

                Author = author,
                Color = Rand.NextColor(),
                Options = _aOptions

            };

            var messages = _commands.Select(cmdInfo => $"**Command:** {cmdInfo.Name} {cmdInfo.Parameters.Select(param => $"<{param.Name}>").Join(' ')}\n{cmdInfo.Summary}")
                .Distinct()
                .Batch(5)
                .Select(group => group.Join("\n\n"));
                       
            paginatedMessage.Pages = messages;

            return PagedReplyAsync(paginatedMessage);

        }

        public bool CheckPrecond(CommandInfo command)
            => command.CheckPreconditionsAsync(Context).GetAwaiter().GetResult().IsSuccess;

    }
}
