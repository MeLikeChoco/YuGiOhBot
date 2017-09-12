using Dapper.Contrib.Extensions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects
{
    public class Settings
    {

        [ExplicitKey]
        public ulong Id { get; set; }
        public string Prefix { get; set; } = "y!";
        public bool Minimal { get; set; } = false;

        public Settings(SocketGuild guild)
        {

            Id = guild.Id;

        }

    }
}
