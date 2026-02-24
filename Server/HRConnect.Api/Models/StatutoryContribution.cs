namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;
  public class StatutoryContribution
  {
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal MonthlySalary { get; set; }
    [Precision(7, 2)]
    [Range(0, (double)StatutoryContributionConstants.UIFCap, ErrorMessage = "UIF cannot be greater than R17,721.00")]
    public decimal UifEmployeeAmount { get; set; }
    [Precision(7, 2)]
    public decimal UifEmployerAmount { get; set; }
    [Precision(18, 2)]
    public decimal EmployerSdlContribution { get; set; }
    public DateTime DeductedAt { get; set; } = DateTime.UtcNow;
    public DateTime Month { get; set; }
  }
}