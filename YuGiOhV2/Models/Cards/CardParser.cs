using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.Cards
{
    public class CardParser
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
            var types = Types?.ToLower().Split(" / ");

            if(types != null)
            {

                if (types.Contains("link"))
                    card = new LinkMonster();
                else if (types.Contains("xyz") && types.Contains("pendulum"))
                    card = new XyzPendulum();
                else if ((types.Contains("synchro") || types.Contains("fusion")) && types.Contains("pendulum"))
                    card = new SynchroAndFusionPendulum();
                else if (types.Contains("xyz"))
                    card = new Xyz();
                else if (types.Contains("pendulum"))
                    card = new PendulumMonster();
                else if (types.Contains("synchro") || types.Contains("fusion"))
                    card = new SynchroAndFusion();
                else
                    card = new RegularMonster();

            }
            else
                card = new SpellTrap();

            card.Name = Name;
            card.RealName = RealName;
            card.CardType = CardType;
            card.Lore = Lore;
            card.Archetypes = Archetype?.Split(',');
            card.Supports = Supports?.Split(',');
            card.AntiSupports = AntiSupports?.Split(',');
            card.OcgExists = OcgExists;
            card.TcgExists = TcgExists;
            card.Img = Img;
            card.Url = Url;
            card.Passcode = Passcode?.Trim('0');

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
                monster.Types = Types.Split(" / ");

                if (monster is IHasAtk hasAtk)
                    hasAtk.Atk = Atk;

                if (monster is IHasDef hasDef)
                    hasDef.Def = Def;

                if (monster is IHasLevel hasLevel)
                    hasLevel.Level = Level;

                if (monster is IHasLink hasLink)
                {

                    hasLink.Link = Link;
                    hasLink.LinkArrows = LinkArrows.Split(',');

                }

                if (monster is IHasMaterials hasMaterials)
                    hasMaterials.Materials = Materials;

                if (monster is IHasRank hasRank)
                    hasRank.Rank = Rank;

                if (monster is IHasScale hasScale)
                    hasScale.PendulumScale = PendulumScale;

            }

            if (card is IHasProperty hasProperty)
                hasProperty.Property = Property;

            return card;

        }

        public static Card Parse(CardParser parser)
        {

            Card card;
            var types = parser.Types?.ToLower().Split(" / ");

            if (types.Contains("link"))
                card = new LinkMonster();
            else if (types.Contains("xyz") && types.Contains("pendulum"))
                card = new XyzPendulum();
            else if ((types.Contains("synchro") || types.Contains("fusion")) && types.Contains("pendulum"))
                card = new SynchroAndFusionPendulum();
            else if (types.Contains("xyz"))
                card = new Xyz();
            else if (types.Contains("pendulum"))
                card = new PendulumMonster();
            else if (types.Contains("synchro") || types.Contains("fusion"))
                card = new SynchroAndFusion();
            else if (types.Any())
                card = new RegularMonster();
            else
                card = new SpellTrap();

            card.Name = parser.Name;
            card.RealName = parser.RealName;
            card.CardType = parser.CardType;
            card.Lore = parser.Lore;
            card.Archetypes = parser.Archetype.Split(';');
            card.Supports = parser.Supports.Split(';');
            card.AntiSupports = parser.Supports.Split(';');
            card.OcgExists = parser.OcgExists;
            card.TcgExists = parser.TcgExists;
            card.Img = parser.Img;
            card.Url = parser.Url;
            card.Passcode = parser.Passcode;

            if (card.OcgExists)
                card.OcgStatus = GetCardStatus(parser.OcgStatus);

            if (card.TcgExists)
            {

                card.TcgAdvStatus = GetCardStatus(parser.TcgAdvStatus);
                card.TcgTrnStatus = GetCardStatus(parser.TcgTrnStatus);

            }

            if (card is Monster monster)
            {

                monster.Attribute = parser.Attribute;
                monster.Types = parser.Types.Split(',');

                if (monster is IHasAtk hasAtk)
                    hasAtk.Atk = parser.Atk;

                if (monster is IHasDef hasDef)
                    hasDef.Def = parser.Def;

                if (monster is IHasScale hasScale)
                    hasScale.PendulumScale = parser.PendulumScale;

                if (monster is IHasMaterials hasMaterials)
                    hasMaterials.Materials = parser.Materials;

                if (monster is IHasRank hasRank)
                    hasRank.Rank = parser.Rank;

                if (monster is IHasLink hasLink)
                {

                    hasLink.Link = parser.Link;
                    hasLink.LinkArrows = parser.LinkArrows.Split(',');

                }

            }

            return card;

        }

        public static CardStatus GetCardStatus(string status)
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

        public static string GetCardName(CardParser parser)
            => parser.Name;

    }
}
