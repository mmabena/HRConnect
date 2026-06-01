namespace HRConnect.Api.DTOs.Payroll.Deduction
{
  using HRConnect.Api.Models.PayrollDeduction;

  public class DeductionUpdateDto
  {
    public required string DeductionId { get; set; }
    public string? CompanyId { get; set; }
    public int? TaxCode { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? DeductionType { get; set; }
    public DeductionInputType? InputType { get; set; }
    public decimal? MinimumValue { get; set; }
    public decimal? MaximumValue { get; set; }
    public bool? EmployerContributed { get; set; }
    public bool? Status { get; set; }
  }
}
