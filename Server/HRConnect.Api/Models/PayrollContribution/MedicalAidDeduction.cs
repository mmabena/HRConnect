namespace HRConnect.Api.Models.PayrollContribution
{
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class MedicalAidDeduction
  {
    [Key]
    public int MedicalAidDeductionId { get; set; }
    //FK
    [ForeignKey(nameof(PayrollRun))]
    public int PayrollRunId { get; set; }
    //FK
    [ForeignKey(nameof(Employee))]
    public int EmployeeId { get; set; }
    //FK
    [ForeignKey(nameof(MedicalOption))]
    public int MedicalOptionId { get; set; }
    //FK
    [ForeignKey(nameof(MedicalOptionCategory))]
    public int MedicalCategoryId { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal PrincipalPremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal? SpousePremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal? ChildPremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal TotalDependentPremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal TotalDeductionAmount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime FinalizedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // setting the default date to use UTC
    public bool IsActive { get; set; }

    public MedicalOption MedicalOption { get; set; }
    public Employee Employee { get; set; }
    public Payroll.PayrollRun PayrollRun { get; set; }
    public MedicalOptionCategory MedicalOptionCategory { get; set; }
  }  
}

