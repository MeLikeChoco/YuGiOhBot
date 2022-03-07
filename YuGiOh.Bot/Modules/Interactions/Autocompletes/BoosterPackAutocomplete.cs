using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Modules.Interactions.Autocompletes;

public class BoosterPackAutocomplete : AutocompleteHandler
{

    private readonly IYuGiOhDbService _yuGiOhDbService;

    public BoosterPackAutocomplete(IYuGiOhDbService yuGiOhDbService)
    {
        _yuGiOhDbService = yuGiOhDbService;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services
    )
    {

        var input = (string) autocompleteInteraction.Data.Current.Value;

        if (string.IsNullOrEmpty(input))
            return AutocompletionResult.FromSuccess();

        var results = await _yuGiOhDbService.GetBoosterPacksAutocompleteAsync(input);

        return AutocompletionResult.FromSuccess(results.Select(result => new AutocompleteResult(result, result)));

    }

}