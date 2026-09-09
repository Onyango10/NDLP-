using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class CustomerAccountDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        // Balances
        public decimal BookBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal HoldAmount { get; set; }
        public decimal OverdraftLimit { get; set; }
        public string Status { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        // Contact Information
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string NationalIdNumber { get; set; }
        // Account opening date
        public DateTime DateOpened { get; set; }
    }
}