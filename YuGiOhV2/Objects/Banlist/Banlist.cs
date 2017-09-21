using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//yes, I could have easily stored this in a crude dictionary-list relationship
//but this has better organization
//and I could serialize this into json if i wanted to for no apparent reason

namespace YuGiOhV2.Objects.Banlist
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

    public class Ocg : IFormat
    {

        public IEnumerable<string> Forbidden { get; set; }
        public IEnumerable<string> Limited { get; set; }
        public IEnumerable<string> SemiLimited { get; set; }

    }

    public class TcgAdv : IFormat
    {

        public IEnumerable<string> Forbidden { get; set; }
        public IEnumerable<string> Limited { get; set; }
        public IEnumerable<string> SemiLimited { get; set; }

    }

    public class TcgTrad : IFormat
    {
        
        public IEnumerable<string> Forbidden { get; set; }
        public IEnumerable<string> Limited { get; set; }
        public IEnumerable<string> SemiLimited { get; set; }

    }
}
