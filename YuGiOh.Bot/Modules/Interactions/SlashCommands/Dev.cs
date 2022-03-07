using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Attributes;
using YuGiOh.Bot.Modules.Interactions.Autocompletes;
using YuGiOh.Bot.Services;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.SlashCommands;

[RequireChannel(541938684438511616)]
public class Dev : MainInteractionBase<SocketSlashCommand>
{

    private readonly IYuGiOhRepository _yugiohRepo;

    public Dev(
        ILoggerFactory loggerFactory,
        Cache cache,
        IYuGiOhDbService yuGiOhDbService,
        IGuildConfigDbService guildConfigDbService,
        Web web,
        IYuGiOhRepository yuGiOhRepository
    ) : base(loggerFactory, cache, yuGiOhDbService, guildConfigDbService, web)
    {
        _yugiohRepo = yuGiOhRepository;
    }

    [SlashCommand("json", "Print json of card in console")]
    public async Task GetJsonOfCard([Autocomplete(typeof(CardAutocomplete))] string input)
    {

        var entity = await _yugiohRepo.GetCardAsync(input);
        var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        Logger.Info(json);

        await RespondAsync("Json printed to console");

    }

}