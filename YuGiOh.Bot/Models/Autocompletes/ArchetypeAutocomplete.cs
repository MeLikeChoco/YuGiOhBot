using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Models.Autocompletes
{
    public class ArchetypeAutocomplete : AutocompleteHandler
    {

        private readonly IYuGiOhDbService _yugiohDbService;

        public ArchetypeAutocomplete(IYuGiOhDbService yugiohDbService)
        {
            _yugiohDbService = yugiohDbService;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {

            var input = autocompleteInteraction.Data.Current.Value as string;

            if (string.IsNullOrEmpty(input))
                return AutocompletionResult.FromSuccess();

            var archetypes = await _yugiohDbService.GetArchetypesAutocompleteAsync(input);

            return AutocompletionResult.FromSuccess(archetypes.Select(archetype => new AutocompleteResult(archetype, archetype)));

        }

    }
}