using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Services
{
    public class CommandHandler
    {

        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly IGuildConfigDbService _guildConfigDbService;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandler(
            DiscordShardedClient client,
            CommandService commandService,
            IGuildConfigDbService guildConfigDbService,
            IServiceProvider serviceProvider)
        {

            _client = client;
            _commandService = commandService;
            _guildConfigDbService = guildConfigDbService;
            _serviceProvider = serviceProvider;

        }

        public async Task HandleCommand(SocketMessage message)
        {

            if (message is not SocketUserMessage
                || message.Author.IsBot
                || string.IsNullOrEmpty(message.Content))
                return;

            var prefix = "y!";

            if (message.Channel is SocketTextChannel textChannel)
            {

                var id = textChannel.Guild.Id;
                var guildConfig = await _guildConfigDbService.GetGuildConfigAsync(id);
                prefix = guildConfig.Prefix;

            }

            var possibleCmd = message as SocketUserMessage;
            var argPos = 0;

            if ((possibleCmd.HasStringPrefix(prefix, ref argPos) || possibleCmd.HasMentionPrefix(_client.CurrentUser, ref argPos)) &&
                possibleCmd.Content.Trim() != prefix)
            {

                var context = new ShardedCommandContext(_client, possibleCmd);

                if (message.Channel is SocketDMChannel)
                    AltConsole.Write("Info", "Command", $"{possibleCmd.Author.Username} in DM's");
                else if (message.Channel is SocketTextChannel txtChannel)
                    AltConsole.Write("Info", "Command", $"{possibleCmd.Author.Username} from {txtChannel.Guild.Name}");

                AltConsole.Write("Info", "Command", $"{possibleCmd.Content}");

                var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);

                if (!result.IsSuccess)
                {

                    if (result.ErrorReason.Contains("unknown command", StringComparison.OrdinalIgnoreCase))
                        return;
                    else if (result.ErrorReason.Contains("you are currently in timeout", StringComparison.OrdinalIgnoreCase))
                        await context.Channel.SendMessageAsync("Please wait 5 seconds between each type of paginator command!");

                    //await context.Channel.SendMessageAsync("https://goo.gl/JieFJM");

                    AltConsole.Write("Error", "Error", result.ErrorReason);
                    //debug purposes
                    //await context.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");

                }

            }

        }

    }
}
