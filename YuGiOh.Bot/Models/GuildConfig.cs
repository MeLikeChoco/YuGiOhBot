using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Models
{
    public class GuildConfig
    {

        public ulong Id { get; set; }
        public string Prefix { get; set; } = "y!";
        public bool Minimal { get; set; } = true;
        public int GuessTime { get; set; } = 60;
        public bool AutoDelete { get; set; } = true;
        public bool Inline { get; set; } = true;

    }
}
