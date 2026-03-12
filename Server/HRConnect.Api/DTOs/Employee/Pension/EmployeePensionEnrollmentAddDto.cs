namespace HRConnect.Api.DTOs.Employee.Pension
{
  public class EmployeePensionEnrollmentAddDto
  {
    public string EmployeeId { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
  }
}
