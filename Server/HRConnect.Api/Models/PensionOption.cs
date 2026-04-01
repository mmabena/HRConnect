namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;

  public class PensionOption
  {
    public int PensionOptionId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ContributionPercentage { get; set; }

    public ICollection<Employee> Employee { get; set; } = [];
    public ICollection<EmployeePensionEnrollment> EmployeePensionEnrollment { get; set; } = [];
    public ICollection<PensionDeduction> PensionDeduction { get; set; } = [];
  }
}
