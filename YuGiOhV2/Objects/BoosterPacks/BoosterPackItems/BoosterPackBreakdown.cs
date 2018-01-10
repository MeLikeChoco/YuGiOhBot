using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects.Attributes;

namespace YuGiOhV2.Objects.BoosterPacks.BoosterPackItems
{
    public class BoosterPackBreakdown
    {

        [Rarity("Common")]
        public int Common { get; set; }
        [Rarity("Rare")]
        public int Rare { get; set; }
        [Rarity("Super Rare")]
        public int SuperRare { get; set; }
        [Rarity("Holofoil Rare")]
        public int HolofoilRare { get; set; }
        [Rarity("Ultra Rare")]
        public int UltraRare { get; set; }
        [Rarity("Secret Rare")]
        public int SecretRare { get; set; }
        [Rarity("Ultra Secret Rare")]
        public int UltraSecretRare { get; set; }
        [Rarity("Secret Ultra Rare")]
        public int SecretUltraRare { get; set; }
        [Rarity("Ultimate Rare")]
        public int UltimateRare { get; set; }
        [Rarity("Holographic Rare")]
        public int HolographicRare { get; set; }
        [Rarity("Prismatic Secret Rare")]
        public int PrismaticSecretRare { get; set; }
        [Rarity("Ghost Rare")]
        public int GhostRare { get; set; }
        [Rarity("Parallel Rare")]
        public int ParallelRare { get; set; }
        [Rarity("Super Parallel Rare")]
        public int SuperParallelRare { get; set; }
        [Rarity("Ultra Parallel Rare")]
        public int UltraParallelRare { get; set; }
        [Rarity("Gold Rare")]
        public int GoldRare { get; set; }

    }
}
