using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Entities
{

    public class BoosterPackCard
    {

        public string Name { get; set; }
        public IEnumerable<string> Rarities { get; set; }

    }

    public enum Rarity
    {

        Common,
        Short,
        SuperShort,
        Rare,
        Super,
        Holo,
        Ultra,
        Ultimate,
        Secret,
        UltraSecret,
        SecretUltra,
        Prismatic,
        Ghost,
        Parallel,
        SuperParallel,
        UltraParallel,
        DuelTerminal,
        DuelTerminalRare,
        DuelTerminalSuper,
        DuelTerminalUltra,
        DuelTerminalSecret,
        Gold

    }

}
