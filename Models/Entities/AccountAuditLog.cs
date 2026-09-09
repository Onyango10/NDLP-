using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NDLP_Project.Models.Entities
{
    [Table("tbl_AccountAuditLogs")]
    public class AccountAuditLog
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(20)]
        [Index("IX_Audit_AccountNumber")]
        public string AccountNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string Action { get; set; } // "VIEW_ACCOUNT", "SEARCH_STATEMENT", "CREATE_QUERY", "UPDATE_QUERY", "STATUS_CHANGE"
        [Required]
        [StringLength(50)]
        public string PerformedBy { get; set; } // Username or system principal
        [Column(TypeName = "datetime2")]
        public DateTime PerformedDate { get; set; }
        [StringLength(100)]
        public string CorrelationId { get; set; } // Links to Ticket 12!
        [StringLength(1000)]
        public string Details { get; set; } // Description of action or fields changed
    }
}