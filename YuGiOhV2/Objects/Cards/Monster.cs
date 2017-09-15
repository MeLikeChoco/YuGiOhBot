using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public class Monster : Card
    {

        public string Attribute { get; set; }
        public string Types { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }
        [Column("pendulumScale")]
        public string Scale { get; set; } //I blame xyz pendulums

    }
}
