using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Water_MG.Models
{
    public class Meter
    {
        [Key]
        public int MeterId { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MeterNumber { get; set; }

        public DateTime? LastReadingDate { get; set; }

        public decimal? LastReadingValue { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}
