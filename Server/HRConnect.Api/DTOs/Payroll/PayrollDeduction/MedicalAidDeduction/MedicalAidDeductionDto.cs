namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction
{
  public class MedicalAidDeductionDto
  {
    public int MedicalAidDeductionId { get; set; }
    //FK
    public int PayrollRunId { get; set; }
    //FK
    public string EmployeeId { get; set; }
    //FK
    public int MedicalOptionId { get; set; }
    //FK
    public int MedicalCategoryId { get; set; }
    public decimal PrincipalPremium { get; set; }
    public decimal? SpousePremium { get; set; }
    public decimal? ChildPremium { get; set; }
    public decimal TotalDependentPremium { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime FinalizedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // setting the default date to use UTC
    public bool IsActive { get; set; }
  }
}