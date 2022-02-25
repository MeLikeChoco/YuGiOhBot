using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using MoreLinq;
using YuGiOh.Bot.Extensions;
using ModuleInfo = Discord.Interactions.ModuleInfo;

namespace YuGiOh.Bot.Services
{
    public class CommandHelpService : IComparer<object>
    {

        private readonly IDiscordClient _client;
        private readonly CommandService _cmdService;
        private readonly InteractionService _interactService;

        private readonly Task<ulong> _ownerIdTask;
        private readonly Task<IEnumerable<object>> _commandsTask;

        public CommandHelpService(
            DiscordShardedClient client,
            CommandService cmdService,
            InteractionService interactService
        )
            : this(client as IDiscordClient, cmdService, interactService) { }

        public CommandHelpService(
            DiscordSocketClient client,
            CommandService cmdService,
            InteractionService interactService
        )
            : this(client as IDiscordClient, cmdService, interactService) { }

        protected CommandHelpService(
            IDiscordClient client,
            CommandService cmdService,
            InteractionService interactService
        )
        {

            _client = client;
            _cmdService = cmdService;
            _interactService = interactService;
            _ownerIdTask = client.GetApplicationInfoAsync().ContinueWith(task => task.Result.Owner.Id);
            _commandsTask = Task.FromResult(
                _cmdService.Commands
                    .SortedMerge(
                        OrderByDirection.Ascending, this,
                        _interactService.SlashCommands,
                        _interactService.ContextCommands,
                        _interactService.ComponentCommands
                    )
            );

        }

        public IEnumerable<object> GetCmds(IUser user)
            => GetCmdsInValidModules(user);

        public IEnumerable<object> GetCmds(IUser user, string input)
        {

            return GetCmdsInValidModules(user)
                .Where(cmd
                    => cmd switch
                    {
                        CommandInfo regCmdInfo => regCmdInfo.Name.EqualsIgnoreCase(input) || regCmdInfo.Aliases.Contains(input, StringComparer.OrdinalIgnoreCase),
                        ICommandInfo appCmdInfo => appCmdInfo.Name.EqualsIgnoreCase(input),
                        _ => false
                    }
                );

        }

        public IEnumerable<object> GetCmdsStartsWithInput(IUser user, string input, StringComparison comparer = StringComparison.OrdinalIgnoreCase)
        {

            return GetCmdsInValidModules(user)
                .Where(cmd
                    => cmd switch
                    {
                        CommandInfo regCmdInfo => regCmdInfo.Name.StartsWith(input, comparer),
                        ICommandInfo appCmdInfo => appCmdInfo.Name.StartsWith(input, comparer),
                        _ => false
                    }
                );

        }

        private IEnumerable<object> GetCmdsInValidModules(IUser user)
        {

            var isOwner = user.Id == _ownerIdTask.Result;

            bool IsValid(object info)
                => !DoesHaveReqOwner(info) || (DoesHaveReqOwner(info) && isOwner);

            return _cmdService.Modules
                .Where(IsValid)
                .SelectMany(module => module.Commands)
                .SortedMerge(OrderByDirection.Ascending, this,
                    _interactService.Modules
                        .Where(IsValid)
                        .SelectMany(module => module.SlashCommands.Concat<object>(module.ContextCommands).Concat(module.ComponentCommands))
                )
                .Where(IsValid);

        }

        private bool DoesHaveReqOwner(object info)
        {

            return info switch
            {

                CommandInfo cmdInfo => cmdInfo.Preconditions.Any(preconditions => preconditions.GetType() == typeof(Discord.Commands.RequireOwnerAttribute)),
                ICommandInfo appCmdInfo => appCmdInfo.Preconditions.Any(precondition => precondition.GetType() == typeof(Discord.Interactions.RequireOwnerAttribute)),
                Discord.Commands.ModuleInfo regModInfo => regModInfo.Preconditions.Any(precondition => precondition.GetType() == typeof(Discord.Commands.RequireOwnerAttribute)),
                _ => ((ModuleInfo) info).Preconditions.Any(precondition => precondition.GetType() == typeof(Discord.Interactions.RequireOwnerAttribute))

            };

        }

        public int Compare(object x, object y)
        {

            var xCmd = x is CommandInfo xCmdInfo ? xCmdInfo.Name : (x as ICommandInfo)?.Name;
            var yCmd = y is CommandInfo yCmdInfo ? yCmdInfo.Name : (y as ICommandInfo)?.Name;

            return xCmd.CompareTo(yCmd);

        }

    }
}