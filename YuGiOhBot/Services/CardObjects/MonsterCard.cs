using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Services.CardObjects
{
    public class MonsterCard : YuGiOhCard
    {

        public string Attribute { get; set; } = string.Empty;
        public string Types { get; set; } = string.Empty;
        public string Atk { get; set; } = string.Empty;

    }
}
