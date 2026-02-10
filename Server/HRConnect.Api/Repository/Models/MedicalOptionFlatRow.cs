namespace HRConnect.Api.Repository.Models
{
  /// <summary>
  /// Internal query result: flattened join of base medical policy option category + medical option
  /// Used ONLY inside the repository layer - never exposed to controllers/clients
  /// </summary>
  
  public record MedicalOptionFlatRow
  (
    // ALL fields from base MedicalPolicyOptionCategory (the parent, like "Policy A")
    int BaseMedicalPolicyOptionCategoryId,
    int? BaseMedicalOptionParentCategoryId,
    string BaseMedicalPolicyOptionCategoryName,
    decimal? BaseMonthlyRiskContributionPrincipal,
    decimal BaseMonthlyRiskContributionAdult,
    decimal BaseMonthlyRiskContributionChild,
    decimal? BaseMonthlyRiskContributionChild2,
    decimal? BaseMonthlyMsaContributionPrincipal,
    decimal? BaseMonthlyMsaContributionAdult,
    decimal? BaseMonthlyMsaContributionChild,
    decimal? BaseTotalMonthlyContributionsPrincipal,
    decimal BaseTotalMonthlyContributionsAdult,
    decimal BaseTotalMonthlyContributionsChild,
    decimal? BaseTotalMonthlyContributionsChild2,
    // ALL fields from PolicyOption
    int BaseMedicalOptionId,
    string BaseMedicalOptionName,
    int BaseMedicalOptionCategoryId,
    decimal? BaseSalaryBracketMin,
    decimal? BaseSalaryBracketMax
  );
}
