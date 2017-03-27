using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Services.CardObjects
{
    public class RegularMonster : MonsterCard
    {

        public string Level { get; set; } = string.Empty;
        public string Def { get; set; } = string.Empty;

    }
}
