using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Models.Autocompleters
{
    public class ArchetypeAutocomplete : AutocompleteHandler
    {

        private IYuGiOhDbService _yugiohDbService;

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
            var archetypes = await _yugiohDbService.GetArchetypesAutocomplete(input);

            return AutocompletionResult.FromSuccess(archetypes.Select(archetype => new AutocompleteResult(archetype, archetype)));

        }

    }
}
