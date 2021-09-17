using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public abstract class Monster : Card
    {

        #region Colors
        private static readonly Color FusionColor = new Color(160, 134, 183);
        private static readonly Color SynchroColor = new Color(204, 204, 204);
        private static readonly Color RitualColor = new Color(157, 181, 204);
        private static readonly Color EffectColor = new Color(255, 139, 83);
        private static readonly Color RegularColor = new Color(253, 230, 138);
        #endregion Colors

        #region Icon Urls
        private const string WindIconUrl = "http://1.bp.blogspot.com/-ndLNmGIXXKk/UxXrNXeUH-I/AAAAAAAABys/rdoqo1Bkhnk/s1600/Wind.png";
        private const string DarkIconUrl = "http://1.bp.blogspot.com/-QUU5KSFMYig/UxXrJZoOOfI/AAAAAAAABxE/7p8CLfWdTXA/s1600/Dark.png";
        private const string LightIconUrl = "http://1.bp.blogspot.com/-MxQabegkthM/UxXrLHywzrI/AAAAAAAABx8/h86nYieq9nc/s1600/Light.png";
        private const string EarthIconUrl = "http://2.bp.blogspot.com/-5fLcEnHAA9M/UxXrKAcSUII/AAAAAAAABxc/5fEingbdyXQ/s1600/Earth.png";
        private const string FireIconUrl = "http://4.bp.blogspot.com/-sS0-GqQ19gQ/UxXrLIymRVI/AAAAAAAAByA/aOAdiLerXoQ/s1600/Fire.png";
        private const string WaterIconUrl = "http://4.bp.blogspot.com/-A43QT1n8o5k/UxXrNJcG-fI/AAAAAAAAByo/0KFlRXQbZjI/s1600/Water.png";
        private const string DivineIconUrl = "http://1.bp.blogspot.com/-xZZF5E2NXi4/UxXrJwDWkaI/AAAAAAAABxg/EG-7ajL9WGc/s1600/Divine.png";
        #endregion Icon Urls

        public MonsterAttribute Attribute { get; set; }
        public string[] Types { get; set; }
        public override bool HasEffect => Types.Contains("Effect");

        protected override Color GetColor()
        {

            if (Types.Contains("Fusion"))
                return FusionColor;
            else if (Types.Contains("Synchro"))
                return SynchroColor;
            else if (Types.Contains("Ritual"))
                return RitualColor;
            else if (Types.Contains("Effect"))
                return EffectColor;
            else
                return RegularColor;

        }

        protected override string GetIconUrl()
            => Attribute switch
            {
                MonsterAttribute.WIND => WindIconUrl,
                MonsterAttribute.DARK => DarkIconUrl,
                MonsterAttribute.LIGHT => LightIconUrl,
                MonsterAttribute.EARTH => EarthIconUrl,
                MonsterAttribute.FIRE => FireIconUrl,
                MonsterAttribute.WATER => WaterIconUrl,
                MonsterAttribute.DIVINE => DivineIconUrl,
                _ => DefaultIconUrl,
            };

        protected override List<string> GetDescription()
            => base.GetDescription().With($"**Attribute:** {Attribute}");

    }

    public enum MonsterAttribute
    {

        Unknown,
        WIND,
        DARK,
        LIGHT,
        EARTH,
        FIRE,
        WATER,
        DIVINE

    }

}
