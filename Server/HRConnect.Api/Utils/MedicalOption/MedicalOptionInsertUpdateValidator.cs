namespace HRConnect.Api.Utils.MedicalOption
{
  using DTOs.MedicalOption;
  using Enums;
  using Enums.Mappers;
  using Interfaces;
  using Models;

  public static class MedicalOptionInsertUpdateValidator
  {
    /*public static MedicalOptionInsertUpdateValidator()
    {
      throw new NotImplementedException();
    }*/

    //1.Period Validator
    //DateTime dateToValidate = /*testDate ??*/ DateTime.UtcNow;





    // Methods
    // 1. PERIOD VALIDATION
    public static bool ValidateUpdatePeriod(DateTime date)
    {
      return DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(date);
    }
    // 1.5. PRE-FILTER ID VALIDATION (Before variant grouping)
    /*public static bool ValidateAllIdsExistAsync(
      IReadOnlyCollection< CreateMedicalOptionVariantsDto > bulkUpdateDto,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var existingIds = dbData.Select(o => o.MedicalOptionId).ToHashSet();

      foreach (var entity in bulkUpdateDto)
      {
        if (!existingIds.Contains(entity.MedicalOptionCategoryId))
          return false;
      }

      return true;
    }*/
    // 2. CATEGORY BUSINESS RULES (FIRST - handles restricted categories | For create the values should not be capped on max)
    public static bool ValidateSalaryBracketRestriction(
      string categoryName,
      decimal? salaryMin,
      decimal? salaryMax)
    {
      var restrictedCategories = Enum.GetValues<NoUpdateOnMedicalOptionCategory>()
        .Select(e => e.ToString()).ToHashSet();

      var isRestricted = restrictedCategories.Contains(categoryName);
      var hasSalaryUpdate = salaryMin > 0 || salaryMax.HasValue;

      return !isRestricted || !hasSalaryUpdate;
    }
    
    private static async Task<BulkValidationResult> ValidateCategoryBusinessRulesAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult() { IsValid = true };

      try
      {
        // Get category info
        var categoryInfo = await repository.GetCategoryByIdAsync(categoryId);
        if (categoryInfo == null)
        {
          result.IsValid = false;
          result.ErrorMessage = $"Category {categoryId} not found";
          return result;
        }

        // Use existing enum mapper to check restrictions
        var variantInfo = MedicalOptionEnumMapper
          .GetCategoryInfoFromVariant(categoryInfo.MedicalOptionCategoryName);

        var isRestricted = Enum.IsDefined(typeof(NoUpdateOnMedicalOptionCategory),
          categoryInfo.MedicalOptionCategoryName);
        // Store Category Name
        //_globalCategoryName = variantInfo.CategoryName;
        if (isRestricted)
        {
          // Check for salary bracket updates in payload
          var salaryBracketUpdates = bulkUpdateDto
            .Where(dto => (dto.SalaryBracketMin.HasValue && dto.SalaryBracketMin > 0) ||
                          (dto.SalaryBracketMax.HasValue || dto.SalaryBracketMax is not null))
            .ToList();

          if (salaryBracketUpdates.Count != 0)
          {
            // Get option names for error message
            var optionIds = salaryBracketUpdates.Select(dto => dto.MedicalOptionId).ToList();
            var dbOptions = await repository.GetMedicalOptionsByIdsAsync(optionIds);
            var optionNames = dbOptions.Select(o => o.MedicalOptionName).ToList();

            result.IsValid = false;
            result.ErrorMessage = $"Salary bracket updates are not allowed for " +
                                  $"{categoryInfo.MedicalOptionCategoryName} category. Options with " +
                                  $"attempted salary updates: [{string.Join(", ", optionNames)}]. " +
                                  $"{categoryInfo.MedicalOptionCategoryName} categories should only " +
                                  $"have minimum salary of 0 and unlimited maximum.";
            return result;
          }

          // Use existing validation method for each DTO
          foreach (var dto in bulkUpdateDto)
          {
            var dbOption = dbData.FirstOrDefault(
              o => o.MedicalOptionId == dto.MedicalOptionId);
            if (dbOption != null)
            {
              if (!ValidateSalaryBracketRestriction(categoryInfo.MedicalOptionCategoryName,
                    dto.SalaryBracketMin, dto.SalaryBracketMax))
              {
                result.IsValid = false;
                result.ErrorMessage = $"Salary bracket updates not allowed for " +
                                      $"{categoryInfo.MedicalOptionCategoryName} category: " +
                                      $"{dbOption.MedicalOptionName}";
                return result;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Category business rules validation error: {ex.Message}";
      }

      return result;
    }
    
  }
}