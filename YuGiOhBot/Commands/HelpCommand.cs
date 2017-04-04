using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhBot.Services;
using YuGiOhBot.Attributes;

namespace YuGiOhBot.Commands
{

    public class HelpCommand : ModuleBase
    {

        private CommandService _commandService;

        public HelpCommand(CommandService commandServiceParams)
        {

            _commandService = commandServiceParams;

        }

        [Command("help"), Alias("h")]
        [Cooldown(10)]
        [Summary("HALP ME")]
        public async Task TheHelpCommand()
        {

            var organizedHelp = new StringBuilder("```http\n");
            //IEnumerable<ModuleInfo> commandModules = _commandService.Modules;

            organizedHelp.AppendLine($"{"Commands".PadRight(30, ' ')} | Description");
            organizedHelp.AppendLine($"{"".PadRight(56, '-')}");
            organizedHelp.AppendLine($"Note >> Capitalization does not matter");
            organizedHelp.AppendLine($"{"".PadRight(56, '-')}");
            organizedHelp.AppendLine($"{"card <card name>".PadRight(30, ' ')} | Searches card based on card name");
            organizedHelp.AppendLine($"{"lcard <card name>".PadRight(30, ' ')} | Coined lazy card, word order does not matter, gets the first available result");
            organizedHelp.AppendLine($"{"rcard".PadRight(30, ' ')} | Returns a random card. Great for making random decks");
            organizedHelp.AppendLine($"{"search <search>".PadRight(30, ' ')} | Searches for cards based on search terms given");
            organizedHelp.AppendLine($"{"lsearch <search>".PadRight(30, ' ')} | Coined lazy search, word order does not matter");
            organizedHelp.AppendLine($"{"archetype <search>".PadRight(30, ' ')} | Attemps an archetype search");
            organizedHelp.AppendLine($"{"banlist <1/2/3>".PadRight(30, ' ')} | Returns the current banlist WARNING >> big message");
            organizedHelp.AppendLine($"{"invite".PadRight(30, ' ')} | Sends invite link to dm");
            organizedHelp.AppendLine($"{"info".PadRight(30, ' ')} | Returns information on bot");
            organizedHelp.AppendLine($"{"uptime".PadRight(30, ' ')} | Returns the uptime of the bot");
            organizedHelp.AppendLine($"{"ping".PadRight(30, ' ')} | Returns the latency between bot and guild");
            organizedHelp.AppendLine($"{"help".PadRight(30, ' ')} | The defacto help command");
            organizedHelp.AppendLine($"{"feedback <feedback>".PadRight(30, ' ')} | Sends feedback to {Context.Client.GetApplicationInfoAsync().Result.Owner.Username}");
            organizedHelp.AppendLine($"{"minimal <true/false>".PadRight(30, ' ')} | Sets minimal card settings for guild");

            if (!(Context.Channel is IDMChannel) && (Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {

                organizedHelp.AppendLine($"{"prefix <prefix>".PadRight(30, ' ')} | Sets prefix for guild");

            }

            organizedHelp.AppendLine($"I also have inline declaration of cards. For example, \"I like [[blue-eyes]]\" will give you a Blue-Eyes card! You can use multiple" +
                $" inline declarations such as \"[[red-eyes]] will beat [[blue-eyes]]\"!");

            organizedHelp.AppendLine($"{"".PadRight(56, '-')}");
            organizedHelp.AppendLine("```");

            IDMChannel channel = await Context.User.CreateDMChannelAsync();
            await channel.SendMessageAsync(organizedHelp.ToString());

        }

    }
}
