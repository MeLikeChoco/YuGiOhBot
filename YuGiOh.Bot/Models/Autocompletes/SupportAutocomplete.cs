using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Models.Autocompletes
{
    public class SupportAutocomplete : AutocompleteHandler
    {

        private readonly IYuGiOhDbService _yuGiOhDbService;

        public SupportAutocomplete(IYuGiOhDbService yuGiOhDbService)
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

            var supports = await _yuGiOhDbService.GetSupportsAutocompleteAsync(input);

            return AutocompletionResult.FromSuccess(supports.Select(support => new AutocompleteResult(support, support)));

        }

    }
}