using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOhV2.Objects.Criterion
{
    public class GuessCriteria : ICriterion<SocketMessage>
    {

        private string _answer;

        public GuessCriteria(string answer)
        {

            _answer = answer;

        }

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {

            return Task.FromResult(parameter.Content == _answer);

        }

    }
}
