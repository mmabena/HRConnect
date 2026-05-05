namespace HRConnect.Api.DTOs.Payroll.Pension
{
  public class PensionDeductionUpdateDto
  {
<<<<<<< HEAD
    public string EmployeeId { get; set; } = string.Empty;
=======
    public required string EmployeeId { get; set; }
>>>>>>> main-v0.2
    public int? PensionOptionId { get; set; }
    public decimal? VoluntaryContribution { get; set; }
    public bool? IsActive { get; set; }
  }
}
