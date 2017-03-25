using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhBot.Services;

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
        [Summary("HALP ME")]
        public async Task TheHelpCommand()
        {

            var organizedHelp = new StringBuilder("```http\n");
            //IEnumerable<ModuleInfo> commandModules = _commandService.Modules;

            organizedHelp.AppendLine($"{"Command".PadRight(30, ' ')} | Description");
            organizedHelp.AppendLine($"{"".PadRight(56, '-')}");
            organizedHelp.AppendLine($"{"card <card name>".PadRight(30, ' ')} | Searches card based on card name, capitalization not needed.");
            organizedHelp.AppendLine($"{"search <search>".PadRight(30, ' ')} | Searches for cards based on search terms given");
            organizedHelp.AppendLine($"{"lsearch <search>".PadRight(30, ' ')} | Coined lazy search, word order does not matter, capitalization not needed");
            organizedHelp.AppendLine($"{"archetype <search>".PadRight(30, ' ')} | Attemps an archetype search");
            organizedHelp.AppendLine($"{"invite".PadRight(30, ' ')} | Sends invite link to dm");
            organizedHelp.AppendLine($"{"info".PadRight(30, ' ')} | Returns information on bot");
            organizedHelp.AppendLine($"{"uptime".PadRight(30, ' ')} | Returns the uptime of the bot");
            organizedHelp.AppendLine($"{"ping".PadRight(30, ' ')} | Returns the latency between bot and guild");

            if(!(Context.Channel is IDMChannel) && (Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {

                organizedHelp.AppendLine($"{"prefix <prefix>".PadRight(30, ' ')} | Sets prefix for guild");

            }

            organizedHelp.AppendLine("```");

            if(CacheService.DMChannelCache.TryGetValue(Context.User.Id, out IDMChannel channel)) await channel.SendMessageAsync(organizedHelp.ToString());
            else
            {

                IDMChannel dmChannel = await Context.User.CreateDMChannelAsync();
                CacheService.DMChannelCache.TryAdd(Context.User.Id, dmChannel);
                await dmChannel.SendMessageAsync(organizedHelp.ToString());

            }

        }

    }
}
