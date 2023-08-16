using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria;

public class UserCriteria : ICriteria
{

    private readonly ulong _userId;

    public UserCriteria(IUser user)
    {
        _userId = user.Id;
    }

    public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
        => Task.FromResult(_userId == message.Author.Id);

    public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
        => Task.FromResult(_userId == message.Author.Id);

}