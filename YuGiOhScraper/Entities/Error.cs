using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Entities
{
    public struct Error
    {

        public string Name { get; set; }
        public string Url { get; set; }
        public string Exception { get; set; }
        public string InnerException { get; set; }
        public string Type { get; set; }

    }
}
