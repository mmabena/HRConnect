namespace HRConnect.Api.Models.PayrollDeduction
{
  using HRConnect.Api.Models.Payroll;
  public class PensionDeduction : PayrollRecord
  {
    // public int PensionDeductionID { get; set; }
    public int PeriodId { get; set; }
    // public int PayrollRunId { get; set; }
    // public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateJoinedCompany { get; set; }
    public string IDNumber { get; set; } = string.Empty;
    public string? Passport { get; set; }
    public string TaxNumber { get; set; } = string.Empty;
    public decimal PensionableSalary { get; set; }
    public decimal PendsionCategoryPercentage { get; set; }
    public decimal PensionContribution { get; set; }
    public decimal VoluntaryContribution { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string PhyscialAddress { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
  }
}

