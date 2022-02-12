using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace YuGiOh.Bot.Extensions
{
    public static class SocketSlashCommandExtensions
    {

        public static string GetCmdString(this SocketSlashCommand cmd)
        {

            var strBuilder = new StringBuilder("/").Append(cmd.CommandName);

            var cmdData = cmd.Data;
            var parameters = cmdData.Options.Aggregate(strBuilder, (accumulator, option) => accumulator.Append(' ').Append(option.Value));

            return strBuilder.ToString();

        }

    }
}
