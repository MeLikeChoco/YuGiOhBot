using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOhV2.Objects.Criterion
{
    public class IntegerCriteria : ICriterion<SocketMessage>
    {
        private readonly int _max;

        private readonly int _min;

        public IntegerCriteria(int max) : this(1, max)
        {
        }

        public IntegerCriteria(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            var content = parameter.Content;

            return Task.FromResult(int.TryParse(content, out var selection) && selection <= _max && selection >= _min);
        }
    }
}