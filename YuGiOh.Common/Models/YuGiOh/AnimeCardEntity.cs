using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YuGiOh.Common.Models.YuGiOh;

[Table("anime_cards")]
public class AnimeCardEntity
{

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public string Name { get; set; }

    public string CardType { get; set; }
    public string Property { get; set; }
    public string Attribute { get; set; }

    public int Link { get; set; }
    public string LinkArrows { get; set; }

    // public string Materials { get; set; }

    public string Level { get; set; } //because "???" and null actually exists
    public string PendulumScale { get; set; } //because "???" and null actually exists
    public string Rank { get; set; } //because "???" and null actually exists

    public string Types { get; set; }
    public string Lore { get; set; }

    public string Atk { get; set; }
    public string Def { get; set; }

    public string Appearances { get; set; }

    public string Img { get; set; }
    public string Url { get; set; }

}