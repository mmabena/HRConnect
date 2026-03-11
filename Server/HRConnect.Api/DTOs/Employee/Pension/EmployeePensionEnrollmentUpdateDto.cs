namespace HRConnect.Api.DTOs.Employee.Pension
{
  public class EmployeePensionEnrollmentUpdateDto
  {
    public int? PensionOptionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateOnly? EffectiveDate { get; set; }
    public int? PayrollRunId { get; set; }
  }
}
