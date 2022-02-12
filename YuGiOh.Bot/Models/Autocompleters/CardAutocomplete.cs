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
    public class CardAutocomplete : AutocompleteHandler
    {

        private readonly IYuGiOhDbService _yuGiOhDbService;

        public CardAutocomplete(IYuGiOhDbService yugiohDbService)
        {
            _yuGiOhDbService = yugiohDbService;
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

            var cards = await _yuGiOhDbService.GetCardsAutocomplete(input);

            return AutocompletionResult.FromSuccess(cards.Select(card => new AutocompleteResult(card.Name, card.Name)));

        }

    }
}
