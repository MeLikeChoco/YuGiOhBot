using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.BoosterPacks
{

    public enum Rarity
    {
        
        Common,
        [Rarity("Short Print")]
        Short,
        [Rarity("Super Short Print")]
        SuperShort,
        Rare,
        [Rarity("Super Rare")]
        Super,
        [Rarity("Holofoil Rare")]
        Holo,
        [Rarity("Ultra Rare")]
        Ultra,
        [Rarity("Ultimate Rare")]
        Ultimate,
        [Rarity("Secret Rare")]
        Secret,
        [Rarity("Ultra Secret Rare")]
        UltraSecret,
        [Rarity("Secret Ultra Rare")]
        SecretUltra,
        [Rarity("Prismatic Secret Rare")]
        Prismatic,
        [Rarity("Ghost Rare")]
        Ghost,
        [Rarity("Parallel Rare")]
        Parallel,
        [Rarity("Normal Parallel Rare")]
        NormalParallel,
        [Rarity("Super Parallel Rare")]
        SuperParallel,
        [Rarity("Ultra Rare")]
        UltraParallel,
        [Rarity("Duel Terminal Parallel Common")]
        DuelTerminal,
        [Rarity("Duel Terminal Rare Parallel Rare")]
        DuelTerminalRare,
        [Rarity("Duel Terminal Super Parallel Rare")]
        DuelTerminalSuper,
        [Rarity("Duel Terminal Ultra Parallel Rare")]
        DuelTerminalUltra,
        [Rarity("Duel Terminal Secret Parallel Rare")]
        DuelTerminalSecret,
        [Rarity("Gold Rare")]
        Gold

    }

    public class RarityAttribute : Attribute
    {

        public string Name { get; set; }

        public RarityAttribute(string name)
            => Name = name;

    }

}
