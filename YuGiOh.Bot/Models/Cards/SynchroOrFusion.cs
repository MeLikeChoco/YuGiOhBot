using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace YuGiOh.Bot.Models.Cards
{
    public class SynchroOrFusion : RegularMonster, IHasMaterials
    {

        #region Colors
        private static readonly Color FusionColor = new Color(160, 134, 183);
        private static readonly Color SynchroColor = new Color(204, 204, 204);
        #endregion Colors

        public string Materials { get; set; }

        protected override Color GetColor()
            => Types.Contains("Fusion") ? FusionColor : SynchroColor;

    }
}
