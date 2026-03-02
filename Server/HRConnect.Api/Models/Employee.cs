namespace HRConnect.Api.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public enum Gender
    {
        Male,
        Female
    }
    public enum Title
    {
        Mr,
        Mrs,
        Ms,
        Dr,
        Prof
    }
    public enum Branch
    {
        Johannesburg, 
        CapeTown,
        UK
    }
    public enum EmploymentStatus
    {
        Permanent,
        FixedTerm,
        Contract
    }
    public class Employee
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        [Required]
        public Title Title { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Surname { get; set; } = string.Empty;
        [StringLength(13)]
        public string IdNumber { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        [Required]
        public Gender Gender { get; set; }
        [Required]
        [StringLength(10)]
        public string ContactNumber { get; set; } = string.Empty;
        [Required]
        [StringLength(10)]
        public string TaxNumber { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PhysicalAddress { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string ZipCode { get; set; } = string.Empty;
        public bool HasDisability { get; set; }
        public string? DisabilityDescription { get; set; }
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public Branch Branch { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlySalary { get; set; }
        [Required]
        public int  PositionId { get; set; }
        public Position? Position { get; set; }
        [Required]
        public EmploymentStatus EmploymentStatus { get; set; }
        public string? CareerManagerID { get; set; }
        [ForeignKey(nameof(CareerManagerID))]
        public Employee? CareerManager { get; set; }
        [Required]
        public string ProfileImage { get; set; } = string.Empty;
        // ProfileImage
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Allow reverse navigation
        public ICollection<Employee>? Subordinates { get; set; }
    }
}