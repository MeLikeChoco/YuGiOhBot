using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace YuGiOh.Bot.Extensions
{
    public static class SocketGuildExtensions
    {

        public static async Task<IReadOnlyCollection<SocketApplicationCommand>> CreateOrOverwriteApplicationCommands(
            this SocketGuild guild,
            ApplicationCommandProperties[] cmds
        )
        {

            var results = new List<SocketApplicationCommand>();
            var existingCmds = await guild.GetApplicationCommandsAsync().ConfigureAwait(false);
            var newCmds = cmds.Where(cmd => existingCmds.All(existingCmd => existingCmd.Name != cmd.Name.Value));
            var oldCmds = cmds.Where(cmd => existingCmds.Any(existingCmd => existingCmd.Name == cmd.Name.Value));

            foreach (var cmd in newCmds)
            {

                var socketCmd = await guild.CreateApplicationCommandAsync(cmd).ConfigureAwait(false);

                results.Add(socketCmd);

            }

            var socketCmds = await guild.BulkOverwriteApplicationCommandAsync(oldCmds.ToArray()).ConfigureAwait(false);

            results.AddRange(socketCmds);

            return results;

        }

    }
}
