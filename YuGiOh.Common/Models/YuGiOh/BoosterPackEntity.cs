using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dommel;

namespace YuGiOh.Common.Models.YuGiOh
{
    [Table("boosterpacks")]
    public class BoosterPackEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        [Ignore]
        public List<BoosterPackDate> Dates { get; set; }
        [Ignore]
        public List<BoosterPackCard> Cards { get; set; }
        public string Url { get; set; }

        public bool TcgExists { get; set; }
        public bool OcgExists { get; set; }

    }

    public class BoosterPackCard
    {

        public string Name { get; set; }
        public List<string> Rarities { get; set; }

    }

    public class BoosterPackDate
    {

        public string Name { get; set; }
        public string Date { get; set; }

    }
}
