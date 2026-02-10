namespace HRConnect.Api.DTOs.MedicalOption
{
  public class MedicalOptionVariantFullDto
  {
    public int BaseMedicalPolicyOptionCategoryId { get; set; }
    public int? BaseMedicalOptionParentCategoryId { get; set; }
    public string BaseMedicalPolicyOptionCategoryName { get; set; } = string.Empty;
    public decimal? BaseMonthlyRiskContributionPrincipal { get; set; }
    public decimal BaseMonthlyRiskContributionAdult { get; set; }
    public decimal BaseMonthlyRiskContributionChild { get; set; }
    public decimal? BaseMonthlyRiskContributionChild2 { get; set; }
    public decimal? BaseMonthlyMsaContributionPrincipal { get; set; }
    public decimal? BaseMonthlyMsaContributionAdult { get; set; }
    public decimal? BaseMonthlyMsaContributionChild { get; set; }
    public decimal? BaseTotalMonthlyContributionsPrincipal { get; set; }
    public decimal BaseTotalMonthlyContributionsAdult { get; set; }
    public decimal BaseTotalMonthlyContributionsChild { get; set; }
    public decimal? BaseTotalMonthlyContributionsChild2 { get; set;}
    // Policy option
    public int BaseMedicalOptionId { get; set; }
    public string BaseMedicalOptionName { get; set; } = string.Empty;
    public int BaseMedicalOptionCategoryId { get; set; }
    public decimal? BaseSalaryBracketMin { get; set; }
    public decimal? BaseSalaryBracketMax { get; set; }
  }
}