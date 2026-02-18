namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;


  public static class MedicalOptionMapper
  {
    //TODO: Implement Mapper Methods:
    // 1. A Mapper that will handle grouping options into their releavant categories

    /// <summary>
    /// Maps a grouping of Medical entities to a MedicalCategoryDto.
    /// This extension method transforms the IGrouping results from the repository
    /// into a client-friendly DTO structure that contains the category information
    /// and all associated medical options.
    /// </summary>
    /// <param name="group">
    /// The IGrouping result from the repository containing the MedicalOptions entities
    /// grouped by MedicalOptionCategoryId, Then the collection contains all MedicalOptions
    /// entities belonging to that category.
    /// </param>
    /// <returns>
    /// A MedicalOptionCategoryDto populated with:
    /// - MedicalOptioncategoryId: The MedicalOptionCategoryId from the repository
    /// - MedicalOptionCategoryName: The MedicalOptionCategoryName from the repository
    ///   (from the first option's navigational property)
    /// - MedicalOptions: A list of MedicalOptionDto objects representing all options in
    ///   this category
    /// </returns>
    /// <remarks>
    /// This method assumes that the MedicalOption entities have their MedicalOptionCategory
    /// navigation property loaded (typically vai .Include() in the repository query).
    /// If the category is not loaded, the MedicalCategoryName will default to an empty string.
    /// </remarks>
    /// <example>
    /// Usage in service:
    /// <code>
    /// var groupedOptions = await _medicalOptionRepository.GetGroupedMedicalOptionsAsync();
    /// var categoryDtos = groupedOptions.Select(group => group.
    ///       ToMedicalOptionCategoryDto()).ToList();
    /// </code>
    /// </example>
    public static MedicalOptionCategoryDto ToMedicalOptionCategoryDto(
      this IGrouping<int, MedicalOption> group)
    {
      var firstOption = group.FirstOrDefault();
      if (firstOption == null)
      {
        return new MedicalOptionCategoryDto
        {
          MedicalOptionCategoryId = group.Key,
          MedicalOptionCategoryName = "Uncategorized",
          MedicalOptions = new List<MedicalOptionDto>()
        };
      }

      return new MedicalOptionCategoryDto
      {
        MedicalOptionCategoryId = group.Key,
        MedicalOptionCategoryName =
          group.First().MedicalOptionCategory?.MedicalOptionCategoryName ?? "Uncategorized",
        MedicalOptions = group.Select(ToMedicalOptionDto).ToList()
      };
    }
    
    /// <summary>
    /// Maps a single MedicalOption entity to a MedicalOptionDto.
    /// This method transforms the domain entity into a data transfer object suitable for
    /// client consumption, excluding any sensitive or unnecessary properties.
    /// </summary>
    /// <param name="option">
    /// The MedicalOption entity to be mapped to DTO format.
    /// Should contain all relevant property values including contribution amounts,
    /// salary brackets, and identifying information.
    /// </param>
    /// <returns>
    /// A MedicalOptionDto populated with all relevant medical option details:
    /// - Identifying properties (ID, Name, MedicalOptionCategoryId)
    /// - Salary bracket information (Min/Max)
    /// - Monthly contribution amounts (Risk and MSA contributions (if applicable))
    /// - Total monthly contributions for different member types (Principal, Adult, Child)
    /// </returns>
    /// <remarks>
    /// This method performs a direct propert mapping with no transforming logic.
    /// All decimals properties maintain their original precision and nullability.
    /// String properties are mapped directly with default empty string handling.
    /// </remarks>
    /// <example>
    /// Usage within ToMedicalOptionCategoryDto:
    /// <code>
    /// Medicaloption = group.Select(option => option.ToMedicalOptionDto()).ToList();
    /// </code>
    /// </example>
    public static MedicalOptionDto ToMedicalOptionDto(this MedicalOption option)
    {
      return new MedicalOptionDto
      {
        MedicalOptionId = option.MedicalOptionId,
        MedicalOptionName = option.MedicalOptionName,
        MedicalOptionCategoryId = option.MedicalOptionCategoryId,
        SalaryBracketMin = option.SalaryBracketMin,
        SalaryBracketMax = option.SalaryBracketMax,
        MonthlyRiskContributionPrincipal = option.MonthlyRiskContributionPrincipal,
        MonthlyRiskContributionAdult = option.MonthlyRiskContributionAdult,
        MonthlyRiskContributionChild = option.MonthlyRiskContributionChild,
        MonthlyRiskContributionChild2 = option.MonthlyRiskContributionChild2,
        MonthlyMsaContributionPrincipal = option.MonthlyMsaContributionPrincipal,
        MonthlyMsaContributionAdult = option.MonthlyMsaContributionAdult,
        MonthlyMsaContributionChild = option.MonthlyMsaContributionChild,
        TotalMonthlyContributionsPrincipal = option.TotalMonthlyContributionsPrincipal,
        TotalMonthlyContributionsAdult = option.TotalMonthlyContributionsAdult,
        TotalMonthlyContributionsChild = option.TotalMonthlyContributionsChild,
        TotalMonthlyContributionsChild2 = option.TotalMonthlyContributionsChild2
      };
    }

    public static UpdateMedicalOptionVariantsDto ToUpdateMedicalOptionVariantDto(
      this MedicalOption bulkUpdate)
    {
      return new UpdateMedicalOptionVariantsDto
      {
        MedicalOptionId = bulkUpdate.MedicalOptionId,
        SalaryBracketMin = bulkUpdate.SalaryBracketMin,
        SalaryBracketMax = bulkUpdate.SalaryBracketMax,
        MonthlyRiskContributionPrincipal = bulkUpdate.MonthlyRiskContributionPrincipal,
        MonthlyRiskContributionAdult = bulkUpdate.MonthlyRiskContributionAdult,
        MonthlyRiskContributionChild = bulkUpdate.MonthlyRiskContributionChild,
        MonthlyRiskContributionChild2 = bulkUpdate.MonthlyRiskContributionChild2,
        MonthlyMsaContributionPrincipal = bulkUpdate.MonthlyMsaContributionPrincipal,
        MonthlyMsaContributionAdult = bulkUpdate.MonthlyMsaContributionAdult,
        MonthlyMsaContributionChild = bulkUpdate.MonthlyMsaContributionChild,
        TotalMonthlyContributionsPrincipal = bulkUpdate.TotalMonthlyContributionsPrincipal,
        TotalMonthlyContributionsAdult = bulkUpdate.TotalMonthlyContributionsAdult,
        TotalMonthlyContributionsChild = bulkUpdate.TotalMonthlyContributionsChild,
        TotalMonthlyContributionsChild2 = bulkUpdate.TotalMonthlyContributionsChild2
      };
    }
  }
}