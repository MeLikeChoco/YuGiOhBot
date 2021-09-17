using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models
{
    public class StringInputTypeReader : TypeReader
    {

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {

            if (string.IsNullOrEmpty(input))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Null or empty input"));

            return Task.FromResult(TypeReaderResult.FromSuccess(input.ConvertTypesetterToTypewriter().Trim()));

        }

    }
}
