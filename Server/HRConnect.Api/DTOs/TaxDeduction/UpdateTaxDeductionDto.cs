namespace HRConnect.Api.DTOs
{
  public class UpdateTaxDeductionDto
  {
    public int Id { get; set; }
    public int TaxYear { get; set; }
    public decimal Remuneration { get; set; }
    public decimal AnnualEquivalent { get; set; }
    public decimal TaxUnder65 { get; set; }
    public decimal Tax65To74 { get; set; }
    public decimal TaxOver75 { get; set; }
  }

  public class TaxTableUploadRequest
  {
    public IFormFile File { get; set; } = null!;
    public int TaxYear { get; set; }
  }
}