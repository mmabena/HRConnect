
namespace HRConnect.Api.DTOs.PayrollDeductions
{
  public class PayrollDeductionsDto
  {
    public string EmployeeId { get; set; } = string.Empty;
    public decimal MonthlySalary { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public decimal SdlAmount { get; set; }
    public decimal UifEmployeeAmount { get; set; }
    public decimal UifEmployerAmount { get; set; }
  }
}