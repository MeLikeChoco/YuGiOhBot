using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YuGiOh.Common.Models.YuGiOh
{
    public class TranslationEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("translationid")]
        public int Id { get; set; }

        [Column("translationcardid")]
        public int CardId { get; set; }

        [Column("translatedlanguage")]
        public string Language { get; set; }

        [Column("translatedname")]
        public string Name { get; set; }

        [Column("translatedlore")]
        public string Lore { get; set; }

    }
}