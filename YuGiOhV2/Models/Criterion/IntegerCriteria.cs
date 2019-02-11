using Discord.Addons.Interactive;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace YuGiOhV2.Models.Criterion
{
    public class IntegerCriteria : ICriterion<SocketMessage>
    {

        private int _min;
        private int _max;

        public IntegerCriteria(int max) : this(1, max) { }

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
