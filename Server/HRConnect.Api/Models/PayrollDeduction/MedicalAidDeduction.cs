namespace HRConnect.Api.Models.PayrollDeduction
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.Payroll;

  public class MedicalAidDeduction : PayrollRecord
  {
    // [Key]
    public int MedicalAidDeductionId { get; set; }

    // public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Branch { get; set; }
    [Column("MedicalOptionId")]
    public int MedicalOptionId { get; set; }
    public int MedicalOptionCategoryId { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    public decimal? TotalDependentsPremium { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime FinalisedDate { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsActive { get; set; }
    public int PrincipalCount { get; set; }
    public int AdultCount { get; set; }
    public int ChildrenCount { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal PrincipalPremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? SpousePremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ChildPremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal TotalDeductionAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal Salary { get; set; }

    //FK
    [ForeignKey(nameof(MedicalOptionCategory))]
    public int MedicalCategoryId { get; set; }
    // Number of Deps

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // setting the default date to use UTC (Ask??)
    public MedicalOption MedicalOption { get; set; }
    //public Employee Employee { get; set; }
    //public PayrollRun PayrollRun { get; set; }
    public MedicalOptionCategory MedicalOptionCategory { get; set; }

  }

}
