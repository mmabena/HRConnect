namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  public class PayrollRun
  {
    [Key]
    public int PayrollRunId { get; set; }
    public Guid PeriodId { get; set; }
    public PayrollPeriod? Period { get; set; }
    public DateTime PeriodDate { get; set; }
    public bool IsFinalised { get; set; }
    public DateTime FinalisedDate { get; set; }
    public ICollection<PayrollRecord>? Records { get; set; }
  }
}