namespace HRConnect.Api.DTOs.Payroll.Deduction
{
  using HRConnect.Api.Models.PayrollDeduction;

  public class DeductionAddDto
  {
    public string CompanyId { get; set; } = string.Empty;
    public int? TaxCode { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
    public string LongDescription { get; set; } = string.Empty;
    public string DeductionType { get; set; } = string.Empty;
    public DeductionInputType InputType { get; set; }
    public decimal? MinimumValue { get; set; }
    public decimal? MaximumValue { get; set; }
    public bool EmployerContributed { get; set; }
  }
}
