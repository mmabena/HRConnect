
namespace HRConnect.Api.DTOs.PayrollDeduction
{
  public class PayrollDeductionDto
  {
    public string EmployeeId { get; set; } = string.Empty;
    public decimal MonthlySalary { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public decimal EmployerSdlContribution { get; set; }
    public decimal UifEmployeeAmount { get; set; }
    public decimal UifEmployerAmount { get; set; }
  }
}