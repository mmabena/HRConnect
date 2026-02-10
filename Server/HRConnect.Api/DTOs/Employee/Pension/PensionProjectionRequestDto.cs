namespace HRConnect.Api.DTOs.Employee.Pension
{

  public class PensionProjectionRequestDto
  {
    public int SelectedPensionPercentage { get; set; }
    public DateTime DOB { get; set; }
    public required string EmploymentEmploymentStatus { get; set; }
    public decimal Salary { get; set; }
    public decimal VoluntaryContribution { get; set; }
  }
}