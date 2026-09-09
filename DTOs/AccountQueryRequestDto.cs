using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class AccountQueryRequestDto
    {
        [Required(ErrorMessage = "Account number is required")]
        [StringLength(20)]
        public string AccountNumber { get; set; }
        [Required(ErrorMessage = "Customer ID is required")]
        [StringLength(50)]
        public string CustomerId { get; set; }
        [Required(ErrorMessage = "Query type is required")]
        [StringLength(50)]
        public string QueryType { get; set; } // "BALANCE_INQUIRY", "STATEMENT_QUERY", "ACCOUNT_DETAILS"
        [StringLength(500)]
        public string Purpose { get; set; } // Reason for query
        // Optional unique client reference for duplicate prevention (Ticket 16)
        [StringLength(50)]
        public string RequestReference { get; set; }
    }
}