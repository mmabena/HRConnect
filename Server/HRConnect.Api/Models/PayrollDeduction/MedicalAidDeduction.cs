namespace HRConnect.Api.Models.PayrollDeduction
{
  using System.ComponentModel.DataAnnotations.Schema;
  using Payroll;

  public class MedicalAidDeduction : PayrollRecord
  {
    //[Key]
    [Column("MedicalAidDeductionId")]
    public int MedicalAidDeductionId { get; set; }
    //FK
    //[ForeignKey(nameof(PayrollRun))]
    //public int PayrollRunNumber { get; set; }
    //FK
    //[ForeignKey(nameof(Employee))]
    //public string EmployeeId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Branch { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal Salary { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    public DateTime EffectiveDate { get; set; } // This is the Medical Start Date
    //FK
    [ForeignKey(nameof(MedicalOption))]
    public int MedicalOptionId { get; set; }
    public string OptionName { get; set; }
    //FK
    [ForeignKey(nameof(MedicalOptionCategory))]
    public int MedicalCategoryId { get; set; }
    public string OptionCategoryName { get; set; }
    // Number of Deps
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
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // setting the default date to use UTC (Ask??)
    public bool IsActive { get; set; }

    public DateTime UpdatedDate { get; set; }

    public MedicalOption MedicalOption { get; set; }
    //public Employee Employee { get; set; }
    //public PayrollRun PayrollRun { get; set; }
    public MedicalOptionCategory MedicalOptionCategory { get; set; }
  }
}
