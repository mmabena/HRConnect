namespace HRConnect.Api.Models.Payroll
{
  //using System.ComponentModel.DataAnnotations;
  using System.Text.Json.Serialization;

  public class PayrollRecord
  {
    //[Key]
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    [JsonIgnore]
    public PayrollRun? PayrollRun { get; set; }
    public bool IsLocked { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
  }
}
