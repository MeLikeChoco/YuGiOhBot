using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;

namespace YuGiOh.Bot.Extensions
{
    public static class DiscordRestClientExtensions
    {

        public static async Task<IEnumerable<RestGlobalCommand>> CreateOrOverwriteBulkGlobalApplicationCommands(
            this DiscordRestClient client,
            ApplicationCommandProperties[] cmds
        )
        {

            var results = new List<RestGlobalCommand>();
            var existingCmds = await client.GetGlobalApplicationCommands().ConfigureAwait(false);
            var newCmds = cmds.Where(cmd => existingCmds.All(existingCmd => existingCmd.Name != cmd.Name.Value));
            var oldCmds = cmds.Where(cmd => existingCmds.Any(existingCmd => existingCmd.Name == cmd.Name.Value));

            foreach (var cmd in newCmds)
            {

                var restCmd = await client.CreateGlobalCommand(cmd).ConfigureAwait(false);

                results.Add(restCmd);

            }

            var restCmds = await client.BulkOverwriteGlobalCommands(oldCmds.ToArray()).ConfigureAwait(false);

            results.AddRange(restCmds);

            return results;

        }

    }
}
