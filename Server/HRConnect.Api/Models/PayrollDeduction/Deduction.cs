namespace HRConnect.Api.Models.PayrollDeduction
{
  public enum DeductionInputType
  {
    Amount = 1,
    Percentage = 2
  }

  public class Deduction
  {
    public required string DeductionId { get; set; }
    public required string CompanyId { get; set; }
    public int? TaxCode { get; set; }
    public required string ShortDescription { get; set; }
    public required string LongDescription { get; set; }
    public required string DeductionType { get; set; }
    public DeductionInputType InputType { get; set; }
    public decimal? MinimumValue { get; set; }
    public decimal? MaximumValue { get; set; }
    public bool EmployerContributed { get; set; }
    public bool Status { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<EmployeeDeduction> EmployeeDeduction { get; set; } = [];
  }
}
