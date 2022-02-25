using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Models.Autocompletes
{
    public class AntisupportAutocomplete : AutocompleteHandler
    {

        private readonly IYuGiOhDbService _yuGiOhDbService;

        public AntisupportAutocomplete(IYuGiOhDbService yuGiOhDbService)
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

            var input = autocompleteInteraction.Data.Current.Value as string;

            if (string.IsNullOrEmpty(input))
                return AutocompletionResult.FromSuccess();

            var antisupports = await _yuGiOhDbService.GetSupportsAutocompleteAsync(input);

            return AutocompletionResult.FromSuccess(antisupports.Select(support => new AutocompleteResult(support, support)));

        }

    }
}