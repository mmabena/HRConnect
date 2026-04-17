namespace HRConnect.Api.DTOs.Payroll.Earning
{
  public class EmployeePayrollEarningAddDto
  {
    public required string EmployeeId { get; set; }
    public required string PayrollEarningId { get; set; }
    public int TaxCode { get; set; }
    public int? OverTimeHoursWorked { get; set; }
    public decimal? Amount { get; set; }
  }
}
