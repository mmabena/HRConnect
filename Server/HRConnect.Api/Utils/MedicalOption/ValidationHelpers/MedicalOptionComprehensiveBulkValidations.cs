namespace HRConnect.Api.Utils.MedicalOption.ValidationHelpers
{
  using DTOs.MedicalOption;
  using Interfaces;
  using Models;
  using Records;

  public static class MedicalOptionComprehensiveBulkValidations
  {
    #region Comprehensive Bulk Validation

    /// <summary>
    /// Comprehensive validation for bulk update operations
    /// </summary>
    public static async Task<BulkValidationResult> ValidateBulkUpdateAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult { IsValid = true };
      var salaryBracketRecords = new List<SalaryBracketValidatorRecord>();

      try
      {
        // 1. Update Period Validation
        if (!MedicalOptionBasicValidations.ValidateUpdatePeriod(DateTime.Now))
        {
          result.IsValid = false;
          result.ErrorMessage = "Bulk update operation cannot be executed outside " +
                                "the update period";
          return result;
        }

        // 2. Entity Count Validation
        if (!MedicalOptionBasicValidations.ValidateEntityCount(dbData.Count, bulkUpdateDto.Count))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options not found in the " +
                                "specified category";
          return result;
        }

        // 3. ID Validations
        if (!await MedicalOptionBasicValidations.ValidateAllIdsExistAsync(bulkUpdateDto, repository, dbData))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options are invalid";
          return result;
        }

        if (!await MedicalOptionBasicValidations.ValidateAllIdsInCategoryAsync(bulkUpdateDto, categoryId, repository, dbData))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options are invalid within the " +
                                "specified category";
          return result;
        }

        // 4. Individual Entity Validations
        foreach (var entity in bulkUpdateDto)
        {
          var dbOption = dbData
            .First(o => o.MedicalOptionId == entity.MedicalOptionId);
          var categoryName = 
            dbOption.MedicalOptionCategory?.MedicalOptionCategoryName ?? string.Empty;

          // Salary bracket restriction validation
          var restricted = MedicalOptionSalaryBracketValidations.ValidateSalaryBracketRestriction(categoryName, 
            entity.SalaryBracketMin, entity.SalaryBracketMax);
          
          if (!restricted)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Salary bracket cannot be updated for category" +
                                  $" '{categoryName}'";
            return result;
          }

          // Contribution validation
          if (!MedicalOptionContributionValidations.ValidateContributionValuesWithContext(entity, dbOption))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Invalid contribution values for medical option" +
                                  $" {entity.MedicalOptionId}";
            return result;
          }

          if (!restricted)
          {
            // Add to salary bracket records for range validation
            if (entity.SalaryBracketMin.HasValue || entity.SalaryBracketMax.HasValue)
            {
              salaryBracketRecords.Add(new SalaryBracketValidatorRecord(
                entity.MedicalOptionId,
                dbOption.MedicalOptionName,
                entity.SalaryBracketMin,
                entity.SalaryBracketMax));
            }
          }
        }

        // 5. Salary Range Validations (only if we have salary bracket updates)
        if (salaryBracketRecords.Count > 0)
        {
          if (!MedicalOptionSalaryBracketValidations.ValidateNoGapsInSalaryRanges(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = "Gaps detected in salary ranges";
            return result;
          }

          if (!MedicalOptionSalaryBracketValidations.ValidateNoOverlappingBrackets(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = "Overlapping salary brackets detected";
            return result;
          }
        }
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Validation error: {ex.Message}";
      }

      return result;
    }

    #endregion
  }
}