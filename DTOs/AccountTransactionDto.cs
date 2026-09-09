using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class AccountTransactionDto
    {
        public string TransactionReference { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime ValueDate { get; set; }
        public string TransactionType { get; set; } // "DEBIT" / "CREDIT"
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal RunningBalance { get; set; }
        public string Description { get; set; }
        public string Channel { get; set; }
        public string Status { get; set; }
    }
}