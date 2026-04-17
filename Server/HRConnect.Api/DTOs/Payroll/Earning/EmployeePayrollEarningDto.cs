namespace HRConnect.Api.DTOs.Payroll.Earning
{
  public class EmployeePayrollEarningDto
  {
    public int EmployeePayrollEarningId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string PayrollEarningId { get; set; } = string.Empty;
    public bool Taxable { get; set; }
    public int TaxCode { get; set; }
    public decimal Amount { get; set; }
    public int PayrollRunId { get; set; }
    public bool IsLocked { get; set; }
  }
}
