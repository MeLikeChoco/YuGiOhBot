using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace YuGiOh.Bot.Models.TypeReaders
{
    public class StringCommandInputTypeReader : TypeReader
    {

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {

            if (string.IsNullOrEmpty(input))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Null or empty input"));

            input = TypeReaderUtils.SanitizeInput(input);

            return Task.FromResult(TypeReaderResult.FromSuccess(input));

        }

    }
}