
namespace HRConnect.Api.Models.Payroll
{
  using System.Text.Json.Serialization;

  public abstract class PayrollRecord
  {
    // public int PayrollRecordId { get; set; }
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    [JsonIgnore]
    public PayrollRun? PayrollRun { get; set; }
    public bool IsLocked { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
  }
}
