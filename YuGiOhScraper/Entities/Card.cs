using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Entities
{
    public class Card
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

    }
}
