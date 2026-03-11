namespace HRConnect.Api.DTOs.Payroll
{
  using HRConnect.Api.Models.Payroll;

  public class PayrollPeriodDto
  {
    public Guid PayrollPeriodId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public bool IsLocked { get; set; }
    public ICollection<PayrollRun>? Runs { get; set; }
  }
}