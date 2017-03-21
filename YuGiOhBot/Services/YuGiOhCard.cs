using System.Collections.Generic;

namespace YuGiOhBot.Services
{
    public class YuGiOhCard
    {

        public string Format { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEffect { get; set; } = false;
        public string Level { get; set; } = string.Empty;
        public string LeftPend { get; set; } = string.Empty; //i know they are both the same for now
        public string RightPend { get; set; } = string.Empty; //but you never know that one day we may get both pends different
        public List<string> Types { get; set; } = null; //effect, synchro, union etc etc    DUAL IS GEMINI
        public string Atk { get; set; } = string.Empty;
        public string Def { get; set; } = string.Empty;
        public string Race { get; set; } = string.Empty; //beast, aqua, zombie etc etc
        public string Attribute { get; set; } = string.Empty; //earth, dark, light etc etc
        public string Archetype { get; set; } = string.Empty;
        public YuGiOhPriceSerializer Prices { get; set; } = null;
        public string ImageUrl { get; set; } = string.Empty;

    }
}
