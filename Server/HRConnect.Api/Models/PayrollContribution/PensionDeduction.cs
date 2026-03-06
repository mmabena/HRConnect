namespace HRConnect.Api.Models.PayrollContribution
{
  using HRConnect.Api.Models.Payroll;

  public class PensionDeduction
  {
    public int PensionDeductionId { get; set; }
    public int EmployeeID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateJoinedCompany { get; set; }
    public string IDNumber { get; set; } = string.Empty;
    public string? Passport { get; set; }
    public string TaxNumber { get; set; } = string.Empty;
    public decimal PensionableSalary { get; set; }
    public int PensionOptionId { get; set; }
    public decimal PendsionCategoryPercentage { get; set; }
    public decimal PensionContribution { get; set; }
    public decimal VoluntaryContribution { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string PhyscialAddress { get; set; } = string.Empty;
    public int PayrollRunId { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }

    public Employee Employee { get; set; }
    public PensionOption PensionOption { get; set; }
    public PayrollRun PayrollRun { get; set; }
  }
}

