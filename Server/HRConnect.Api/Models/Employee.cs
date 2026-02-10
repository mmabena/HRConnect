namespace HRConnect.Api.Models
{
    
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading.Tasks;
    public class Employee
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Surname { get; set; } = string.Empty;
        [StringLength(13)]
        public string IdNumber { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        [Required]
        public string Gender { get; set; } = string.Empty;
        [Required]
        [StringLength(10)]
        public string ContactNumber { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PhysicalAddress { get; set; } = string.Empty;
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public string Branch { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlySalary { get; set; }
        [Required]
        public int  PositionId { get; set; }
        public Position? Position { get; set; }
        [Required]
        public string EmploymentStatus { get; set; } = string.Empty;
        [Required]
        public string CareerManager { get; set; } = string.Empty;
        [Required]
        public string EmpPicture { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        


    }
}