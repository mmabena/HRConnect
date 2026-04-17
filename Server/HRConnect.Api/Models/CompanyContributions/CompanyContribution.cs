
namespace HRConnect.Api.Models.CompanyContributions
{
  public class CompanyContribution
  {
    public int CompanyContributionId { get; set; }
    public string Code { get; set; } = string.Empty; // DEATHBEN or DISABILITY
    public string ShortDescription { get; set; } = string.Empty;
    public string LongDescription { get; set; } = string.Empty;
    public string TaxCode { get; set; } = "3801";
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; }
  }
}