namespace HRConnect.Api.Models.Pension
{
  using HRConnect.Api.Models.Payroll;

  public class EmployeePensionEnrollment
  {
    public int EmployeePensionEnrollmentId { get; set; }
    public int PensionOptionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public int PayrollRunId { get; set; }

    public PensionOption PensionOption { get; set; }
    public Employee Employee { get; set; }
    public PayrollRun? PayrollRun { get; set; }
  }
}
