using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using MoreLinq;
using YuGiOhV2.Extensions;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Help : CustomBase
    {

        private CommandService _commands;
        private Random _rand;
        private Database _db;

        private static readonly PaginatedAppearanceOptions AOptions = new PaginatedAppearanceOptions
        {

            JumpDisplayOptions = JumpDisplayOptions.Never,
            DisplayInformationIcon = false,
            FooterFormat = "This message will be deleted in 10 minutes! | Page {0}/{1}",
            Timeout = TimeSpan.FromMinutes(10)

        };

        public Help(CommandService commands, Random rand, Database db)
        {

            _commands = commands;
            _rand = rand;
            _db = db;

        }

        [Command("help")]
        [Summary("Get help on commands based on input!")]
        public Task SpecificHelpCommand([Remainder]string input)
        {

            var commands = _commands.Commands
                .Where(command => command.Name == input)
                .Where(command => CheckPrecond(command));

            if(commands.Count() == 0)
                return NoResultError("commands", input);

            var str = $"```fix\n";

            foreach(var command in commands)
            {

                str += $"{command.Name} ";

                foreach(var parameter in command.Parameters)
                {

                    str += $"<{parameter.Name}> ";

                }

                str += $"\n{command.Summary}\n\n";

            }

            str += "```";

            return ReplyAsync(str.Trim());
            
        }

        [Command("help")]
        [Summary("Defacto help command!")]
        [Ratelimit(1, 0.084, Measure.Minutes)]
        public async Task HelpCommand()
        {

            var messages = new List<string>();
            var author = new EmbedAuthorBuilder()
                .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithName("HALP, IVE FALLEN AND CANT GET UP");

            var paginatedMessage = new PaginatedMessage
            {

                Author = author,
                Color = _rand.GetColor(),
                Options = AOptions

            };

            IEnumerable<CommandInfo> commands;

            if (Context.User.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id &&
                Context.Guild.Id == 171432768767524864)
                commands = _commands.Commands.Where(command => CheckPrecond(command));
            else
                commands = _commands.Commands
                    .Where(command => !command.Preconditions.Any(precondition => precondition is RequireOwnerAttribute))
                    .Where(command => command.CheckPreconditionsAsync(Context).Result.IsSuccess);

            var groups = commands.Batch(5);

            foreach (var group in groups)
            {

                var str = "";

                foreach (var command in group)
                {

                    str += $"**Command:** {command.Name} ";

                    foreach (var parameter in command.Parameters)
                    {

                        str += $"<{parameter.Name}> ";

                    }

                    str += $"\n{command.Summary}\n\n";

                }

                messages.Add(str.Trim());

            }

            paginatedMessage.Pages = messages;

            await PagedReplyAsync(paginatedMessage);

        }

        public bool CheckPrecond(CommandInfo command)
            => command.CheckPreconditionsAsync(Context).Result.IsSuccess;

    }
}
