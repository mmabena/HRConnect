namespace HRConnect.Api.DTOs.Employee.Pension
{
  public class EmployeePensionEnrollmentUpdateDto
  {
    public int? PensionOptionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public decimal? VoluntaryContribution { get; set; }
    public bool? IsVoluntaryContributionPermament { get; set; }
  }
}
