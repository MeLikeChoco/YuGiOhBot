using Discord.WebSocket;

namespace YuGiOh.Bot.Extensions;

public static class SocketUserExtensions
{

    /// <summary>
    /// Get username with discriminator
    /// </summary>
    /// <returns>{username}#{discriminator}</returns>
    public static string GetFullUsername(this SocketUser user)
        => $"{user.Username}#{user.Discriminator}";

}