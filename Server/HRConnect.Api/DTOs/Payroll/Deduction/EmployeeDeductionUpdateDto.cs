namespace HRConnect.Api.DTOs.Payroll.Deduction
{
  public class EmployeeDeductionUpdateDto
  {
    public required string EmployeeId { get; set; }
    public required string DeductionId { get; set; }
    public decimal AmountOrPercentage { get; set; }
  }
}
