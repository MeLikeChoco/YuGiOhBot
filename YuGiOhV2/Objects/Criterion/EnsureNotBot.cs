using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOhV2.Objects.Criterion
{
    public class EnsureNotBot : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            return Task.FromResult(!parameter.Author.IsBot);
        }
    }
}