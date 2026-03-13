namespace HRConnect.Api.DTOs.Employee.Pension
{
  public class EmployeePensionEnrollmentDto
  {
    public int PensionOptionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public int PayrollRunId { get; set; }
    public string? WarningMessage { get; set; }
  }
}
