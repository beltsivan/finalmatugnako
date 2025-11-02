using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class LockerRequest
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Student ID Number")]
        public string? IdNumber { get; set; }

        [Display(Name = "Locker Number")]
        public string? LockerNumber { get; set; }

        public string? Semester { get; set; }

        [Display(Name = "Contact Number")]
        [Phone]
        public string? ContactNumber { get; set; }

        [Display(Name = "Registration File Path")]
        public string? RegistrationFilePath { get; set; }

        [Display(Name = "I accept the Terms of Service and Locker Usage Policy")]
        [Required]
        public bool TermsAccepted { get; set; }

        // Status tracking
        public bool? Approved { get; set; }
        public string? Approver { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModified { get; set; }
    }
}

