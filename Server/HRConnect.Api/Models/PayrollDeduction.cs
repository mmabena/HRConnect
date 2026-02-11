namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;

  public static class DeductionConstants
  {
    public const decimal UIFCap = 17712m;
    public const decimal UIFEmployeeAmount = 0.01m;
    public const decimal UIFEmployerAmount = 0.01m;
    public const decimal SDLAmount = 0.01m;
  }

  public class PayrollDeduction
  {
    // public int PayrollDeductionId { get; set; }
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal MonthlySalary { get; set; }
    [Precision(7, 2)]
    [Range(0, (double)DeductionConstants.UIFCap, ErrorMessage = "UIF cannot be greater than R17,7210.00")]
    public decimal UifEmployeeAmount { get; set; }
    [Precision(7, 2)]
    public decimal UifEmployerAmount { get; set; }
    [Precision(18, 2)]
    public decimal SdlAmount { get; set; }
    public DateTime DeductedAt { get; set; } = DateTime.UtcNow;
  }
}