using Dapper.Contrib.Extensions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects
{
    public class Setting
    {

        [ExplicitKey]
        public ulong Id { get; set; }
        public string Prefix { get; set; } = "y!";
        public bool Minimal { get; set; } = false;
        public int GuessTime { get; set; } = 60;
        public bool AutoDelete { get; set; } = true;
        public bool Inline { get; set; } = true;

        public Setting(SocketGuild guild)
        {

            Id = guild.Id;

        }

        public Setting(ulong id, string prefix, bool minimal, int guessTime, bool inLine)
        {

            Id = id;
            Prefix = prefix;
            Minimal = minimal;
            GuessTime = guessTime;
            Inline = inLine;

        }
        
        public Setting() { }

    }
}
