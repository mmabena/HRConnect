namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;

  //Take into heavy consideration on how to design the payroll record
  public class PayrollRecord
  {
    [Key]
    public int PayrollRecordId { get; set; }
    public int PayrollRunId { get; set; }
    public PayrollRun? PayrollRun { get; set; }
    public bool IsLocked { get; set; }
    [Precision(18, 2)]
    public decimal Deduction { get; set; }
    [Precision(18, 2)]
    public decimal MonthlySalary { get; set; }
  }
}