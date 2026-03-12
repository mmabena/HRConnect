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
    [Key]
    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    public Title Title { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Surname { get; set; } = string.Empty;

    [StringLength(13)]
    public string? IdNumber { get; set; }   // made nullable

    public string? PassportNumber { get; set; }   // made nullable

    [Required]
    public Gender Gender { get; set; }

    public string? ContactNumber { get; set; }    // made nullable

    public string? TaxNumber { get; set; }        // made nullable

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhysicalAddress { get; set; }  // made nullable
    public string? City { get; set; }             // made nullable
    public string? ZipCode { get; set; }          // made nullable

    public bool HasDisability { get; set; }
    public string? DisabilityDescription { get; set; }

    public DateOnly? DateOfBirth { get; set; }    // made nullable

    public DateOnly? StartDate { get; set; }      // made nullable

    [Required]
    public Branch Branch { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlySalary { get; set; }   // made nullable

    public int? PositionId { get; set; }          // made nullable
    public Position? Position { get; set; }

    [Required]
    public EmploymentStatus EmploymentStatus { get; set; }

    public string? CareerManagerID { get; set; }
    [ForeignKey(nameof(CareerManagerID))]
    public Employee? CareerManager { get; set; }

    public string? ProfileImage { get; set; }     // made nullable

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public ICollection<PensionFund>? PensionFunds { get; set; }
    public int? PensionOptionId { get; set; }     // made nullable
    public PensionOption? PensionOption { get; set; }

    public ICollection<Employee>? Subordinates { get; set; }
  }
}
