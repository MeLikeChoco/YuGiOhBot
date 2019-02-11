using Discord.Addons.Interactive;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Criterion
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
