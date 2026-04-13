namespace HRConnect.Api.DTOs.CompanyContribution
{
    using HRConnect.Api.Models;
    public class CompanyContributionDto
    {
        public int CompanyContributionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public bool IsActive { get; set; }
    }
}
