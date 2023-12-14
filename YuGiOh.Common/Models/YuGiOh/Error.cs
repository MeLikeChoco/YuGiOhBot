using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace YuGiOh.Common.Models.YuGiOh
{
    [Table("errors")]
    public class Error
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Url { get; set; }
        public Type Type { get; set; }
        public DateTime Timestamp { get; set; }

    }

    public enum Type
    {
        [PgName("Card")]
        Card,

        [PgName("Boosterpack")]
        Booster,
        
        [PgName("Anime")]
        Anime
    }
}