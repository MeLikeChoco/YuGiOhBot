using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Attributes
{
    public class RequireChannelAttribute : PreconditionAttribute
    {

        private readonly ulong _id;

        public RequireChannelAttribute(ulong id)
            => _id = id;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {

            if (context.Channel.Id == _id)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Channel id does not meet id specified in precondition."));

        }

    }
}
