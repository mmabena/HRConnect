namespace HRConnect.Api.Models.Payroll
{
  using System.Text.Json.Serialization;
  using HRConnect.Api.Models.Pension;

  public class PayrollRun
  {
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public int PeriodId { get; set; }
    [JsonIgnore]
    public PayrollPeriod? Period { get; set; }
    public DateTime PeriodDate { get; set; }
    public bool IsFinalised { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? FinalisedDate { get; set; }
    public ICollection<PayrollRecord> Records { get; set; } = new List<PayrollRecord>();
    //public ICollection<EmployeePensionEnrollment> EmployeePensionEnrollment { get; set; } = [];
  }
}