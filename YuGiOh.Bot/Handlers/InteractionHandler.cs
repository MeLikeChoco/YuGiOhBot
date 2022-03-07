using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Handlers
{
    public class InteractionHandler
    {

        private readonly ILogger _logger;
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly InteractionServiceConfig _interactionConfig;
        private readonly IServiceProvider _serviceProvider;

        public InteractionHandler(
            ILoggerFactory loggerFactory,
            DiscordShardedClient client,
            InteractionService interactionService,
            InteractionServiceConfig interactionConfig,
            IServiceProvider serviceProvider
        )
        {

            _logger = loggerFactory.CreateLogger("Interaction Command");
            _client = client;
            _interactionService = interactionService;
            _interactionConfig = interactionConfig;
            _serviceProvider = serviceProvider;

        }

        public async Task HandleInteractionAsync(SocketInteraction interaction)
        {

            switch (interaction)
            {

                case SocketSlashCommand slashCommand:
                    await HandleSlashCommand(slashCommand);
                    break;
                case SocketAutocompleteInteraction autocomplete:
                    await HandleAutocomplete(autocomplete);
                    break;

            }

        }

        private async Task HandleSlashCommand(SocketSlashCommand interaction)
        {

            if (interaction.User.IsBot)
                return;

            var user = interaction.User;

            switch (interaction.Channel)
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

            _logger.Info(interaction.GetCmdString());
            // AltConsole.Write("Info", "Interaction Command", interaction.GetCmdString());

            var context = new ShardedInteractionContext<SocketSlashCommand>(_client, interaction);
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
                _logger.Error(result.ErrorReason);
            // AltConsole.Write("Error", "Error", result.ErrorReason);

        }

        private async Task HandleAutocomplete(SocketAutocompleteInteraction interaction)
        {

            if (interaction.User.IsBot)
                return;

            var user = interaction.User;

            switch (interaction.Channel)
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

            var context = new ShardedInteractionContext<SocketAutocompleteInteraction>(_client, interaction);
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
                _logger.Error(result.ErrorReason);
            // AltConsole.Write("Error", "Error", result.ErrorReason);

        }

    }
}