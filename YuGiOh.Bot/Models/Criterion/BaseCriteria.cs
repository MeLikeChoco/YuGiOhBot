using Discord.Addons.Interactive;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criterion
{
    public class BaseCriteria : Criteria<SocketMessage>
    {

        public BaseCriteria()
        {

            AddCriterion(new EnsureSourceUserCriterion());
            AddCriterion(new EnsureSourceChannelCriterion());

        }

    }
}
