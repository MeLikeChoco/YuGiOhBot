using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Common.Models.YuGiOh
{
    public class TranslationEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("translationid")]
        public int Id { get; set; }
        public int CardId { get; set; }
        public string Language { get; set; }
        [Column("translationname")]
        public string Name { get; set; }
        [Column("translationlore")]
        public string Lore { get; set; }

    }
}
