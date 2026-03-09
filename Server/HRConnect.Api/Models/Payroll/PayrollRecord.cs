using Humanizer;

namespace HRConnect.Api.Models.Payroll
{
  using System.ComponentModel.DataAnnotations.Schema;

  // [NotMapped]
  public abstract class PayrollRecord
  {
    // public int PayrollRecordId { get; set; }
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public PayrollRun? PayrollRun { get; set; }
    // Every Record type should be able to be locked
    public bool IsLocked { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
  }
}
