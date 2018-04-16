using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Extensions;
using YuGiOhV2.Objects;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Help : CustomBase
    {
        
        public CommandService Commands { get; set; }
        public Random Rand { get; set; }
        public Database Database { get; set; }
        public Config Config { get; set; }

        private Setting _setting;

        private PaginatedAppearanceOptions _aOptions => new PaginatedAppearanceOptions()
        {

            JumpDisplayOptions = JumpDisplayOptions.Never,
            DisplayInformationIcon = false,
            FooterFormat = _setting.AutoDelete ? "This message will be deleted in 3 minutes! | Page {0}/{1}" : "This message will not be deleted! | Page {0}/{1}",
            Timeout = _setting.AutoDelete ? TimeSpan.FromMinutes(3) : TimeSpan.FromSeconds(-1)

        };

        protected override void BeforeExecute(CommandInfo command)
            => _setting = Database.Settings[Context.Guild.Id];

        [Command("help")]
        [Summary("Get help on commands based on input!")]
        public Task SpecificHelpCommand([Remainder]string input)
        {

            var commands = Commands.Commands
                .Where(command => command.Name == input && CheckPrecond(command));

            if(!commands.Any())
                return NoResultError("commands", input);

            var str = new StringBuilder($"```fix\n");

            foreach(var command in commands)
            {

                str.Append($"{command.Name} ");

                foreach(var parameter in command.Parameters)
                {

                    str.Append($"<{parameter.Name}> ");

                }

                str.AppendLine($"\n{command.Summary}\n");

            }

            str.Append("```");

            return ReplyAsync(str.ToString());
            
        }

        [Command("help")]
        [Summary("Defacto help command!")]
        public async Task HelpCommand()
        {

            var messages = new List<string>();
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

            IEnumerable<CommandInfo> commands;

            if (Context.User.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id &&
                Context.Guild.Id == 171432768767524864)
                commands = Commands.Commands.Where(command => CheckPrecond(command));
            else
                commands = Commands.Commands
                    .Where(command => !command.Preconditions.Any(precondition => precondition is RequireOwnerAttribute) && 
                                       command.CheckPreconditionsAsync(Context).GetAwaiter().GetResult().IsSuccess);

            var groups = commands.Batch(5);

            foreach (var group in groups)
            {

                var str = new StringBuilder();

                foreach (var command in group)
                {

                    str.Append($"**Command:** {command.Name} ");

                    foreach (var parameter in command.Parameters)
                    {

                        str.Append($"<{parameter.Name}> ");

                    }

                    str.AppendLine($"\n{command.Summary}\n");

                }

                messages.Add(str.ToString());

            }

            paginatedMessage.Pages = messages;

            await PagedReplyAsync(paginatedMessage);

        }

        public bool CheckPrecond(CommandInfo command)
            => command.CheckPreconditionsAsync(Context).GetAwaiter().GetResult().IsSuccess;

    }
}
