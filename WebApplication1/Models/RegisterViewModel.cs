using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class RegisterViewModel
    {
    [Required]
    [Display(Name = "School ID Number")]
    public string? SchoolIdNumber { get; set; }

    [Required]
    [Display(Name = "First Name")]
    public string? Firstname { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    public string? Lastname { get; set; }

        [Required]
        [Range(10, 150)]
        public int Age { get; set; }

    [Required]
    [MaxLength(50)]
    public string? Username { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

        [Display(Name = "Course")]
        public string? Course { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }
    }
}
