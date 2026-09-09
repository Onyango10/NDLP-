using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NDLP_Project.Models.Entities
{
    [Table("tbl_CustomerAccounts")]
    public class CustomerAccount
    {
        [Key]
        public Guid Id { get; set; }


        [Required]
        [StringLengthAttribute(20)]
        [Index("IX_AccountNumber", IsUnique = true)]
        public string AccountNumber {  get; set; }

        [Required]
        [StringLength(150)]
        public String CustomerName { get; set; }

        [Required]
        [StringLength(30)]
        public String CustomerId {  get; set; }

        [Required]
        [StringLengthAttribute(30)]
        public String AccountType { get; set; }
        [Required]
        [StringLength(5)]
        public String Currency {  get; set; }

        [Column(TypeName = "decimal")]
        public decimal BookBalance { get; set; }

        [Column(TypeName = "decimal")]
        public decimal AvailableBalance { get; set; }
        [Column(TypeName = "decimal")]
        public decimal HoldAmount { get; set; }
        [Column(TypeName = "decimal")]
        public decimal OverdraftLimit { get; set; }
        [Required]
        [StringLength(20)]
        public string Status { get; set; }
        [StringLength(10)]
        public string BranchCode { get; set; }
        [StringLength(100)]
        public string BranchName { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        public string NationalIdNumber { get; set; }
        // Maker-Checker & Auditing Fields
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? ModifiedDate { get; set; }
        [StringLength(50)]
        public string ModifiedBy { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? ApprovedDate { get; set; }
        [StringLength(50)]
        public string ApprovedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }


    }
}