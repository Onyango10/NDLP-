using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class AccountQueryResponseDto
    {
        public Guid Id { get; set; }
        public string RequestReference { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerId { get; set; }
        public string QueryType { get; set; }
        public string Status { get; set; } // "PENDING", "COMPLETED", "REJECTED"
        public string Purpose { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; }
        public string ResolutionNotes { get; set; }
    }
}