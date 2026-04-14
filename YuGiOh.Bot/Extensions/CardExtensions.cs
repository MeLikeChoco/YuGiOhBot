using System;
using System.Linq;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Bot.Extensions
{
    public static class CardExtensions
    {

        public static Card ToModel(this CardEntity entity)
        {

            Card card;
            var types = entity.Types?.ToLower().Split(" / ");

            if (types is not null)
            {

                if (types.Contains("link"))
                    card = new LinkMonster();
                else if (types.Contains("xyz") && types.Contains("pendulum"))
                    card = new XyzPendulum();
                else if ((types.Contains("synchro") || types.Contains("fusion")) && types.Contains("pendulum"))
                    card = new SynchroOrFusionPendulum();
                else if (types.Contains("xyz"))
                    card = new Xyz();
                else if (types.Contains("pendulum"))
                    card = new PendulumMonster();
                else if (types.Contains("synchro") || types.Contains("fusion"))
                    card = new SynchroOrFusion();
                else
                    card = new RegularMonster();

            }
            else
                card = new SpellTrap();

            card.Name = entity.Name;
            card.RealName = entity.RealName;
            card.CardType = Enum.TryParse<CardType>(entity.CardType, true, out var cardType) ? cardType : CardType.Unknown;
            card.Lore = entity.Lore;
            card.Archetypes = entity.Archetypes;
            card.Supports = entity.Supports;
            card.AntiSupports = entity.AntiSupports;
            card.OcgExists = entity.OcgExists;
            card.TcgExists = entity.TcgExists;
            card.Img = entity.Img;
            card.Url = entity.Url;
            card.Passcode = entity.Passcode?.TrimStart('0');

            if (card.OcgExists)
                card.OcgStatus = GetCardStatus(entity.OcgStatus);

            if (card.TcgExists)
            {

                card.TcgStatus = GetCardStatus(entity.TcgStatus);
                // card.TcgTrnStatus = GetCardStatus(entity.TcgTrnStatus);

            }
            
            card.GenesysPoints = entity.Genesys;

            switch (card)
            {

                case Monster monster:
                {

                    monster.Attribute = Enum.TryParse<MonsterAttribute>(entity.Attribute, true, out var attribute) ? attribute : MonsterAttribute.Unknown;
                    monster.Types = entity.Types.Split(" / ");

                    if (monster is IHasAtk hasAtk)
                        hasAtk.Atk = entity.Atk;

                    if (monster is IHasDef hasDef)
                        hasDef.Def = entity.Def;

                    if (monster is IHasLevel hasLevel)
                        hasLevel.Level = entity.Level;

                    if (monster is IHasLink hasLink)
                    {

                        hasLink.Link = entity.Link;
                        hasLink.LinkArrows = entity.LinkArrows.Split(',');

                    }

                    if (monster is IHasMaterials hasMaterials)
                        hasMaterials.Materials = entity.Materials;

                    if (monster is IHasRank hasRank)
                        hasRank.Rank = entity.Rank;

                    if (monster is IHasScale hasScale)
                    {

                        hasScale.PendulumScale = entity.PendulumScale;
                        hasScale.PendulumLore = entity.PendulumLore;

                    }

                    break;

                }
                case IHasProperty hasProperty:
                    hasProperty.Property = entity.Property;
                    break;
            }

            return card;

        }

        private static CardStatus GetCardStatus(string status)
        {

            if (status is null || status.ContainsIgnoreCase("unreleased"))
                return CardStatus.Unreleased;
            if (status.ContainsIgnoreCase("forbidden"))
                return CardStatus.Forbidden;
            if (status.ContainsIgnoreCase("illegal"))
                return CardStatus.Illegal;
            if (status.ContainsIgnoreCase("legal"))
                return CardStatus.Legal;
            if (status.ContainsIgnoreCase("unlimited"))
                return CardStatus.Unlimited; //unlimited and semi limited is checked first because they both contain "limited"
            if (status.ContainsIgnoreCase("semi") && status.Contains("limited"))
                return CardStatus.SemiLimited;

            return status.ContainsIgnoreCase("limited") ? CardStatus.Limited : CardStatus.NA;

        }

    }
}