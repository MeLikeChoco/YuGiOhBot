using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using YuGiOhV2.Extensions;

namespace YuGiOhV2.Models
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
