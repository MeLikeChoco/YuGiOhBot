namespace YuGiOhV2.Objects.Cards
{
    public class Monster : Card
    {

        public string Attribute { get; set; }
        public string Types { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }
        public string PendulumScale { get; set; } //I blame xyz pendulums
        public string Materials { get; set; }

    }
}
