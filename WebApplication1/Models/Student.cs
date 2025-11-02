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
        public string? Firstname { get; set; }
        public string? Course { get; set; }
        public string? Email { get; set; }
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }=DateTime.Now;
       
    }
   
}
