namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;

  /// <summary>
  /// Provides extension methods for mapping between MedicalOption domain entities 
  /// and MedicalOption data transfer objects (DTOs).
  /// This mapper class facilitates the transformation of data layers while maintaining
  /// separation of concerns between domain models and API contracts.
  /// </summary>
  /// <remarks>
  /// All mapping methods are designed to be null-safe and handle edge cases gracefully.
  /// The mapper supports both single entity mapping and collection-based operations.
  /// Decimal precision and nullability are preserved throughout all transformations.
  /// </remarks>
  public static class MedicalOptionMapper
  {
    //TODO: Implement Mapper Methods:
    // 1. A Mapper that will handle grouping options into their releavant categories

    /// <summary>
    /// Maps a grouping of MedicalOption entities to a MedicalOptionCategoryDto.
    /// This extension method transforms the IGrouping results from the repository
    /// into a client-friendly DTO structure that contains the category information
    /// and all associated medical options.
    /// </summary>
    /// <param name="group">
    /// The IGrouping result from the repository containing the MedicalOption entities
    /// grouped by MedicalOptionCategoryId. The collection contains all MedicalOption
    /// entities belonging to that category.
    /// </param>
    /// <returns>
    /// A MedicalOptionCategoryDto populated with:
    /// - MedicalOptionCategoryId: The category ID from the group key
    /// - MedicalOptionCategoryName: The category name from the first option's navigation property
    /// - MedicalOptions: A list of MedicalOptionDto objects representing all options in this category
    /// </returns>
    /// <remarks>
    /// This method assumes that the MedicalOption entities have their MedicalOptionCategory
    /// navigation property loaded (typically via .Include() in the repository query).
    /// If the category is not loaded, the MedicalOptionCategoryName will default to "Uncategorized".
    /// Empty groups return a DTO with "Uncategorized" as the category name.
    /// </remarks>
    /// <example>
    /// Usage in service layer:
    /// <code>
    /// var groupedOptions = await _medicalOptionRepository.GetGroupedMedicalOptionsAsync();
    /// var categoryDtos = groupedOptions.Select(group => group.ToMedicalOptionCategoryDto()).ToList();
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
    /// - Monthly contribution amounts (Risk and MSA contributions)
    /// - Total monthly contributions for different member types (Principal, Adult, Child, Child2)
    /// </returns>
    /// <remarks>
    /// This method performs a direct property mapping with no transformation logic.
    /// All decimal properties maintain their original precision and nullability.
    /// String properties are mapped directly with default empty string handling.
    /// Navigation properties are not included in the DTO to maintain a clean API contract.
    /// </remarks>
    /// <example>
    /// Usage within ToMedicalOptionCategoryDto:
    /// <code>
    /// MedicalOptions = group.Select(option => option.ToMedicalOptionDto()).ToList();
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

    /// <summary>
    /// Maps a MedicalOption entity to an UpdateMedicalOptionVariantsDto.
    /// This transformation is used when preparing data for bulk update operations,
    /// converting the current entity state into a DTO that can be modified and sent back.
    /// </summary>
    /// <param name="bulkUpdate">
    /// The MedicalOption entity to be converted to update DTO format.
    /// Should contain the current state of all updatable properties.
    /// </param>
    /// <returns>
    /// An UpdateMedicalOptionVariantsDto populated with the current values of all
    /// modifiable properties from the source entity, ready for client-side modifications.
    /// </returns>
    /// <remarks>
    /// This method includes all properties that can be modified via the bulk update endpoint.
    /// The MedicalOptionId is preserved to ensure the correct entity is targeted during updates.
    /// Null values in the source entity are preserved as null in the DTO.
    /// </remarks>
    /// <example>
    /// Usage when preparing bulk update data:
    /// <code>
    /// var existingOptions = await repository.GetOptionsByCategoryAsync(categoryId);
    /// var updateDtos = existingOptions.Select(option => option.ToUpdateMedicalOptionVariantDto()).ToList();
    /// </code>
    /// </example>
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
    
    /// <summary>
    /// Updates a MedicalOption entity with values from an UpdateMedicalOptionVariantsDto.
    /// This method applies changes from the DTO to the existing entity, updating only
    /// the properties that are allowed to be modified.
    /// </summary>
    /// <param name="entity">The MedicalOption entity to be updated.</param>
    /// <param name="dto">The DTO containing the new values to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when entity or dto is null.</exception>
    /// <remarks>
    /// This method performs a direct property assignment for all updatable fields.
    /// Null values in the DTO will overwrite existing values in the entity with null.
    /// The method does not update the MedicalOptionId or MedicalOptionCategoryId as these
    /// are considered immutable identifiers.
    /// This approach centralizes the update logic and ensures consistency across the application.
    /// </remarks>
    /// <example>
    /// Usage in repository or service during bulk updates:
    /// <code>
    /// foreach (var updateDto in bulkUpdateDtos)
    /// {
    ///     var entity = await context.MedicalOptions.FindAsync(updateDto.MedicalOptionId);
    ///     entity.UpdateFromDto(updateDto);
    /// }
    /// await context.SaveChangesAsync();
    /// </code>
    /// </example>
    public static void UpdateFromDto(this MedicalOption entity, UpdateMedicalOptionVariantsDto dto)
    {
      entity.SalaryBracketMin = dto.SalaryBracketMin;
      entity.SalaryBracketMax = dto.SalaryBracketMax;
      entity.MonthlyMsaContributionAdult = dto.MonthlyMsaContributionAdult;
      entity.MonthlyMsaContributionChild = dto.MonthlyMsaContributionChild;
      entity.MonthlyMsaContributionPrincipal = dto.MonthlyMsaContributionPrincipal;
      entity.MonthlyRiskContributionAdult = dto.MonthlyRiskContributionAdult;
      entity.MonthlyRiskContributionChild = dto.MonthlyRiskContributionChild;
      entity.MonthlyRiskContributionChild2 = dto.MonthlyRiskContributionChild2;
      entity.MonthlyRiskContributionPrincipal = dto.MonthlyRiskContributionPrincipal;
      entity.TotalMonthlyContributionsAdult = dto.TotalMonthlyContributionsAdult;
      entity.TotalMonthlyContributionsChild = dto.TotalMonthlyContributionsChild;
      entity.TotalMonthlyContributionsChild2 = dto.TotalMonthlyContributionsChild2;
      entity.TotalMonthlyContributionsPrincipal = dto.TotalMonthlyContributionsPrincipal;
    }
  }
}