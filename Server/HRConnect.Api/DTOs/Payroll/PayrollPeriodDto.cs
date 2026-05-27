namespace HRConnect.Api.DTOs.Payroll
{
  using Models.Payroll;

  public class PayrollPeriodDto
  {
    public int PayrollPeriodId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public bool IsLocked { get; set; }
    public ICollection<PayrollRun> Runs { get; set; } = new List<PayrollRun>();
  }
}