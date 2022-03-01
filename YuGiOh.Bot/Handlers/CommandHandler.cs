using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Handlers
{
    public class CommandHandler
    {

        private readonly ILogger _logger;
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly IGuildConfigDbService _guildConfigDbService;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandler(
            ILoggerFactory loggerFactory,
            DiscordShardedClient client,
            CommandService commandService,
            IGuildConfigDbService guildConfigDbService,
            IServiceProvider serviceProvider
        )
        {

            _logger = loggerFactory.CreateLogger("Command");
            _client = client;
            _commandService = commandService;
            _guildConfigDbService = guildConfigDbService;
            _serviceProvider = serviceProvider;

        }

        public async Task HandleCommandAsync(SocketMessage message)
        {

            if (message is not SocketUserMessage possibleCmd
                || possibleCmd.Author.IsBot
                || string.IsNullOrEmpty(possibleCmd.Content))
                return;

            var prefix = "y!";

            if (possibleCmd.Channel is SocketTextChannel textChannel)
            {

                var id = textChannel.Guild.Id;
                var guildConfig = await _guildConfigDbService.GetGuildConfigAsync(id);
                prefix = guildConfig.Prefix;

            }

            var argPos = 0;

            if ((possibleCmd.HasStringPrefix(prefix, ref argPos) || possibleCmd.HasMentionPrefix(_client.CurrentUser, ref argPos)) &&
                possibleCmd.Content.Trim() != prefix)
            {

                var context = new ShardedCommandContext(_client, possibleCmd);
                var user = possibleCmd.Author;

                switch (possibleCmd.Channel)
                {

                    case SocketDMChannel:
                        _logger.Info($"{user.Username}#{user.Discriminator} in DM's");
                        // AltConsole.Write("Info", "Command", $"{user.Username}#{user.Discriminator} in DM's");
                        break;
                    case SocketTextChannel txtChannel:
                        _logger.Info($"{user.Username}#{user.Discriminator} from {txtChannel.Guild.Name}/{txtChannel.Name}");
                        // AltConsole.Write("Info", "Command", $"{user.Username}#{user.Discriminator} from {txtChannel.Guild.Name}/{txtChannel.Name}");
                        break;

                }

                _logger.Info(possibleCmd.Content);
                // AltConsole.Write("Info", "Command", $"{possibleCmd.Content}");

                var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);

                if (!result.IsSuccess)
                {

                    if (result.ErrorReason.Contains("unknown command", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (result.ErrorReason.Contains("you are currently in timeout", StringComparison.OrdinalIgnoreCase))
                        await context.Channel.SendMessageAsync("Please wait 5 seconds between each type of paginator command!");

                    //await context.Channel.SendMessageAsync("https://goo.gl/JieFJM");

                    _logger.Error(result.ErrorReason);
                    // AltConsole.Write("Error", "Error", result.ErrorReason);
                    //debug purposes
                    //await context.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");

                }

            }

        }

    }
}