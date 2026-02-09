namespace HRConnect.Api.Repository.Models
{
  /// <summary>
  /// Internal query result: flattened join of base medical policy option category + medical option
  /// Used ONLY inside the repository layer - never exposed to controllers/clients
  /// </summary>
  
  internal record MedicalOptionFlatRow
  (
    // ALL fields from base MedicalPolicyOptionCategory (the parent, like "Policy A")
    int MedicalPolicyOptionCategoryId,
    string MedicalPolicyOptionCategoryName,
    decimal? MonthlyRiskContributionPrincipal,
    decimal MonthlyRiskContributionAdult,
    decimal MonthlyRiskContributionChild,
    decimal? MonthlyRiskContributionChild2,
    decimal? MonthlyMsaContributionPrincipal,
    decimal? MonthlyMsaContributionAdult,
    decimal? MonthlyMsaContributionChild,
    decimal? TotalMonthlyContributionsPrincipal,
    decimal TotalMonthlyContributionsAdult,
    decimal TotalMonthlyContributionsChild,
    decimal? TotalMonthlyContributionsChild2,
    // ALL fields from PolicyOption
    int MedicalOptionId,
    string MedicalOptionName,
    int MedicalOptionCategoryId,
    decimal? SalaryBracketMin,
    decimal? SalaryBracketMax
  );
}
