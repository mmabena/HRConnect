namespace HRConnect.Api.DTOs.Payroll.Pension
{
  public class PensionDeductionDto
  {
    public string EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateJoinedCompany { get; set; }
    public string IDNumber { get; set; } = string.Empty;
    public string? Passport { get; set; }
    public string TaxNumber { get; set; } = string.Empty;
    public decimal PensionableSalary { get; set; }
    public int PensionOptionId { get; set; }
    public decimal PendsionCategoryPercentage { get; set; }
    public decimal PensionContribution { get; set; }
    public decimal? VoluntaryContribution { get; set; }
    public decimal TotalContribution { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string PhyscialAddress { get; set; } = string.Empty;
    public int PayrollRunId { get; set; }
    public DateOnly? CreatedDate { get; set; }
    public bool IsActive { get; set; }
  }
}
