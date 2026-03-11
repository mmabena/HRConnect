namespace HRConnect.Api.Models.PayrollDeduction
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.Payroll;

  public class MedicalAidDeduction : PayrollRecord
  {
    // [Key]
    [Column("MedicalOptionId")]
    // public int MedicalAidDeductionId { get; set; }
    public int MedicalOptionId { get; set; }
    public int MedicalOptionCategoryId { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    public decimal? PrincipalPremium { get; set; }
    public decimal? SpousePremium { get; set; }
    public decimal? ChildPremium { get; set; }
    public decimal? TotalDependentsPremium { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime FinalisedDate { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsActive { get; set; }
  }

}
