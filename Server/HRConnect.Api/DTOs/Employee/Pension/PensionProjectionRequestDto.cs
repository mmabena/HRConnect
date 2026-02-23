namespace HRConnect.Api.DTOs.Employee.Pension
{
  public enum ContributionFrequency
  {
    NotSet = 0,
    OnceOff = 1,
    Permanent = 2
  }
  public class PensionProjectionRequestDto
  {
    public int SelectedPensionPercentage { get; set; }
    public DateTime DOB { get; set; }
    public required string EmploymentStatus { get; set; }
    public decimal Salary { get; set; }
    public decimal VoluntaryContribution { get; set; }
    public ContributionFrequency VoluntaryContributionFrequency { get; set; }
  }
}