using Discord.Addons.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Criterion
{
    public class GuessCriteria : ICriterion<SocketMessage>
    {

        private readonly string _answer;

        public GuessCriteria(string answer)
            => _answer = answer;

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
            => Task.FromResult(parameter.Content.Equals(_answer, StringComparison.OrdinalIgnoreCase));

    }
}
