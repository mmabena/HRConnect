namespace HRConnect.Api.Mappers
{
  using DTOs.MedicalOption;
  using Repository.Models;

  public static class MedicalOptionMapper
  {
    public static MedicalOptionVariantFullDto ToMedicalOptionVariantFullDto(
      this MedicalOptionFlatRow r)
      => new MedicalOptionVariantFullDto
      {
        BaseMedicalPolicyOptionCategoryId = r.BaseMedicalPolicyOptionCategoryId,
        BaseMedicalOptionParentCategoryId = r.BaseMedicalOptionParentCategoryId,
        BaseMedicalPolicyOptionCategoryName = r.BaseMedicalPolicyOptionCategoryName,
        BaseMonthlyRiskContributionPrincipal = r.BaseMonthlyRiskContributionPrincipal,
        BaseMonthlyRiskContributionAdult = r.BaseMonthlyRiskContributionAdult,
        BaseMonthlyRiskContributionChild = r.BaseMonthlyRiskContributionChild,
        BaseMonthlyRiskContributionChild2 = r.BaseMonthlyRiskContributionChild2,
        BaseMonthlyMsaContributionPrincipal = r.BaseMonthlyMsaContributionPrincipal,
        BaseMonthlyMsaContributionAdult = r.BaseMonthlyMsaContributionAdult,
        BaseMonthlyMsaContributionChild = r.BaseMonthlyMsaContributionChild,
        BaseTotalMonthlyContributionsPrincipal = r.BaseTotalMonthlyContributionsPrincipal,
        BaseTotalMonthlyContributionsAdult = r.BaseTotalMonthlyContributionsAdult,
        BaseTotalMonthlyContributionsChild = r.BaseTotalMonthlyContributionsChild,
        BaseTotalMonthlyContributionsChild2 = r.BaseTotalMonthlyContributionsChild2,
        BaseMedicalOptionId = r.BaseMedicalOptionId,
        BaseMedicalOptionName = r.BaseMedicalOptionName,
        BaseMedicalOptionCategoryId = r.BaseMedicalOptionCategoryId,
        BaseSalaryBracketMin = r.BaseSalaryBracketMin,
        BaseSalaryBracketMax = r.BaseSalaryBracketMax
      };

  /*  public static List<MedicalOptionCategoryGroupDto> ToMedicalOptionCategoryGroupDto(
      this IEnumerable<MedicalOptionFlatRow> rows)
      => rows
        .GroupBy(r => r.BaseMedicalOptionParentCategoryId)
        .Select(g => new MedicalOptionCategoryGroupDto()
        {
          MedicalOptionGroupName = rows.Select(r => r.BaseMedicalOptionParentCategoryId == r.BaseMedicalPolicyOptionCategoryId).ToLookup()),
          Options = g
            .OrderBy(r => r.BaseSalaryBracketMin) // optional sort by salary
            .Select(r => r.ToMedicalOptionVariantFullDto())
            .ToList()
        })
        .ToList();
    */
    
  } 
}