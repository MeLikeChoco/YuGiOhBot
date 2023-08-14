using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dommel;

namespace YuGiOh.Common.Models.YuGiOh
{
    [Table("cards")]
    public class CardEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string RealName { get; set; }

        public string CardType { get; set; }
        public string Property { get; set; }
        public string Types { get; set; }
        public string Attribute { get; set; }
        public string Materials { get; set; }

        public string Lore { get; set; }
        public string PendulumLore { get; set; }

        [Ignore]
        public List<TranslationEntity> Translations { get; set; }

        [Ignore]
        public List<string> Archetypes { get; set; }

        [Column("archetypes"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ArchetypesId { get; set; }

        [Ignore]
        public List<string> Supports { get; set; }

        [Column("supports"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupportsId { get; set; }

        [Ignore]
        public List<string> AntiSupports { get; set; }

        [Column("antisupports"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AntiSupportsId { get; set; }

        public int Link { get; set; }
        public string LinkArrows { get; set; }

        public string Atk { get; set; }
        public string Def { get; set; }

        //these values can have legit 0's
        public int Level { get; set; } = -1;
        public int PendulumScale { get; set; } = -1;
        public int Rank { get; set; } = -1;

        public bool TcgExists { get; set; }
        public bool OcgExists { get; set; }

        public string Img { get; set; }
        public string Url { get; set; }

        public string Passcode { get; set; }

        public string OcgStatus { get; set; } = "N/A";
        public string TcgAdvStatus { get; set; } = "N/A";
        public string TcgTrnStatus { get; set; } = "N/A";

        public string CardTrivia { get; set; }

    }
}