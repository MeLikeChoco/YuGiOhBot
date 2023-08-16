using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria;

public class ChannelCriteria : ICriteria
{

    private readonly ulong _channelId;

    public ChannelCriteria(IMessageChannel channel)
    {
        _channelId = channel.Id;
    }

    public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
        => Task.FromResult(_channelId == message.Channel.Id);

    public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
        => Task.FromResult(_channelId == message.Channel.Id);

}