using System.Threading.Tasks;
using Discord.WebSocket;
using YuGiOh.Bot.Models;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Handlers
{
    public class GuildHandler
    {

        private readonly IGuildConfigDbService _guildConfigService;

        public GuildHandler(IGuildConfigDbService guildConfigService)
        {
            _guildConfigService = guildConfigService;
        }

        public Task HandleAddedToGuildAsync(SocketGuild guild)
            => _guildConfigService.InsertGuildConfigAsync(new GuildConfig { Id = guild.Id });

    }
}