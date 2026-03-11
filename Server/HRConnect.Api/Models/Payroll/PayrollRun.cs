namespace HRConnect.Api.Models.Payroll
{
  using System.Text.Json.Serialization;

  //using HRConnect.Api.Models.PayrollContribution;
  //using HRConnect.Api.Models.Pension;

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
    //This is supposed to hold a colletion of 'PayrollRecord' types
    // like Pension and Medical Aid Contributions/Deductions
    public ICollection<PayrollRecord> Records { get; set; } = [];
    //public ICollection<EmployeePensionEnrollment> EmployeePensionEnrollment { get; set; } = [];
    //public ICollection<PensionDeduction> PensionDeduction { get; set; } = [];
  }
}