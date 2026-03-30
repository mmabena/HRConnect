namespace HRConnect.Api.Models.Payroll
{
  using System.ComponentModel.DataAnnotations;

  public class PayrollRecord
  {
    [Key]
    public int PayrollRecordId { get; set; }
    public int PayrollRunId { get; set; }
    public PayrollRun? PayrollRun { get; set; }
    // Every Record type should be able to be locked
    public bool IsLocked { get; set; }
  }
}
