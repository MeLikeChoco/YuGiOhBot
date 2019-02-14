using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Entities
{
    public class ModuleInfo
    {

        public IEnumerable<Card> Cards { get; set; }
        public IEnumerable<BoosterPack> BoosterPacks { get; set; }
        public IEnumerable<Error> Errors { get; set; }

    }
}
