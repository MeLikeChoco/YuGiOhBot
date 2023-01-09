using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace YuGiOh.Bot.Models.TypeReaders;

public class StringInteractionInputTypeConverter : TypeConverter<string>
{

    public override ApplicationCommandOptionType GetDiscordType()
        => ApplicationCommandOptionType.String;

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {

        var input = option.Value.ToString();
        
        if (string.IsNullOrEmpty(input))
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, "Null or empty input"));

        input = TypeReaderUtils.SanitizeInput(input);

        return Task.FromResult(TypeConverterResult.FromSuccess(input));
        
    }

}