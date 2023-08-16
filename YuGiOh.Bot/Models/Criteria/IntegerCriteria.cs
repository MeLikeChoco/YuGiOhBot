using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria
{
    public class IntegerCriteria : ICriteria
    {

        private int _min;
        private int _max;

        public IntegerCriteria(int max) : this(1, max) { }

        public IntegerCriteria(int min, int max)
        {

            _min = min;
            _max = max;

        }

        public Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
        {

            var content = message.Content;
            
            return Task.FromResult(int.TryParse(content, out var selection) && selection <= _max && selection >= _min);
            
        }

        public Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
        {

            var content = message.Content;
            
            return Task.FromResult(int.TryParse(content, out var selection) && selection <= _max && selection >= _min);
            
        }
        
    }
}
