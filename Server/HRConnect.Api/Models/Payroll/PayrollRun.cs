namespace HRConnect.Api.Models.Payroll
{
  public class PayrollRun
  {
    // public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public Guid PeriodId { get; set; }
    public PayrollPeriod? Period { get; set; }
    public DateTime PeriodDate { get; set; }
    public bool IsFinalised { get; set; }
    public DateTime? FinalisedDate { get; set; }
    //This is supposed to hold a colletion of 'PayrollRecord' types
    // like Pension and Medical Aid Contributions/Deductions
    public ICollection<PayrollRecord>? Records { get; set; }
  }
}