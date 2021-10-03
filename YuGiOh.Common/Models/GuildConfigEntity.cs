using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Common.Models
{
    [Table("configs")]
    public class GuildConfigEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string Prefix { get; set; } = "y!";
        public bool Minimal { get; set; } = true;
        public int GuessTime { get; set; } = 60;
        public int HangmanTime { get; set; } = 300;
        public bool AutoDelete { get; set; } = true;
        public bool Inline { get; set; } = true;
        public bool HangmanAllowWords { get; set; } = true;

    }
}
