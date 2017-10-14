using Discord.Addons.Interactive;
using Discord.WebSocket;

namespace YuGiOhV2.Objects.Criterion
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