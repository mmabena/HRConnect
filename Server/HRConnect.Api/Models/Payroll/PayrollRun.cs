namespace HRConnect.Api.Models.Payroll
{
  using System.Text.Json.Serialization;

  public class PayrollRun
  {
    // public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public int PayrollRunNumber { get; set; }
    public int PeriodId { get; set; }
    [JsonIgnore]
    public PayrollPeriod Period { get; set; } = null!;
    public DateTime PeriodDate { get; set; }
    public bool IsFinalised { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? FinalisedDate { get; set; }
    public ICollection<PayrollRecord> Records { get; set; } = new List<PayrollRecord>();
  }
}