namespace HRConnect.Api.DTOs.Payroll.Earning
{
  public class EmployeePayrollEarningUpdateDto
  {
    public required string EmployeeId { get; set; }
    public required string PayrollEarningId { get; set; }
    public int? OverTimeHoursWorked { get; set; }
    public decimal? Amount { get; set; }
  }
}
