using Discord.WebSocket;

namespace YuGiOh.Bot.Extensions;

public static class SocketTextChannelExtensions
{

    /// <summary>
    /// Get guild name with text channel name
    /// </summary>
    /// <param name="txtChannel"></param>
    /// <returns>{guild}/{channel}</returns>
    public static string GetGuildAndChannel(this SocketTextChannel txtChannel)
        => $"{txtChannel.Guild.Name}/{txtChannel.Name}";

}