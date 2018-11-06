using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public abstract class CardParser
    {

        public string Name { get; set; }
        public string RealName { get; set; }

        public string CardType { get; set; }
        public string Property { get; set; }
        public string Types { get; set; }
        public string Attribute { get; set; }
        public string Materials { get; set; }
        public string Lore { get; set; }

        public string Archetype { get; set; }
        public string Supports { get; set; }
        public string AntiSupports { get; set; }

        public int Link { get; set; }
        public string LinkArrows { get; set; }

        public string Atk { get; set; }
        public string Def { get; set; }

        public int Level { get; set; }
        public int PendulumScale { get; set; }
        public int Rank { get; set; }

        public bool TcgExists { get; set; }
        public bool OcgExists { get; set; }

        public string Img { get; set; }
        public string Url { get; set; }

        public string Passcode { get; set; }

        public string OcgStatus { get; set; }
        public string TcgAdvStatus { get; set; }
        public string TcgTrnStatus { get; set; }

        public Card Parse()
        {

            Card card;

            if ((Level == -1 && PendulumScale == -1) || (Types.ToLower().Contains("pendulum") && !Types.ToLower().Contains("xyz")))
                card = new RegularMonster();
            else if (Types.ToLower().Contains("xyz"))
                card = new Xyz();
            else if (Types.ToLower().Contains("link"))
                card = new LinkMonster();
            else
                card = new SpellTrap();

            card.Name = Name;
            card.RealName = RealName;
            card.CardType = CardType;
            card.Lore = Lore;
            card.Archetype = Archetype.Split(';');
            card.Supports = Supports.Split(';');
            card.AntiSupports = Supports.Split(';');
            card.OcgExists = OcgExists;
            card.TcgExists = TcgExists;
            card.Img = Img;
            card.Url = Url;
            card.Passcode = Passcode;

            if (card.OcgExists)
                card.OcgStatus = GetCardStatus(OcgStatus);

            if (card.TcgExists)
            {

                card.TcgAdvStatus = GetCardStatus(TcgAdvStatus);
                card.TcgTrnStatus = GetCardStatus(TcgTrnStatus);

            }

            if (card is Monster monster)
            {

                monster.Attribute = Attribute;
                monster.Types = Types.Split(',');
                monster.Atk = Atk;
                monster.Def = Def;

                if (PendulumScale != -1)
                    monster.PendulumScale = PendulumScale;

                if (!string.IsNullOrEmpty(Materials))
                    monster.Materials = Materials;

                if (monster is Xyz xyz)
                    xyz.Rank = Rank;

            }

            return card;

        }

        private CardStatus GetCardStatus(string status)
        {

            status = status.ToLower();

            if (status.Contains("forbidden"))
                return CardStatus.Forbidden;
            if (status.Contains("illegal"))
                return CardStatus.Illegal;
            if (status.Contains("legal"))
                return CardStatus.Legal;
            if (status.Contains("not yet released"))
                return CardStatus.Unreleased;
            if (status.Contains("unlimited"))
                return CardStatus.Unlimited;  //unlimited and semi limited is checked first because they both contains "limited"
            if (status.Contains("semi") && status.Contains("limited"))
                return CardStatus.SemiLimited;

            return CardStatus.Limited;

        }

    }
}
