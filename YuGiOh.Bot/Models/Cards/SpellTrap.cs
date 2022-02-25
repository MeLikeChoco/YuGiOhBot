using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public class SpellTrap : Card, IHasProperty
    {

        #region Icon Urls

        private const string RitualIconUrl = "http://1.bp.blogspot.com/-AuufBN2P_2Q/UxXrMJAkPJI/AAAAAAAAByQ/ZFuEQPj-UtQ/s1600/Ritual.png";
        private const string QuickPlayIconUrl = "http://4.bp.blogspot.com/-4neFVlt9xyk/UxXrMO1cynI/AAAAAAAAByY/WWRyA3beAl4/s1600/Quick-Play.png";
        private const string FieldIconUrl = "http://1.bp.blogspot.com/-3elroOLxcrM/UxXrK5AzXuI/AAAAAAAABxo/qrMUuciJm8s/s1600/Field.png";
        private const string EquipIconUrl = "http://1.bp.blogspot.com/-_7q4XTlAX_g/UxXrKeKbppI/AAAAAAAABxY/uHl2cPYY6PA/s1600/Equip.png";
        private const string CounterIconUrl = "http://3.bp.blogspot.com/-EoqEY8ef698/UxXrJRfgnPI/AAAAAAAABxA/e9-pD6CSdwk/s1600/Counter.png";
        private const string ContinuousIconUrl = "http://3.bp.blogspot.com/-O_1NZeHQBSk/UxXrJfY0EEI/AAAAAAAABxI/vKg5txOFlog/s1600/Continuous.png";
        private const string SpellIconUrl = "http://2.bp.blogspot.com/-RS2Go77CqUw/UxXrMaDiM-I/AAAAAAAAByU/cjc2OyyUzvM/s1600/Spell.png";
        private const string TrapIconUrl = "http://3.bp.blogspot.com/-o8wNPTv-VVw/UxXrNA8kTMI/AAAAAAAAByw/uXwjDLJZPxI/s1600/Trap.png";

        #endregion Icon Urls

        #region Colors

        private static readonly Color SpellColor = new Color(29, 158, 116);
        private static readonly Color TrapColor = new Color(188, 90, 132);

        #endregion Colors

        public string Property { get; set; }

        public override bool HasEffect { get; } = true;

        protected override string GetIconUrl()
            => Property switch
            {
                "Ritual" => RitualIconUrl,
                "Quick-Play" => QuickPlayIconUrl,
                "Field" => FieldIconUrl,
                "Equip" => EquipIconUrl,
                "Counter" => CounterIconUrl,
                "Continuous" => ContinuousIconUrl,
                _ => CardType == CardType.Spell ? SpellIconUrl : TrapIconUrl
            };

        protected override Color GetColor()
            => CardType switch
            {
                CardType.Spell => SpellColor,
                CardType.Trap => TrapColor,
                _ => TrapColor,
            };

        protected override List<string> GetDescription()
            => base.GetDescription().With($"**Property:** {Property}");

    }

    //public class SpellTrapProperty
    //{

    //    #region Attributes
    //    public static readonly SpellTrapProperty Ritual = new SpellTrapProperty("Ritual");
    //    public static readonly SpellTrapProperty QuickPlay = new SpellTrapProperty("Quick-Play");
    //    public static readonly SpellTrapProperty Field = new SpellTrapProperty("Field");
    //    public static readonly SpellTrapProperty Equip = new SpellTrapProperty("Equip");
    //    public static readonly SpellTrapProperty Counter = new SpellTrapProperty("Counter");
    //    public static readonly SpellTrapProperty Continuous = new SpellTrapProperty("Continuous");
    //    #endregion Attributes

    //    private static readonly IEnumerable<SpellTrapProperty> Attributes = typeof(SpellTrapProperty)
    //        .GetFields(BindingFlags.Static | BindingFlags.Public)
    //        .Select(field => field.GetValue(null) as SpellTrapProperty);

    //    private string _property;

    //    private SpellTrapProperty(string property)
    //        => _property = property;

    //    public override string ToString()
    //        => _property;

    //    public static SpellTrapProperty Parse(string input)
    //        => Attributes.FirstOrDefault(property => property.ToString() == input);

    //}
}