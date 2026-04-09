namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction
{
  public class MedicalAidDeductionDto
  {
    public int MedicalAidDeductionId { get; set; }
    public int PayrollRunId { get; set; }
    public string EmployeeId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Branch { get; set; }
    public decimal Salary { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public int MedicalOptionId { get; set; }
    public string MedicalOptionName { get; set; }
    public int MedicalCategoryId { get; set; }
    public string OptionCategoryName { get; set; }
    public int PrincipalCount { get; set; }
    public int AdultCount { get; set; }
    public int ChildrenCount { get; set; }
    public decimal PrincipalPremium { get; set; } // total
    public decimal? SpousePremium { get; set; }
    public decimal? ChildPremium { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // setting the default date to use UTC
    public bool IsActive { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string? TerminationReason { get; set; }
    
  }
}