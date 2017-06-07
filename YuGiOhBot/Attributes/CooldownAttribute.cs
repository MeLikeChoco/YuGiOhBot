using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhBot.Core;

namespace YuGiOhBot.Attributes
{

    [AttributeUsage(AttributeTargets.Method)]
    public class CooldownAttribute : PreconditionAttribute
    {

        private int _cooldown;
        private static ConcurrentDictionary<string, ConcurrentDictionary<ulong, Stopwatch>> _cooldownList = new ConcurrentDictionary<string, ConcurrentDictionary<ulong, Stopwatch>>();
        private static Timer _clearCooldownlist = new Timer(ClearDictionary, null, 1000, 60000);

        /// <summary> Sets how many seconds between each command </summary>
        /// <param name="cooldown"> The amount of seconds betweeen each invocation of the command </param>
        public CooldownAttribute(int cooldown)
            => _cooldown = cooldown;

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {

            if (!_cooldownList.Keys.Contains(command.Name))
                _cooldownList.TryAdd(command.Name, new ConcurrentDictionary<ulong, Stopwatch>());

            if (_cooldownList.TryGetValue(command.Name, out ConcurrentDictionary<ulong, Stopwatch> guildCooldowns))
            {

                if (guildCooldowns.TryGetValue(context.Guild.Id, out Stopwatch sw))
                {

                    if (sw.Elapsed < TimeSpan.FromSeconds(_cooldown))
                    {

                        TimeSpan remaining = TimeSpan.FromSeconds(_cooldown) - sw.Elapsed;

                        try
                        {
                            context.Channel.SendMessageAsync($"Cooldown until next usage: {remaining.Seconds} seconds");
                        }
                        catch { }

                        AltConsole.PrintAsync("Precondition", "Cooldown", $"{context.Guild.Name} triggered cooldown message!").GetAwaiter().GetResult();
                        return Task.FromResult(PreconditionResult.FromError($"Cooldown until next usage: {remaining.Seconds} seconds"));

                    }
                    else
                    {

                        guildCooldowns.TryRemove(context.Guild.Id, out sw);
                        sw.Stop();
                        return Task.FromResult(PreconditionResult.FromSuccess());

                    }

                }
                else
                {

                    sw = new Stopwatch();
                    sw.Start();

                    guildCooldowns.TryAdd(context.Guild.Id, sw);
                    return Task.FromResult(PreconditionResult.FromSuccess());

                }

            }

            return Task.FromResult(PreconditionResult.FromError("Some error happened, lel"));

        }

        private static void ClearDictionary(object state)
        {

            foreach(KeyValuePair<string, ConcurrentDictionary<ulong, Stopwatch>> commandCd in _cooldownList)
            {

                var removeStopwatches = new List<ulong>();

                foreach(KeyValuePair<ulong, Stopwatch> cooldowns in commandCd.Value)
                {

                    if (cooldowns.Value.Elapsed > TimeSpan.FromSeconds(60))
                        removeStopwatches.Add(cooldowns.Key);

                }

                removeStopwatches.ForEach(id => commandCd.Value.TryRemove(id, out Stopwatch gey));

            }

        }

    }
}
