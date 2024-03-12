using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public List<BoosterPackDateEntity> Dates { get; set; }

        [Column("dates"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DatesId { get; set; }
        
        [Ignore]
        public List<BoosterPackCardEntity> Cards { get; set; }

        [Column("cards"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CardsId { get; set; }

        public string Url { get; set; }

        public bool TcgExists { get; set; }
        public bool OcgExists { get; set; }

    }

    [Table("boosterpack_cards")]
    public class BoosterPackCardEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BoosterPackCardsId { get; set; }

        [Column("boosterpackcardname")]
        public string Name { get; set; }

        [Column("rarities"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RaritiesId { get; set; }
        
        [Ignore]
        public List<string> Rarities { get; set; }

    }

    [Table("boosterpack_dates")]
    public class BoosterPackDateEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BoosterPackDatesId { get; set; }

        [Column("boosterpackdatename")]
        public string Name { get; set; }

        [Column("boosterpackdate")]
        public string Date { get; set; }

    }
}