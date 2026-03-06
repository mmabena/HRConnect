namespace HRConnect.Api.Models
{
  using HRConnect.Api.Models.PayrollContribution;
  using HRConnect.Api.Models.Pension;

  public class PensionOption
  {
    public int PensionOptionId { get; set; }
    public decimal ContributionPercentage { get; set; }

    public ICollection<Employee> Employee { get; set; } = [];
    public ICollection<EmployeePensionEnrollment> EmployeePensionEnrollment { get; set; } = [];
    public ICollection<PensionDeduction> PensionDeduction { get; set; } = [];
  }
}
