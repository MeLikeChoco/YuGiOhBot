﻿using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    internal class EnsureIsIntegerCriterion : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }

        public Task<bool> JudgeAsync(IInteractionContext sourceContext, SocketMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}
