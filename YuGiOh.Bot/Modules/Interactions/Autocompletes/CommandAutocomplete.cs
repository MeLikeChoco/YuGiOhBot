using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using MoreLinq;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Modules.Interactions.Autocompletes
{
    public class CommandAutocomplete : AutocompleteHandler
    {

        private readonly CommandHelpService _cmdHelpService;

        public CommandAutocomplete(CommandHelpService cmdHelpService)
        {
            _cmdHelpService = cmdHelpService;
        }

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {

            var input = autocompleteInteraction.Data.Current.Value as string;

            var matches = _cmdHelpService.GetCmdsStartsWithInput(context.User, input)
                .Select(cmd =>
                {

                    if (cmd is CommandInfo regCmdInfo)
                        return regCmdInfo.Name;
                    else
                        return (cmd as IApplicationCommandInfo)?.Name;

                })
                .Distinct()
                .PartialSort(25);

            var results = AutocompletionResult.FromSuccess(matches.Select(cmdName => new AutocompleteResult(cmdName, cmdName)));

            return Task.FromResult(results);

        }
    }
}