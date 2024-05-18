using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Water_MG.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        public int CustomerId { get; set; }

        public DateTime BillingDate { get; set; }

        public decimal AmountDue { get; set; }

        public bool IsPaid { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        // Navigation property
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
