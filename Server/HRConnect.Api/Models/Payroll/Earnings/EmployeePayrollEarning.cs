namespace HRConnect.Api.Models.Payroll.Earnings
{
  using System.ComponentModel.DataAnnotations.Schema;

  public class EmployeePayrollEarning
  {
    public int EmployeePayrollEarningId { get; set; }
    public required string EmployeeId { get; set; }
    public required string PayrollEarningId { get; set; }
    public bool Taxable { get; set; }
    public int TaxCode { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    public int PayrollRunId { get; set; }
    public bool IsLocked { get; set; }

    public Employee? Employee { get; set; }
    public PayrollEarning? PayrollEarning { get; set; }
  }
}
