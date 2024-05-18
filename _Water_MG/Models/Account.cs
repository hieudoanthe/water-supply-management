using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Water_MG.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(50)]
        public string Role { get; set; }

        public string TypeAccount { get; set; }

        // Navigation property
        public virtual ICollection<Customer> Customers { get; set; }

        public Account()
        {
            this.Role = "User";
            this.TypeAccount = "CN";
        }
    }
}