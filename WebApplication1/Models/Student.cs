using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    [Required, MaxLength(25)]
    public string Lastname { get; set; } = "";

    [Required, MaxLength(25)]
    public string Firstname { get; set; } = "";

    [MaxLength(50)]
    public string? Course { get; set; }

    [MaxLength(100), DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Display(Name = "Date Created")]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    // New fields for registration/login
    [Display(Name = "School ID Number")]
    [MaxLength(50)]
    public string? SchoolIdNumber { get; set; }

    public int? Age { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = "";

    // Stored hashed password
    [MaxLength(512)]
    public string? PasswordHash { get; set; }
       
    }
   
}
