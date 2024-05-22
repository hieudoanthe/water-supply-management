using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Water_MG.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int BillId { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public decimal AmountPaid { get; set; }

        public string TypePay {  get; set; }

        [ForeignKey("BillId")]
        public virtual Bill Bill { get; set; }
    }
}
