using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class AccountStatusUpdateDto
    {
        [Required(ErrorMessage = "Status is required")]
        [StringLength(20)]
        public string Status { get; set; } // "ACTIVE", "DORMANT", "FROZEN", "BLOCKED", "DEACTIVATED"
        [Required(ErrorMessage = "Reason for status change is mandatory for audit purposes")]
        [StringLength(250)]
        public string Reason { get; set; }
    }
}