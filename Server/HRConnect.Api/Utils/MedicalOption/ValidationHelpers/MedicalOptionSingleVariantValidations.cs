namespace HRConnect.Api.Utils.MedicalOption.ValidationHelpers
{
  using DTOs.MedicalOption;
  using Interfaces;
  using Models;

  public class MedicalOptionSingleVariantValidations
  {
    #region Single Variant Validation

    /// <summary>
    /// Validates a single variant with ALL validation rules applied
    /// </summary>
    public static async Task<BulkValidationResult> ValidateSingleVariantWithAllRulesAsync(
      string variantName,
      List<UpdateMedicalOptionVariantsDto> variantOptions,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository,
      int categoryId, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult();

      try
      {
        // 1. Entity count validation for this variant
        var variantUpdateDtos = bulkUpdateDto
          .Where(dto => variantOptions
            .Any(opt => opt.MedicalOptionId == dto.MedicalOptionId))
          .ToList();

        if (!MedicalOptionBasicValidations.ValidateEntityCount(variantOptions.Count, variantUpdateDtos.Count))
        {
          result.IsValid = false;
          result.ErrorMessage = $"Entity count mismatch for variant '{variantName}'. " +
                                $"Expected: {variantOptions.Count}, Provided: " +
                                $"{variantUpdateDtos.Count}";
          return result;
        }

        // 2. ID existence validation for this variant
        foreach (var dto in variantUpdateDtos)
        {
          if (!await repository.MedicalOptionExistsAsync(dto.MedicalOptionId))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Medical option ID {dto.MedicalOptionId} does not exist in " +
                                  $"variant '{variantName}'";
            return result;
          }
        }

        // 3. Category membership validation for this variant
        foreach (var dto in variantUpdateDtos)
        {
          if (!await repository.MedicalOptionExistsWithinCategoryAsync(categoryId,
                dto.MedicalOptionId))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Medical option ID {dto.MedicalOptionId} does not belong to " +
                                  $"category {categoryId} in variant '{variantName}'";
            return result;
          }
        }

        // 4. Individual contribution validation for this variant
        foreach (var dto in variantUpdateDtos)
        {
          var dbOption = dbData.FirstOrDefault(
            o => o.MedicalOptionId == dto.MedicalOptionId);
          if (dbOption != null && !MedicalOptionContributionValidations.ValidateContributionValuesWithContext(dto, dbOption))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Invalid contribution values for option {dto.MedicalOptionId} " +
                                  $"({dbOption.MedicalOptionName}) in variant '{variantName}'";
            return result;
          }
        }

        // 5. Salary bracket gap/overlap validation for this variant
        // Get the database options for this variant
        var dbVariantOptions = dbData
          .Where(o => variantOptions.Any(dto => dto.MedicalOptionId == o.MedicalOptionId))
          .ToList();

        var salaryValidation = MedicalOptionContributionValidations.ValidateSingleVariantSalaryBrackets(dbVariantOptions);
        if (!salaryValidation.IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = $"Salary bracket validation failed for variant '{variantName}': " +
                                $"{salaryValidation.ErrorMessage}";
          return result;
        }

        // 6. Contribution structure consistency within this variant
        var contributionStructureValidation = ValidateContributionStructureWithinVariant(
          variantOptions);
        if (!contributionStructureValidation.IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = $"Contribution structure inconsistency in variant '{variantName}': " +
                                $"{contributionStructureValidation.ErrorMessage}";
          return result;
        }

        // 7. Variant-specific rules
        var variantRulesValidation = MedicalOptionValidator.ValidateVariantSpecificBusinessRules(variantName, variantOptions, 
          dbData);
        if (!variantRulesValidation.IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = $"Variant-specific rules failed for '{variantName}': " +
                                $"{variantRulesValidation.ErrorMessage}";
          return result;
        }
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Single variant validation error for '{variantName}': {ex.Message}";
      }

      return result;
    }

    /// <summary>
    /// Validates contribution structure consistency within a single variant
    /// </summary>
    private static BulkValidationResult ValidateContributionStructureWithinVariant(
      List<UpdateMedicalOptionVariantsDto> variantOptions)
    {
      var result = new BulkValidationResult(){IsValid = true};

      try {
        if (variantOptions.Count < 2) return result; // Not applicable for single option

        // Get first option for expected structure
        var firstOption = variantOptions.First();
        var expectedHasMsa = firstOption.MonthlyMsaContributionAdult.HasValue &&
                             firstOption.MonthlyMsaContributionAdult > 0;
        var expectedHasPrincipal = firstOption.MonthlyRiskContributionPrincipal.HasValue &&
                                   firstOption.MonthlyRiskContributionPrincipal > 0;

        foreach (var option in variantOptions.Skip(1)) {
          var actualHasMsa = option.MonthlyMsaContributionAdult.HasValue &&
                             option.MonthlyMsaContributionAdult > 0;
          var actualHasPrincipal = option.MonthlyRiskContributionPrincipal.HasValue &&
                                   option.MonthlyRiskContributionPrincipal > 0;

          if (expectedHasMsa != actualHasMsa) {
            result.IsValid = false;
            result.ErrorMessage = $"Inconsistent MSA structure within variant: Option " +
                                  $"{firstOption.MedicalOptionId} has MSA, but Option " +
                                  $"{option.MedicalOptionId} does not";
            return result;
          }

          if (expectedHasPrincipal != actualHasPrincipal) {
            result.IsValid = false;
            result.ErrorMessage = $"Inconsistent Principal structure within variant: Option " +
                                  $"{firstOption.MedicalOptionId} has Principal, but Option " +
                                  $"{option.MedicalOptionId} does not";
            return result;
          }
        }
      }
      catch (Exception ex) {
        result.IsValid = false;
        result.ErrorMessage = $"Contribution structure validation error: {ex.Message}";
      }

      return result;
    }

    #endregion
  }
}