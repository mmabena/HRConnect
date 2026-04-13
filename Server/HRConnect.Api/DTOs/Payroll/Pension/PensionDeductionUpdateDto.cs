namespace HRConnect.Api.DTOs.Payroll.Pension
{
  public class PensionDeductionUpdateDto
  {
    public string EmployeeId { get; set; }
    public int? PensionOptionId { get; set; }
    public decimal? VoluntaryContribution { get; set; }
    public bool? IsActive { get; set; }
  }
}
