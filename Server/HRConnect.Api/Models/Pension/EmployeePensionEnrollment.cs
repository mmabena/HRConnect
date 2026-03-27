namespace HRConnect.Api.Models.Pension
{
  using System.ComponentModel.DataAnnotations.Schema;

  public class EmployeePensionEnrollment
  {
    public int EmployeePensionEnrollmentId { get; set; }
    public int PensionOptionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VoluntaryContribution { get; set; } = decimal.Zero;
    public bool? IsVoluntaryContributionPermament { get; set; }
    public int PayrollRunId { get; set; }
    public bool IsLocked { get; set; }

    public PensionOption PensionOption { get; set; }
    public Employee Employee { get; set; }
  }
}
