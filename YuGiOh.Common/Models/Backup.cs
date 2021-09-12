using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Common.Models
{
    public class Backup
    {
        
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id {  get; set; }
        public Guid Guid { get; set; }
        [Column("created_time"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }

    }
}
