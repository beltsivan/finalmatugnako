using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Organization { get; set;}

        [Required]
        [Display(Name = "Activity Title")]
        public string ActTitle { get; set; }

        [Required]
        public string Venue { get; set; }   

        [Display(Name = "Date Needed")]
        public DateTime DateNeeded { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Time From")]
        public DateTime TimeFrom { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Time To")]
        public DateTime TimeTo { get; set; }

        [Required]
        public string Participants { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public string Speaker { get; set; }

        [Required]
        [Display(Name = "Purpose / Objective")]
        public string PurposeObjective { get; set; }

        [Display(Name = "Equipment & Other Facilities Needed")]
        public string EquipmentFacilities { get; set; }

        [Required]
        [Display(Name = "Nature of Activity")]
        public string NatureOfActivity { get; set; }

        [Required]
        [Display(Name = "Source of Funds")]
        public string SourceOfFunds { get; set; }
        
    // Status tracking (nullable so existing records are preserved)
    // Marked [NotMapped] for now because the database schema may not yet include these columns.
    // This prevents EF from querying non-existent columns and avoids the SqlException.
    // TODO: remove [NotMapped] and add an EF migration to persist these fields when ready.
    [NotMapped]
    public bool? Approved { get; set; }

    [NotMapped]
    public string? Approver { get; set; }

    [NotMapped]
    public DateTime? ApprovalDate { get; set; }
    }
}
