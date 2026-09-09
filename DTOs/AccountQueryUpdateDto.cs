using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class AccountQueryUpdateDto
    {
        [Required(ErrorMessage = "Status is required")]
        [StringLength(20)]
        public string Status { get; set; } // "IN_PROGRESS", "COMPLETED", "REJECTED"
        [StringLength(500)]
        public string ResolutionNotes { get; set; } // Findings or resolution details
        [StringLength(500)]
        public string Purpose { get; set; } // Additional remarks
    }
}