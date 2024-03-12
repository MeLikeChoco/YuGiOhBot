using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YuGiOh.Common.Models.YuGiOh;

public class TranslationEntity
{

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("translationid")]
    public int Id { get; set; }

    [Column("cardid")]
    public int CardId { get; set; }

    [Column("language")]
    public string Language { get; set; }

    [Column("translationname")]
    public string Name { get; set; }

    [Column("translationlore")]
    public string Lore { get; set; }

}