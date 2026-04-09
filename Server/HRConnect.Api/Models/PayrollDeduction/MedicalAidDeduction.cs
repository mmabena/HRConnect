namespace HRConnect.Api.Models.PayrollDeduction
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.Payroll;

  public class MedicalAidDeduction : PayrollRecord
  {
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Branch { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal Salary { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    // This is the Medical Aid Start Date
    public DateTime EffectiveDate { get; set; } 
    // This is the end date of the medical aid (caters for the event when the member changes plans or terminates their medical aid)
    public DateTime? TerminationDate { get; set; }
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
    //Total Contribution + Breakdown
    [Column(TypeName = "decimal(15, 2)")]
    public decimal PrincipalPremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? SpousePremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ChildPremium { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal TotalDeductionAmount { get; set; }
    public DateTime CreatedDate { get; set; } 
    public bool IsActive { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string? TerminationReason { get; set; } = string.Empty;// Reason for termination (Moving to another premium?, moving to another medical aid provider, etc.)

    public MedicalOption MedicalOption { get; set; }
    public MedicalOptionCategory MedicalOptionCategory { get; set; }
  }
}