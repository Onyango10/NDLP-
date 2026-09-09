using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NDLP_Project.Models.Entities
{
    [Table("tbl_AccountTransactions")]
    public class AccountTransaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        [Index("IX_TXN_AccountNumber")]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(30)]
        [Index("IX_TransactionReference",IsUnique = true)]
        public String TransactionReference { get; set; }


        [Column(TypeName = "datetime2")]
        public DateTime TransactionDate { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime ValueDate { get; set; }
        [Required]
        [StringLength(10)]
        public string TransactionType { get; set; } // "DEBIT" or "CREDIT"
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } // "KES", "USD"
        [Column(TypeName = "decimal")]
        public decimal RunningBalance { get; set; } // Balance after this transaction
        [Required]
        [StringLength(250)]
        public string Description { get; set; } // Narration (e.g. "ATM Withdrawal", "Salary")
        [StringLength(50)]
        public string Channel { get; set; } // "MOBILE", "ATM", "BRANCH", "WEB"
        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "POSTED", "PENDING", "REVERSED"
        // Auditing
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }
        [StringLength(50)]
        public string CreatedBy { get; set; }





    }
}