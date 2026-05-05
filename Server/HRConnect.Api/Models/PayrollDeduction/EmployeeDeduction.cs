namespace HRConnect.Api.Models.PayrollDeduction
{
  public class EmployeeDeduction
  {
    public int EmployeeDeductionId { get; set; }
    public required string EmployeeId { get; set; }
    public required string DeductionId { get; set; }
    public required string DeductionType { get; set; }
    public DeductionInputType DeductionInputType { get; set; }
    public decimal AmountOrPercentage { get; set; }
    public decimal CalculatedDeductionAmount { get; set; }
    public int PayrollRunId { get; set; }
    public bool IsLocked { get; set; }

    public Deduction Deduction { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
  }
}
