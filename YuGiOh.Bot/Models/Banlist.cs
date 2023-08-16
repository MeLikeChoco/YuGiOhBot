using System.Collections.Generic;

//yes, I could have easily stored this in a crude dictionary-list relationship
//but this has better organization
//and I could serialize this into json if i wanted to for no apparent reason

namespace YuGiOh.Bot.Models
{
    public class Banlist
    {

        public Ocg OcgBanlist { get; set; }
        public TcgAdv TcgAdvBanlist { get; set; }
        public TcgTrad TcgTradBanlist { get; set; }

        public Banlist()
        {

            OcgBanlist = new Ocg();
            TcgAdvBanlist = new TcgAdv();
            TcgTradBanlist = new TcgTrad();

        }

    }

    public class Ocg : BanlistFormat { }

    public class TcgAdv : BanlistFormat { }

    public class TcgTrad : BanlistFormat { }

    public abstract class BanlistFormat
    {

        public IEnumerable<string> Forbidden { get; set; }
        public IEnumerable<string> Limited { get; set; }
        public IEnumerable<string> SemiLimited { get; set; }

    }
}