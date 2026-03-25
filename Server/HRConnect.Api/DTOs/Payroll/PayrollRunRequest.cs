namespace HRConnect.Api.DTOs.Payroll
{
  public class PayrollRunRequestDto

  {
    public int PayrollRunNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
  }
}