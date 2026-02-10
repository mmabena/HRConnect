using System.Security;

namespace HRConnect.Api.DTOs.PayrollDeductions
{
  public class PayrollDeductionsDto
  {
    public int EmployeeId { get; set; }
    public decimal MonthlySalary { get; set; }
    public int IdNumber { get; set; }
    public int PassportNumber { get; set; }
    public decimal SdlAmount { get; set; }
    public decimal UifEmployeeAmount { get; set; }
    public decimal UifEmployerAmount { get; set; }
  }
}