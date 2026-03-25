namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction
{
  public class UpdateMedicalAidDeductionResponseDto
  {
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public string EmployeeId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Branch { get; set; }
    public decimal Salary { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    // This is the Medical Start Date
    public DateTime EffectiveDate { get; set; } 
    // this is the end date of the medical aid (caters for the event when the member changes plans or terminates their medical aid)
    public DateTime? TerminationDate { get; set; }
    public int MedicalOptionId { get; set; }
    public string OptionName { get; set; }
    public int MedicalCategoryId { get; set; }
    public string OptionCategoryName { get; set; }
    // Number of Deps
    public int PrincipalCount { get; set; }
    public int AdultCount { get; set; }
    public int ChildrenCount { get; set; }
    
    public decimal PrincipalPremium { get; set; }
    public decimal? SpousePremium { get; set; }
    public decimal? ChildPremium { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string? TerminationReason { get; set; } = string.Empty;
  }
}