namespace HRConnect.Api.DTOs.CompanyContribution
{
  public class CreateCompanyContributionDto
  {
    public string Code { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string LongDescription { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
  }
}