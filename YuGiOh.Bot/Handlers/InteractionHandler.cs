using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Handlers
{
    public class InteractionHandler
    {

        private readonly CommandService _commandService;

        public InteractionHandler(CommandService commandService)
        {
            _commandService = commandService;
        }

        public async Task HandleInteraction(SocketInteraction interaction)
        {

            switch (interaction)
            {

                case SocketSlashCommand slashCommand:

                    break;
                default:
                    break;

            }

        }

    }
}
