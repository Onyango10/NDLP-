using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NDLP_Project.Models.Entities
{
    [Table("Tbl_AccountQueryRequest")]
    public class AccountQueryRequest
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(50)]
        [Index("IX_Query_RequestReference", IsUnique = true)]
        public string RequestReference { get; set; } // e.g. "REQ-20260907-0001" (Idempotency Key)
        [Required]
        [StringLength(20)]
        [Index("IX_Query_AccountNumber")]
        public string AccountNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string CustomerId { get; set; }
        [Required]
        [StringLength(50)]
        public string QueryType { get; set; } // "BALANCE_INQUIRY", "STATEMENT_QUERY", "ACCOUNT_DETAILS"
        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "PENDING", "IN_PROGRESS", "COMPLETED", "REJECTED"
        [StringLength(500)]
        public string Purpose { get; set; } // Reason for query or customer inquiry notes
        [StringLength(500)]
        public string ResolutionNotes { get; set; } // Notes when completed or updated (Ticket 6)
        // Submitter & Lifecycle tracking
        [Required]
        [StringLength(50)]
        public string RequestedBy { get; set; } // User/Teller/System who submitted
        [Column(TypeName = "datetime2")]
        public DateTime RequestedDate { get; set; }
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedDate { get; set; }
    }
}