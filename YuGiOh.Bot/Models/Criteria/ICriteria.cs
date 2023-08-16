using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria;

public interface ICriteria
{

    Task<bool> ValidateAsync(ICommandContext context, SocketMessage message);
    Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message);

}