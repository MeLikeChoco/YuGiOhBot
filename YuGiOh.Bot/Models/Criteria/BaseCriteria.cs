using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria
{
    public class BaseCriteria : Criteria
    {

        public BaseCriteria(ICommandContext context)
        {

            AddCriteria(new UserCriteria(context.User));
            AddCriteria(new ChannelCriteria(context.Channel));

        }

        public BaseCriteria(IInteractionContext context)
        {

            AddCriteria(new UserCriteria(context.User));
            AddCriteria(new ChannelCriteria(context.Channel));

        }

    }
}
