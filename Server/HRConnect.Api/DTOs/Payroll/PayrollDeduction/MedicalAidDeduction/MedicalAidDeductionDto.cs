namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction
{
  public class MedicalAidDeductionDto
  {
    public int MedicalAidDeductionId { get; set; }
    //FK
    public int PayrollRunId { get; set; }
    //FK
    public string EmployeeId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Branch { get; set; }
    public decimal Salary { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    public DateTime EffectiveDate { get; set; } // This is the Medical Start Date
    //FK
    public int MedicalOptionId { get; set; }
    //FK
    public int MedicalCategoryId { get; set; }
    public int PrincipalCount { get; set; }
    public int AdultCount { get; set; }
    public int ChildrenCount { get; set; }
    public decimal PrincipalPremium { get; set; }
    public decimal? SpousePremium { get; set; }
    public decimal? ChildPremium { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // setting the default date to use UTC
    public bool IsActive { get; set; }
  }
}