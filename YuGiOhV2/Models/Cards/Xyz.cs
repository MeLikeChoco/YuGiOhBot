using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using YuGiOhV2.Extensions;

namespace YuGiOhV2.Models.Cards
{
    public class Xyz : Monster, IHasRank, IHasMaterials, IHasAtk, IHasDef
    {

        private static readonly Color XyzColor = new Color(0, 0, 1);

        public int Rank { get; set; }
        public string Materials { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }

        protected override Color GetColor()
            => XyzColor;

        protected override List<string> GetDescription()
            => base.GetDescription().With($"**Rank:** {Rank}");

    }
}
