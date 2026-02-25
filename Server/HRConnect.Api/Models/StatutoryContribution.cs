namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Utils;
  public class StatutoryContribution
  {
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    [Precision(18, 4)]
    public decimal MonthlySalary { get; set; }
    [Precision(18, 4)]
    [Range(0, (double)StatutoryContributionConstants.UIFCap, ErrorMessage = "UIF cannot be greater than R354.24")]
    public decimal UifEmployeeAmount { get; set; }
    [Precision(18, 4)]
    public decimal UifEmployerAmount { get; set; }
    [Precision(18, 4)]
    public decimal EmployerSdlContribution { get; set; }
    public DateTime DeductedAt { get; set; } = DateTime.UtcNow;
    public DateOnly CurrentMonth { get; set; }
    //Navigation property
    public StatutoryContributionType? ContributionType { get; set; }
  }
}