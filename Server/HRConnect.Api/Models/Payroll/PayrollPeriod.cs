namespace HRConnect.Api.Models.Payroll
{
  using System.ComponentModel.DataAnnotations;
  public class PayrollPeriod
  {
    [Key]
    public int PayrollPeriodId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public bool IsLocked { get; set; }
    public ICollection<PayrollRun> Runs { get; set; } = [];
  }
}