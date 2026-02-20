namespace HRConnect.Api.Utils.Enums.MedicalOption
{
  using DTOs.MedicalOption;
  using Interfaces;
  using Mappers;
  using Models.MedicalOptions.Records;

  public static class MedicalOptionValidator
  {
    //TODO:
    //1. Validate Entity Count against bulkPayload Count
    //2. Validate if each ID is valid from the bulk payload
    //3. Validate Salary Bracket for the following:
    //   -=> Alliance and Double must not have a salary bracket cap
    //   -=> Updates can only be done between November and December (31st 59:59:59)
    //   -=> There should be no gaps in between the salary ranges (they should be apart by half a
    //       cent/ or a rand)
    //   -=> There should be no overlapping salary bracket caps within the same category
    //       (where one option overlaps 1+ other options in the same category)
    //   -=> Only perform an update if all the entities within the payload exist in the database
    //       (only on valid ID numbers) -- out of the scope of this file's operation
    //4. Do cross-checks on the contribution values if the payload's value balance out
    //   (will need to cater for nulls as not all options have RISK + MSA = Total Contrib)
    
     #region Basic Validations

    /// <summary>
    /// Validates if the update operation is within the allowed period (November-December)
    /// </summary>
    public static bool ValidateUpdatePeriod()
    {
      return DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(DateTime.Now);
    }

    /// <summary>
    /// Validates entity count matches between database and payload
    /// </summary>
    public static bool ValidateEntityCount(int dbEntityCount, int bulkPayloadCount)
    {
      return dbEntityCount == bulkPayloadCount;
    }

    /// <summary>
    /// Validates if all medical option IDs in the payload exist in the database
    /// </summary>
    public static async Task<bool> ValidateAllIdsExistAsync(
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository)
    {
      foreach (var entity in bulkUpdateDto)
      {
        if (!await repository.MedicalOptionExistsAsync(entity.MedicalOptionId))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Validates if all entities belong to the specified category
    /// </summary>
    public static async Task<bool> ValidateAllIdsInCategoryAsync(
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      int categoryId,
      IMedicalOptionRepository repository)
    {
      foreach (var entity in bulkUpdateDto)
      {
        if (!await repository.MedicalOptionExistsWithinCategoryAsync(categoryId, entity.MedicalOptionId))
          return false;
      }
      return true;
    }

    #endregion

    #region Salary Bracket Validations

    /// <summary>
    /// Validates salary bracket restrictions for Alliance and Double categories
    /// </summary>
    public static bool ValidateSalaryBracketRestriction(
      string categoryName,
      decimal? salaryMin,
      decimal? salaryMax)
    {
      var restrictedCategories = Enum.GetValues<NoUpdateOnMedicalOptionCategory>()
        .Select(e => e.ToString()).ToHashSet();

      var isRestricted = restrictedCategories.Contains(categoryName);
      var hasSalaryUpdate = salaryMin.HasValue || salaryMax.HasValue;

      return !isRestricted || !hasSalaryUpdate;
    }

    /// <summary>
    /// Validates there are no gaps between salary ranges
    /// </summary>
    public static bool ValidateNoGapsInSalaryRanges(
      List<SalaryBracketValidatorRecord> records)
    {
      var sortedRecords = records
        .Where(r => r.salaryBracketMin.HasValue && r.salaryBracketMax.HasValue)
        .OrderBy(r => r.salaryBracketMin)
        .ToList();

      for (int i = 0; i < sortedRecords.Count - 1; i++)
      {
        var current = sortedRecords[i];
        var next = sortedRecords[i + 1];

        // Allow for 0.01 or 1.00 difference between ranges
        var expectedNextMin = current.salaryBracketMax.Value + 0.01m;
        if (next.salaryBracketMin > expectedNextMin + 0.99m) // Allow up to 1.00 gap
          return false;
      }

      return true;
    }

    /// <summary>
    /// Validates there are no overlapping salary brackets within the same category
    /// </summary>
    public static bool ValidateNoOverlappingBrackets(
      List<SalaryBracketValidatorRecord> records)
    {
      var sortedRecords = records
        .Where(r => r.salaryBracketMin.HasValue && r.salaryBracketMax.HasValue)
        .OrderBy(r => r.salaryBracketMin)
        .ToList();

      for (int i = 0; i < sortedRecords.Count - 1; i++)
      {
        var current = sortedRecords[i];
        var next = sortedRecords[i + 1];

        // Check for overlap (next.min should be > current.max)
        if (next.salaryBracketMin <= current.salaryBracketMax)
          return false;
      }

      return true;
    }

    #endregion

    #region Contribution Validations

    /// <summary>
    /// Validates contribution values based on database context
    /// </summary>
    public static bool ValidateContributionValuesWithContext(
      UpdateMedicalOptionVariantsDto entity,
      MedicalOption dbOption)
    {
      // Determine if option has MSA and Principal based on database
      var hasMsa = dbOption.MonthlyMsaContributionAdult.HasValue && 
                   dbOption.MonthlyMsaContributionAdult > 0;
      var hasPrincipal = dbOption.MonthlyRiskContributionPrincipal.HasValue && 
                        dbOption.MonthlyRiskContributionPrincipal > 0;

      return ValidateContributionValues(entity, hasMsa, hasPrincipal);
    }

    /// <summary>
    /// Core contribution validation logic
    /// </summary>
    public static bool ValidateContributionValues(
      UpdateMedicalOptionVariantsDto entity,
      bool hasMsa,
      bool hasPrincipal)
    {
      // Validate Risk contributions (all options must have these)
      if (!ValidateRiskContributions(entity, hasPrincipal))
        return false;

      // Validate MSA contributions (if applicable)
      if (hasMsa && !ValidateMsaContributions(entity, hasPrincipal))
        return false;

      // Validate Total contributions
      return ValidateTotalContributions(entity, hasMsa, hasPrincipal);
    }

    private static bool ValidateRiskContributions(
      UpdateMedicalOptionVariantsDto entity,
      bool hasPrincipal)
    {
      // Adult and Child risk contributions must be > 0
      if (entity.MonthlyRiskContributionAdult <= 0 || 
          entity.MonthlyRiskContributionChild <= 0)
        return false;

      // Principal validation (if applicable)
      if (hasPrincipal && 
          (!entity.MonthlyRiskContributionPrincipal.HasValue || 
           entity.MonthlyRiskContributionPrincipal <= 0))
        return false;

      return true;
    }

    private static bool ValidateMsaContributions(
      UpdateMedicalOptionVariantsDto entity,
      bool hasPrincipal)
    {
      // Adult and Child MSA contributions must be > 0
      if (!entity.MonthlyMsaContributionAdult.HasValue || 
          entity.MonthlyMsaContributionAdult <= 0 ||
          !entity.MonthlyMsaContributionChild.HasValue || 
          entity.MonthlyMsaContributionChild <= 0)
        return false;

      // Principal MSA validation (if applicable)
      if (hasPrincipal && 
          (!entity.MonthlyMsaContributionPrincipal.HasValue || 
           entity.MonthlyMsaContributionPrincipal <= 0))
        return false;

      return true;
    }

    private static bool ValidateTotalContributions(
      UpdateMedicalOptionVariantsDto entity,
      bool hasMsa,
      bool hasPrincipal)
    {
      const decimal tolerance = 0.01m;

      if (hasMsa)
      {
        // Risk + MSA should equal Total
        var adultTotal = entity.MonthlyRiskContributionAdult + entity.MonthlyMsaContributionAdult.Value;
        var childTotal = entity.MonthlyRiskContributionChild + entity.MonthlyMsaContributionChild.Value;

        if (Math.Abs(adultTotal - entity.TotalMonthlyContributionsAdult) > tolerance)
          return false;

        if (Math.Abs(childTotal - entity.TotalMonthlyContributionsChild) > tolerance)
          return false;

        if (hasPrincipal)
        {
          var principalTotal = entity.MonthlyRiskContributionPrincipal.Value + 
                               entity.MonthlyMsaContributionPrincipal.Value;
          if (Math.Abs(principalTotal - entity.TotalMonthlyContributionsPrincipal.Value) > tolerance)
            return false;
        }
      }
      else
      {
        // Risk should equal Total when no MSA
        if (Math.Abs(entity.MonthlyRiskContributionAdult - entity.TotalMonthlyContributionsAdult) > tolerance)
          return false;

        if (Math.Abs(entity.MonthlyRiskContributionChild - entity.TotalMonthlyContributionsChild) > tolerance)
          return false;

        if (hasPrincipal && 
            Math.Abs(entity.MonthlyRiskContributionPrincipal.Value - 
                     entity.TotalMonthlyContributionsPrincipal.Value) > tolerance)
          return false;
      }

      return true;
    }

    #endregion

    #region Comprehensive Bulk Validation

    /// <summary>
    /// Comprehensive validation for bulk update operations
    /// </summary>
    public static async Task<BulkValidationResult> ValidateBulkUpdateAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository,
      List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult { IsValid = true };
      var salaryBracketRecords = new List<SalaryBracketValidatorRecord>();

      try
      {
        // 1. Update Period Validation
        if (!ValidateUpdatePeriod())
        {
          result.IsValid = false;
          result.ErrorMessage = "Bulk update operation cannot be executed outside the update period";
          return result;
        }

        // 2. Entity Count Validation
        if (!ValidateEntityCount(dbData.Count, bulkUpdateDto.Count))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options not found in the specified category";
          return result;
        }

        // 3. ID Validations
        if (!await ValidateAllIdsExistAsync(bulkUpdateDto, repository))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options are invalid";
          return result;
        }

        if (!await ValidateAllIdsInCategoryAsync(bulkUpdateDto, categoryId, repository))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options are invalid within the specified category";
          return result;
        }

        // 4. Individual Entity Validations
        foreach (var entity in bulkUpdateDto)
        {
          var dbOption = dbData.First(o => o.MedicalOptionId == entity.MedicalOptionId);
          var categoryName = dbOption.MedicalOptionCategory?.MedicalOptionCategoryName ?? string.Empty;

          // Salary bracket restriction validation
          if (!ValidateSalaryBracketRestriction(categoryName, entity.SalaryBracketMin, entity.SalaryBracketMax))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Salary bracket cannot be updated for category '{categoryName}'";
            return result;
          }

          // Contribution validation
          if (!ValidateContributionValuesWithContext(entity, dbOption))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Invalid contribution values for medical option {entity.MedicalOptionId}";
            return result;
          }

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

        // 5. Salary Range Validations (only if we have salary bracket updates)
        if (salaryBracketRecords.Any())
        {
          if (!ValidateNoGapsInSalaryRanges(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = "Gaps detected in salary ranges";
            return result;
          }

          if (!ValidateNoOverlappingBrackets(salaryBracketRecords))
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

  /// <summary>
  /// Result object for bulk validation operations
  /// </summary>
  public class BulkValidationResult
  {
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SalaryBracketValidatorRecord> SalaryBracketRecords { get; set; } = new();
 
  } 
}