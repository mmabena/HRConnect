namespace HRConnect.Api.DTOs.Payroll.Deduction
{
  using HRConnect.Api.Models.PayrollDeduction;

  public class EmployeeDeductionDto
  {
    public int EmployeeDeductionId { get; set; }
    public required string EmployeeId { get; set; }
    public required string DeductionId { get; set; }
    public required string DeductionType { get; set; }
    public DeductionInputType DeductionInputType { get; set; }
    public decimal AmountOrPercentage { get; set; }
    public decimal CalculatedDeductionAmount { get; set; }
    public int PayRunId { get; set; }
    public bool IsLocked { get; set; }
  }
}
