using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public class Card
    {

        public string Name { get; set; }
        public string RealName { get; set; }
        public string CardType { get; set; }
        public string Lore { get; set; }

        public string[] Archetypes { get; set; }
        public string[] Supports { get; set; }
        public string[] AntiSupports { get; set; }

        public bool OcgExists { get; set; }
        public bool TcgExists { get; set; }

        public string Img { get; set; }
        public string Url { get; set; }
        public string Passcode { get; set; }

        public CardStatus OcgStatus { get; set; }
        public CardStatus TcgAdvStatus { get; set; }
        public CardStatus TcgTrnStatus { get; set; }

        public bool HasEffect
        {
            get
            {

                if (this is SpellTrap)
                    return true;
                else
                {

                    var monster = this as Monster;

                    if (monster.Types.Contains("Effect"))
                        return true;
                    else
                        return false;

                }

            }
        }

    }

    public enum CardStatus
    {

        Forbidden,
        Illegal,
        Legal,
        Limited,
        SemiLimited,
        Unreleased,
        Unlimited

    }

}
