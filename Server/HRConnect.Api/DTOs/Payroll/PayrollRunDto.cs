namespace HRConnect.Api.DTOs.Payroll
{
  using HRConnect.Api.Models.Payroll;
  public class PayrollRunDto
  {
    public int PayrollRunId { get; set; }
    public int PeriodId { get; set; }
    public PayrollPeriod? Period { get; set; }
    public DateTime PeriodDate { get; set; }
    public bool IsFinalised { get; set; }
    public DateTime FinalisedDate { get; set; }
    public ICollection<PayrollRecord>? Records { get; set; }
  }
}