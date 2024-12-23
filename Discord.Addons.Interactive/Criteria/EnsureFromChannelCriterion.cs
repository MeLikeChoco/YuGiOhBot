﻿using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class EnsureFromChannelCriterion : ICriterion<IMessage>
    {
        private readonly ulong _channelId;

        public EnsureFromChannelCriterion(IMessageChannel channel)
            => _channelId = channel.Id;

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            bool ok = _channelId == parameter.Channel.Id;
            return Task.FromResult(ok);
        }

        public Task<bool> JudgeAsync(IInteractionContext sourceContext, IMessage parameter)
        {
            var ok = _channelId == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}
