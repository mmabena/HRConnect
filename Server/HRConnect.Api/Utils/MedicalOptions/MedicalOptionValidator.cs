namespace HRConnect.Api.Utils.MedicalOptions
{
  using HRConnect.Api.DTOs.MedicalOption;
  using Enums;
  using Enums.Mappers;
  using Factories;
  using Interfaces;
  using Mappers;
  using Models;
  using Models.MedicalOptions.Records;
  using HRConnect.Api.Utils.Enums.Mappers;

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
    //4. Do cross-checks on the contribution values if the payload's value balance out (per option in the variant)
    //   (will need to cater for nulls as not all options have RISK + MSA = Total Contrib)

    //Helper methods

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
        if (!await repository.MedicalOptionExistsWithinCategoryAsync(categoryId,
              entity.MedicalOptionId))
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
      var hasSalaryUpdate = salaryMin > 0 || salaryMax.HasValue;

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
/*
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
*/

    /// <summary>
    /// Validates salary bracket gaps and overlaps within a single variant
    /// </summary>
    private static BulkValidationResult ValidateSingleVariantSalaryBrackets(List<MedicalOption> variantOptions)
    {
      var result = new BulkValidationResult();

      try
      {
        var salaryBracketRecords = new List<SalaryBracketValidatorRecord>();

        foreach (var option in variantOptions)
        {
          if (option.SalaryBracketMin.HasValue || option.SalaryBracketMax.HasValue)
          {
            salaryBracketRecords.Add(new SalaryBracketValidatorRecord(
              option.MedicalOptionId,
              option.MedicalOptionName,
              option.SalaryBracketMin,
              option.SalaryBracketMax));
          }
        }

        if (salaryBracketRecords.Count > 1)
        {
          if (!ValidateNoGapsInSalaryRanges(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = "Gaps detected in salary ranges within variant";
            return result;
          }

          if (!ValidateNoOverlappingBrackets(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = "Overlapping salary brackets detected within variant";
            return result;
          }
        }

        result.IsValid = true;
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Salary bracket validation error: {ex.Message}";
      }

      return result;
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
        var adultTotal = entity.MonthlyRiskContributionAdult +
                         entity.MonthlyMsaContributionAdult.Value;
        var childTotal = entity.MonthlyRiskContributionChild +
                         entity.MonthlyMsaContributionChild.Value;

        if (Math.Abs((decimal)(adultTotal - entity.TotalMonthlyContributionsAdult)) > tolerance)
          return false;

        if (Math.Abs((decimal)(childTotal - entity.TotalMonthlyContributionsChild)) > tolerance)
          return false;

        if (hasPrincipal)
        {
          var principalTotal = entity.MonthlyRiskContributionPrincipal.Value +
                               entity.MonthlyMsaContributionPrincipal.Value;
          if (Math.Abs(principalTotal - entity.TotalMonthlyContributionsPrincipal.Value) >
              tolerance)
            return false;
        }
      }
      else
      {
        // Risk should equal Total when no MSA
        if (Math.Abs((decimal)(entity.MonthlyRiskContributionAdult - entity.TotalMonthlyContributionsAdult)) >
            tolerance)
          return false;

        if (Math.Abs((decimal)(entity.MonthlyRiskContributionChild - entity.TotalMonthlyContributionsChild)) >
            tolerance)
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
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
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
          var restricted = ValidateSalaryBracketRestriction(categoryName, entity.SalaryBracketMin, entity.SalaryBracketMax);
          if (!restricted)
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
    
    // Variant based implementation
    #region Variant Comprehensive Validation

/// <summary>
/// Comprehensive validation that processes each variant individually with full validation,
/// then performs cross-variant validation
/// </summary>
public static async Task<BulkValidationResult> ValidateAllVariantsComprehensiveAsync(
  int categoryId,
  IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
  IMedicalOptionRepository repository,
  List<MedicalOption> dbData)
{
  var result = new BulkValidationResult();

  try
  {
    // Basic period validation (global)
    if (!ValidateUpdatePeriod())
    {
      result.IsValid = false;
      result.ErrorMessage = "Updates can only be performed between November and December";
      return result;
    }

    if (dbData.Count == 0)
    {
      result.IsValid = false;
      result.ErrorMessage = $"No medical options found for category ID: {categoryId}";
      return result;
    }

    // Group by variant using your existing factory
    var variantGroups = GroupOptionsByVariant(dbData);
    
    // Validate each variant individually with ALL validations
    foreach (var variantGroup in variantGroups)
    {
      var variantValidation = await ValidateSingleVariantWithAllRulesAsync(
        variantGroup.Key, 
        variantGroup.Value, 
        bulkUpdateDto, 
        repository,
        categoryId);

      if (!variantValidation.IsValid)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Variant '{variantGroup.Key}' validation failed: {variantValidation.ErrorMessage}";
        return result;
      }
    }

    // Cross-variant validation (only after all individual variants pass)
    var crossVariantResult = ValidateCrossVariantComprehensiveAsync(
      variantGroups, bulkUpdateDto, categoryId);
      
    if (!crossVariantResult.IsValid)
    {
      result.IsValid = false;
      result.ErrorMessage = $"Cross-variant validation failed: {crossVariantResult.ErrorMessage}";
      return result;
    }
  }
  catch (Exception ex)
  {
    result.IsValid = false;
    result.ErrorMessage = $"Comprehensive validation error: {ex.Message}";
  }

  return result;
}

#endregion

#region Single Variant Validation

/// <summary>
/// Validates a single variant with ALL validation rules applied
/// </summary>
private static async Task<BulkValidationResult> ValidateSingleVariantWithAllRulesAsync(
  string variantName,
  List<MedicalOption> variantOptions,
  IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
  IMedicalOptionRepository repository,
  int categoryId)
{
  var result = new BulkValidationResult();

  try
  {
    // 1. Entity count validation for this variant
    var variantUpdateDtos = bulkUpdateDto
      .Where(dto => variantOptions.Any(opt => opt.MedicalOptionId == dto.MedicalOptionId))
      .ToList();

    if (!ValidateEntityCount(variantOptions.Count, variantUpdateDtos.Count))
    {
      result.IsValid = false;
      result.ErrorMessage = $"Entity count mismatch for variant '{variantName}'. Expected: {variantOptions.Count}, Provided: {variantUpdateDtos.Count}";
      return result;
    }

    // 2. ID existence validation for this variant
    foreach (var dto in variantUpdateDtos)
    {
      if (!await repository.MedicalOptionExistsAsync(dto.MedicalOptionId))
      {
        result.IsValid = false;
        result.ErrorMessage = $"Medical option ID {dto.MedicalOptionId} does not exist in variant '{variantName}'";
        return result;
      }
    }

    // 3. Category membership validation for this variant
    foreach (var dto in variantUpdateDtos)
    {
      if (!await repository.MedicalOptionExistsWithinCategoryAsync(categoryId, dto.MedicalOptionId))
      {
        result.IsValid = false;
        result.ErrorMessage = $"Medical option ID {dto.MedicalOptionId} does not belong to category {categoryId} in variant '{variantName}'";
        return result;
      }
    }

    // 4. Individual contribution validation for this variant
    foreach (var dto in variantUpdateDtos)
    {
      var dbOption = variantOptions.FirstOrDefault(o => o.MedicalOptionId == dto.MedicalOptionId);
      if (dbOption != null && !ValidateContributionValuesWithContext(dto, dbOption))
      {
        result.IsValid = false;
        result.ErrorMessage = $"Invalid contribution values for option {dto.MedicalOptionId} ({dbOption.MedicalOptionName}) in variant '{variantName}'";
        return result;
      }
    }

    // 5. Salary bracket gap/overlap validation for this variant
    var salaryValidation = ValidateSingleVariantSalaryBrackets(variantOptions);
    if (!salaryValidation.IsValid)
    {
      result.IsValid = false;
      result.ErrorMessage = $"Salary bracket validation failed for variant '{variantName}': {salaryValidation.ErrorMessage}";
      return result;
    }

    // 6. Contribution structure consistency within this variant
    var contributionStructureValidation = ValidateContributionStructureWithinVariant(variantOptions);
    if (!contributionStructureValidation.IsValid)
    {
      result.IsValid = false;
      result.ErrorMessage = $"Contribution structure inconsistency in variant '{variantName}': {contributionStructureValidation.ErrorMessage}";
      return result;
    }

    // 7. Variant-specific rules
    var variantRulesValidation = ValidateVariantSpecificRules(variantName, variantOptions);
    if (!variantRulesValidation.IsValid)
    {
      result.IsValid = false;
      result.ErrorMessage = $"Variant-specific rules failed for '{variantName}': {variantRulesValidation.ErrorMessage}";
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
  List<MedicalOption> variantOptions)
{
  var result = new BulkValidationResult();

  try
  {
    if (variantOptions.Count < 2) return result; // Skip if only one option

    var firstOption = variantOptions.First();
    var expectedHasMsa = firstOption.MonthlyMsaContributionAdult.HasValue &&
                         firstOption.MonthlyMsaContributionAdult > 0;
    var expectedHasPrincipal = firstOption.MonthlyRiskContributionPrincipal.HasValue &&
                               firstOption.MonthlyRiskContributionPrincipal > 0;

    foreach (var option in variantOptions.Skip(1))
    {
      var actualHasMsa = option.MonthlyMsaContributionAdult.HasValue &&
                        option.MonthlyMsaContributionAdult > 0;
      var actualHasPrincipal = option.MonthlyRiskContributionPrincipal.HasValue &&
                              option.MonthlyRiskContributionPrincipal > 0;

      if (expectedHasMsa != actualHasMsa)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Inconsistent MSA structure within variant: {firstOption.MedicalOptionName} has MSA, but {option.MedicalOptionName} does not";
        return result;
      }

      if (expectedHasPrincipal != actualHasPrincipal)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Inconsistent Principal structure within variant: {firstOption.MedicalOptionName} has Principal, but {option.MedicalOptionName} does not";
        return result;
      }
    }
  }
  catch (Exception ex)
  {
    result.IsValid = false;
    result.ErrorMessage = $"Contribution structure validation error: {ex.Message}";
  }

  return result;
}

/// <summary>
/// Validates variant-specific rules
/// </summary>
private static BulkValidationResult ValidateVariantSpecificRules(
  string variantName,
  List<MedicalOption> variantOptions)
{
  var result = new BulkValidationResult();

  try
  {
    if (variantName.Contains("Network"))
    {
      var networkValidation = ValidateNetworkVariantRules(variantOptions);
      if (!networkValidation.IsValid)
      {
        result.IsValid = false;
        result.ErrorMessage = networkValidation.ErrorMessage;
        return result;
      }
    }

    if (variantName.Contains("Plus"))
    {
      var plusValidation = ValidatePlusVariantRules(variantOptions);
      if (!plusValidation.IsValid)
      {
        result.IsValid = false;
        result.ErrorMessage = plusValidation.ErrorMessage;
        return result;
      }
    }
  }
  catch (Exception ex)
  {
    result.IsValid = false;
    result.ErrorMessage = $"Variant-specific rules validation error: {ex.Message}";
  }

  return result;
}

/// <summary>
/// Placeholder for network variant validation
/// </summary>
private static BulkValidationResult ValidateNetworkVariantRules(List<MedicalOption> variantOptions)
{
  var result = new BulkValidationResult { IsValid = true };
  
  // Add network-specific validation logic here
  
  return result;
}

/// <summary>
/// Placeholder for plus variant validation
/// </summary>
private static BulkValidationResult ValidatePlusVariantRules(List<MedicalOption> variantOptions)
{
  var result = new BulkValidationResult { IsValid = true };
  
  // Add plus-specific validation logic here
  
  return result;
}

#endregion

#region Cross-Variant Validation

/// <summary>
/// Comprehensive cross-variant validation
/// </summary>
private static BulkValidationResult ValidateCrossVariantComprehensiveAsync(
  Dictionary<string, List<MedicalOption>> variantGroups,
  IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
  int categoryId)
{
  var result = new BulkValidationResult();

  try
  {
    // 1. Cross-variant salary bracket overlaps
    var allBrackets = variantGroups
      .SelectMany(group => group.Value)
      .Where(o => o.SalaryBracketMin.HasValue && o.SalaryBracketMax.HasValue)
      .OrderBy(o => o.SalaryBracketMin)
      .ToList();

    for (int i = 0; i < allBrackets.Count - 1; i++)
    {
      var current = allBrackets[i];
      var next = allBrackets[i + 1];

      var currentVariant = MedicalOptionVariantFactory.GetVariantInfoSafe(current);
      var nextVariant = MedicalOptionVariantFactory.GetVariantInfoSafe(next);

      if (currentVariant.CategoryName != nextVariant.CategoryName && 
          next.SalaryBracketMin <= current.SalaryBracketMax)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Cross-variant salary overlap: {current.MedicalOptionName} ({currentVariant.CategoryName}) and {next.MedicalOptionName} ({nextVariant.CategoryName})";
        return result;
      }
    }

    // 2. Cross-variant contribution structure consistency
    var variantStructures = new Dictionary<string, (bool HasMsa, bool HasPrincipal)>();

    foreach (var variantGroup in variantGroups)
    {
      var firstOption = variantGroup.Value.FirstOrDefault();
      if (firstOption != null)
      {
        var hasMsa = firstOption.MonthlyMsaContributionAdult.HasValue &&
                     firstOption.MonthlyMsaContributionAdult > 0;
        var hasPrincipal = firstOption.MonthlyRiskContributionPrincipal.HasValue &&
                           firstOption.MonthlyRiskContributionPrincipal > 0;

        variantStructures[variantGroup.Key] = (hasMsa, hasPrincipal);
      }
    }

    var uniqueStructures = variantStructures.Values.Distinct().ToList();
    if (uniqueStructures.Count > 1)
    {
      result.IsValid = false;
      result.ErrorMessage = $"Inconsistent contribution structures across variants in category {categoryId}. Structures: {string.Join(", ", variantStructures.Select(kvp => $"{kvp.Key}({(kvp.Value.HasMsa ? "MSA" : "NoMSA")}/{(kvp.Value.HasPrincipal ? "Principal" : "NoPrincipal")})"))}";
      return result;
    }

    // 3. Cross-variant business rules
    var crossVariantRules = ValidateCrossVariantBusinessRules(variantGroups);
    if (!crossVariantRules.IsValid)
    {
      result.IsValid = false;
      result.ErrorMessage = $"Cross-variant business rules failed: {crossVariantRules.ErrorMessage}";
      return result;
    }
  }
  catch (Exception ex)
  {
    result.IsValid = false;
    result.ErrorMessage = $"Cross-variant validation error: {ex.Message}";
  }

  return result;
}

/// <summary>
/// Validates cross-variant business rules
/// </summary>
private static BulkValidationResult ValidateCrossVariantBusinessRules(
  Dictionary<string, List<MedicalOption>> variantGroups)
{
  var result = new BulkValidationResult();

  try
  {
    // Example: Network variants should not overlap with Plus variants
    var networkVariants = variantGroups
      .Where(g => g.Key.Contains("Network"))
      .SelectMany(g => g.Value)
      .ToList();

    var plusVariants = variantGroups
      .Where(g => g.Key.Contains("Plus"))
      .SelectMany(g => g.Value)
      .ToList();

    // Add specific cross-variant rules here
    if (networkVariants.Count != 0 && plusVariants.Count != 0)
    {
      // Example rule: Network variants should have lower contributions than Plus variants
      var avgNetworkContribution = networkVariants.Average(o => o.TotalMonthlyContributionsAdult);
      var avgPlusContribution = plusVariants.Average(o => o.TotalMonthlyContributionsAdult);

      if (avgNetworkContribution >= avgPlusContribution)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Network variants should have lower contributions than Plus variants. Network avg: R{avgNetworkContribution:N2}, Plus avg: R{avgPlusContribution:N2}";
        return result;
      }
    }
  }
  catch (Exception ex)
  {
    result.IsValid = false;
    result.ErrorMessage = $"Cross-variant business rules error: {ex.Message}";
  }

  return result;
}

#endregion


    #region Comprehensive Category Variant Validation

/// <summary>
/// MAIN VALIDATION ENTRY POINT
/// Comprehensive validation for all variants within a category using existing validation logic as core
/// Addresses TODO items from lines 14-26 with enhanced error handling
/// </summary>
public static async Task<BulkValidationResult> ValidateAllCategoryVariantsComprehensiveAsync(
  int categoryId,
  IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
  IMedicalOptionRepository repository,
  List<MedicalOption> dbData)
{
  var result = new BulkValidationResult();

  try {
    // 1. PERIOD VALIDATION (Global requirement)
    if (!ValidateUpdatePeriod()) {
      result.IsValid = false;
      result.ErrorMessage = "Updates can only be performed between November and December";
      return result;
    }

    if (dbData.Count == 0) {
      result.IsValid = false;
      result.ErrorMessage = $"No medical options found for category ID: {categoryId}";
      return result;
    }

    // 2. CATEGORY BUSINESS RULES (FIRST - handles restricted categories)
    var categoryValidation = await ValidateCategoryBusinessRulesAsync(
      categoryId, bulkUpdateDto, repository);
      
    if (!categoryValidation.IsValid) {
      result.IsValid = false;
      result.ErrorMessage = categoryValidation.ErrorMessage;
      return result;
    }

    // 3. GROUP BY VARIANT using existing factory
    var variantGroups = GroupOptionsByVariant(dbData);
    
    if (variantGroups.Count == 0) {
      result.IsValid = false;
      result.ErrorMessage = $"No valid variants found for category ID: {categoryId}";
      return result;
    }

    // 4. VALIDATE EACH VARIANT INDIVIDUALLY
    foreach (var variantGroup in variantGroups) {
      var variantValidation = await ValidateSingleVariantWithExistingLogicAsync(
        variantGroup.Key, 
        variantGroup.Value, 
        bulkUpdateDto, 
        repository,
        categoryId);

      if (!variantValidation.IsValid) {
        result.IsValid = false;
        result.ErrorMessage = $"Variant '{variantGroup.Key}' validation failed: {variantValidation.ErrorMessage}";
        return result;
      }
    }

    // 5. SUCCESS
    result.IsValid = true;
  }
  catch (Exception ex) {
    result.IsValid = false;
    result.ErrorMessage = $"Comprehensive category validation error: {ex.Message}";
  }

  return result;
}

/// <summary>
/// CATEGORY BUSINESS RULES VALIDATION (FIRST CHECK)
/// Uses existing enum system for restricted category validation
/// </summary>
private static async Task<BulkValidationResult> ValidateCategoryBusinessRulesAsync(
  int categoryId,
  IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
  IMedicalOptionRepository repository)
{
  var result = new BulkValidationResult();

  try {
    // Get category info
    var categoryInfo = await repository.GetCategoryByIdAsync(categoryId);
    if (categoryInfo == null) {
      result.IsValid = false;
      result.ErrorMessage = $"Category {categoryId} not found";
      return result;
    }

    // Use existing enum mapper to check restrictions
    var variantInfo = MedicalOptionEnumMapper.GetCategoryInfoFromVariant(categoryInfo.MedicalOptionCategoryName);
    var isRestricted = Enum.IsDefined(typeof(NoUpdateOnMedicalOptionCategory), categoryInfo.MedicalOptionCategoryName);
    
    if (isRestricted) {
      // Check for salary bracket updates in payload
      var salaryBracketUpdates = bulkUpdateDto
        .Where(dto => dto.SalaryBracketMin.HasValue || dto.SalaryBracketMax.HasValue)
        .ToList();

      if (salaryBracketUpdates.Count != 0) {
        // Get option names for error message
        var optionIds = salaryBracketUpdates.Select(dto => dto.MedicalOptionId).ToList();
        var dbOptions = await repository.GetMedicalOptionsByIdsAsync(optionIds);
        var optionNames = dbOptions.Select(o => o.MedicalOptionName).ToList();

        result.IsValid = false;
        result.ErrorMessage = $"Salary bracket updates are not allowed for {categoryInfo.MedicalOptionCategoryName} category. Options with attempted salary updates: [{string.Join(", ", optionNames)}]. {categoryInfo.MedicalOptionCategoryName} categories should only have minimum salary of 0 and unlimited maximum.";
        return result;
      }

      // Use existing validation method for each DTO
      foreach (var dto in bulkUpdateDto) {
        var dbOption = await repository.GetMedicalOptionByIdAsync(dto.MedicalOptionId);
        if (dbOption != null) {
          if (!ValidateSalaryBracketRestriction(categoryInfo.MedicalOptionCategoryName, dto.SalaryBracketMin, dto.SalaryBracketMax)) {
            result.IsValid = false;
            result.ErrorMessage = $"Salary bracket updates not allowed for {categoryInfo.MedicalOptionCategoryName} category: {dbOption.MedicalOptionName}";
            return result;
          }
        }
      }
    }
  }
  catch (Exception ex) {
    result.IsValid = false;
    result.ErrorMessage = $"Category business rules validation error: {ex.Message}";
  }

  return result;
}

/// <summary>
/// SINGLE VARIANT VALIDATION USING EXISTING LOGIC
/// </summary>
private static async Task<BulkValidationResult> ValidateSingleVariantWithExistingLogicAsync(
  string variantName,
  List<MedicalOption> variantOptions,
  IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
  IMedicalOptionRepository repository,
  int categoryId)
{
  var result = new BulkValidationResult();

  try {
    // Filter DTOs for this variant
    var variantUpdateDtos = bulkUpdateDto
      .Where(dto => variantOptions.Any(opt => opt.MedicalOptionId == dto.MedicalOptionId))
      .ToList();

    // 1. Entity count validation (existing method)
    if (!ValidateEntityCount(variantOptions.Count, variantUpdateDtos.Count)) {
      result.IsValid = false;
      result.ErrorMessage = $"Entity count mismatch for variant '{variantName}'. Expected: {variantOptions.Count}, Provided: {variantUpdateDtos.Count}";
      return result;
    }

    // 2. ID existence validation (existing method)
    if (!await ValidateAllIdsExistAsync(variantUpdateDtos, repository)) {
      result.IsValid = false;
      result.ErrorMessage = $"One or more medical option IDs do not exist in variant '{variantName}'";
      return result;
    }

    // 3. Category membership validation (existing method)
    if (!await ValidateAllIdsInCategoryAsync(variantUpdateDtos, categoryId, repository)) {
      result.IsValid = false;
      result.ErrorMessage = $"One or more medical option IDs do not belong to category {categoryId} in variant '{variantName}'";
      return result;
    }

    // 4. Individual contribution validation (existing method)
    foreach (var dto in variantUpdateDtos) {
      var dbOption = variantOptions.FirstOrDefault(o => o.MedicalOptionId == dto.MedicalOptionId);
      if (dbOption != null && !ValidateContributionValuesWithContext(dto, dbOption)) {
        result.IsValid = false;
        result.ErrorMessage = $"Invalid contribution values for option {dto.MedicalOptionId} ({dbOption.MedicalOptionName}) in variant '{variantName}'";
        return result;
      }
    }

    // 5. Salary bracket validation (existing methods)
    var salaryBracketRecords = new List<SalaryBracketValidatorRecord>();
    foreach (var dto in variantUpdateDtos) {
      var dbOption = variantOptions.FirstOrDefault(o => o.MedicalOptionId == dto.MedicalOptionId);
      if (dbOption != null && (dto.SalaryBracketMin.HasValue || dto.SalaryBracketMax.HasValue)) {
        salaryBracketRecords.Add(new SalaryBracketValidatorRecord(
          dto.MedicalOptionId, dbOption.MedicalOptionName, dto.SalaryBracketMin, dto.SalaryBracketMax));
      }
    }

    if (salaryBracketRecords.Count > 0) {
      if (!ValidateNoGapsInSalaryRanges(salaryBracketRecords)) {
        result.IsValid = false;
        result.ErrorMessage = $"Gaps detected in salary ranges for variant '{variantName}'";
        return result;
      }

      if (!ValidateNoOverlappingBrackets(salaryBracketRecords)) {
        result.IsValid = false;
        result.ErrorMessage = $"Overlapping salary brackets detected in variant '{variantName}'";
        return result;
      }
    }

    // 6. Contribution structure consistency within variant
    var structureValidation = ValidateVariantContributionStructure(variantOptions, variantName);
    if (!structureValidation.IsValid) {
      result.IsValid = false;
      result.ErrorMessage = structureValidation.ErrorMessage;
      return result;
    }

    // 7. Variant-specific business rules
    var variantRulesValidation = ValidateVariantSpecificBusinessRules(variantName, variantOptions);
    if (!variantRulesValidation.IsValid) {
      result.IsValid = false;
      result.ErrorMessage = variantRulesValidation.ErrorMessage;
      return result;
    }

    // 8. Network Choice specific child contribution rules
    if (variantName.Contains("Network Choice")) {
      var childContributionValidation = ValidateNetworkChoiceChildContributions(
        variantName, variantOptions, bulkUpdateDto);
        
      if (!childContributionValidation.IsValid) {
        result.IsValid = false;
        result.ErrorMessage = childContributionValidation.ErrorMessage;
        return result;
      }
    }
  }
  catch (Exception ex) {
    result.IsValid = false;
    result.ErrorMessage = $"Single variant validation error for '{variantName}': {ex.Message}";
  }

  return result;
}

/// <summary>
/// VARIANT-SPECIFIC BUSINESS RULES
/// Corrected for Network Choice (Risk + Principal, NO MSA)
/// </summary>
private static BulkValidationResult ValidateVariantSpecificBusinessRules(
  string variantName, 
  List<MedicalOption> variantOptions)
{
  var result = new BulkValidationResult();

  try {
    // Network Choice variants - Risk + Principal (NO MSA)
    if (variantName.Contains("Network Choice")) {
      var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
      var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

      if (hasMsa) {
        result.IsValid = false;
        result.ErrorMessage = $"Network Choice variant '{variantName}' should NOT have MSA contributions (Risk + Principal only)";
        return result;
      }

      if (!hasPrincipal) {
        result.IsValid = false;
        result.ErrorMessage = $"Network Choice variant '{variantName}' must have Principal contributions";
        return result;
      }
    }

    // First Choice variants - Risk only (no MSA, no Principal)
    else if (variantName.Contains("First Choice")) {
      var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
      var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

      if (hasMsa) {
        result.IsValid = false;
        result.ErrorMessage = $"First Choice variant '{variantName}' should NOT have MSA contributions (Risk only)";
        return result;
      }

      if (hasPrincipal) {
        result.IsValid = false;
        result.ErrorMessage = $"First Choice variant '{variantName}' should NOT have Principal contributions (Risk only)";
        return result;
      }
    }

    // Essential variants - Risk + MSA + Principal
    else if (variantName.Contains("Essential")) {
      var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
      var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

      if (!hasMsa) {
        result.IsValid = false;
        result.ErrorMessage = $"Essential variant '{variantName}' must have MSA contributions";
        return result;
      }

      if (!hasPrincipal) {
        result.IsValid = false;
        result.ErrorMessage = $"Essential variant '{variantName}' must have Principal contributions";
        return result;
      }
    }

    // Vital variants - Risk only (no MSA, no Principal)
    else if (variantName.Contains("Vital")) {
      var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
      var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

      if (hasMsa) {
        result.IsValid = false;
        result.ErrorMessage = $"Vital variant '{variantName}' should NOT have MSA contributions (Risk only)";
        return result;
      }

      if (hasPrincipal) {
        result.IsValid = false;
        result.ErrorMessage = $"Vital variant '{variantName}' should NOT have Principal contributions (Risk only)";
        return result;
      }
    }

    // Double variants - Risk + MSA (no Principal)
    else if (variantName.Contains("Double")) {
      var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
      var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

      if (!hasMsa) {
        result.IsValid = false;
        result.ErrorMessage = $"Double variant '{variantName}' must have MSA contributions";
        return result;
      }

      if (hasPrincipal) {
        result.IsValid = false;
        result.ErrorMessage = $"Double variant '{variantName}' should NOT have Principal contributions (Risk + MSA only)";
        return result;
      }
    }

    // Alliance variants - Risk + MSA (no Principal)
    else if (variantName.Contains("Alliance")) {
      var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
      var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

      if (!hasMsa) {
        result.IsValid = false;
        result.ErrorMessage = $"Alliance variant '{variantName}' must have MSA contributions";
        return result;
      }

      if (hasPrincipal) {
        result.IsValid = false;
        result.ErrorMessage = $"Alliance variant '{variantName}' should NOT have Principal contributions (Risk + MSA only)";
        return result;
      }
    }
  }
  catch (Exception ex) {
    result.IsValid = false;
    result.ErrorMessage = $"Variant-specific business rules validation error for '{variantName}': {ex.Message}";
  }

  return result;
}

/// <summary>
/// CORE CONTRIBUTION VALIDATION LOGIC
/// Updated for correct Network Choice handling (Risk + Principal, NO MSA)
/// </summary>
public static bool ValidateContributionValues(
  UpdateMedicalOptionVariantsDto entity,
  bool hasMsa,
  bool hasPrincipal)
{
  // Determine variant type based on contribution structure
  var isRiskOnlyVariant = !hasMsa && !hasPrincipal;           // First Choice, Vital
  var isRiskPlusPrincipalVariant = !hasMsa && hasPrincipal;     // Network Choice
  var isMsaOnlyVariant = hasMsa && !hasPrincipal;              // Alliance, Double
  var isMsaWithPrincipalVariant = hasMsa && hasPrincipal;      // Essential

  // Validate Risk contributions (all options must have these)
  if (!ValidateRiskContributions(entity, hasPrincipal))
    return false;

  // Apply validation logic based on variant type
  if (isRiskOnlyVariant) {
    // Risk-only variants: Risk should equal Total
    return ValidateRiskOnlyTotalContributions(entity);
  } else if (isRiskPlusPrincipalVariant) {
    // Risk + Principal variants: Risk should equal Total
    return ValidateRiskPlusPrincipalTotalContributions(entity);
  } else if (isMsaOnlyVariant) {
    // MSA-only variants: Risk + MSA should equal Total
    return ValidateMsaOnlyTotalContributions(entity);
  } else if (isMsaWithPrincipalVariant) {
    // MSA + Principal variants: Risk + MSA should equal Total
    return ValidateMsaWithPrincipalTotalContributions(entity);
  }

  return false; // Unknown variant type
}

/// <summary>
/// VALIDATION FOR RISK + PRINCIPAL VARIANTS (Network Choice)
/// Risk should equal Total (NO MSA validation)
/// </summary>
private static bool ValidateRiskPlusPrincipalTotalContributions(UpdateMedicalOptionVariantsDto entity)
{
  const decimal tolerance = 0.01m;

  // Adult: Risk should equal Total
  if (Math.Abs((decimal)(entity.MonthlyRiskContributionAdult - entity.TotalMonthlyContributionsAdult)) > tolerance)
    return false;

  // Child: Risk should equal Total
  if (Math.Abs((decimal)(entity.MonthlyRiskContributionChild - entity.TotalMonthlyContributionsChild)) > tolerance)
    return false;

  // Principal: Risk should equal Total (must be present)
  if (!entity.MonthlyRiskContributionPrincipal.HasValue || !entity.TotalMonthlyContributionsPrincipal.HasValue) {
    return false; // Principal contributions required for Network Choice
  }

  if (Math.Abs(entity.MonthlyRiskContributionPrincipal.Value - entity.TotalMonthlyContributionsPrincipal.Value) > tolerance)
    return false;

  // Child2: Risk should equal Total (if present)
  /*if (entity.MonthlyRiskContributionChild2.HasValue && entity.TotalMonthlyContributionsChild2.HasValue) {
    if (Math.Abs(entity.MonthlyRiskContributionChild2.Value - entity.TotalMonthlyContributionsChild2.Value) > tolerance)
      return false;
  }
*/
  return true;
}

/// <summary>
/// VALIDATION FOR RISK-ONLY VARIANTS (First Choice, Vital)
/// Risk = Total for Adult, Child, and Child2 (if present)
/// </summary>
private static bool ValidateRiskOnlyTotalContributions(UpdateMedicalOptionVariantsDto entity)
{
  const decimal tolerance = 0.01m;

  // Adult: Risk should equal Total
  if (Math.Abs((decimal)(entity.MonthlyRiskContributionAdult - entity.TotalMonthlyContributionsAdult)) > tolerance)
    return false;

  // Child: Risk should equal Total
  if (Math.Abs((decimal)(entity.MonthlyRiskContributionChild - entity.TotalMonthlyContributionsChild)) > tolerance)
    return false;

  // Child2: Risk should equal Total (if present)
  if (entity.MonthlyRiskContributionChild2.HasValue && entity.TotalMonthlyContributionsChild2.HasValue) {
    if (Math.Abs(entity.MonthlyRiskContributionChild2.Value - entity.TotalMonthlyContributionsChild2.Value) > tolerance)
      return false;
  }

  return true;
}

/// <summary>
/// VALIDATION FOR MSA-ONLY VARIANTS (Alliance, Double)
/// Risk + MSA should equal Total (no Principal validation)
/// </summary>
private static bool ValidateMsaOnlyTotalContributions(UpdateMedicalOptionVariantsDto entity)
{
  const decimal tolerance = 0.01m;

  // Adult: Risk + MSA should equal Total
  var adultTotal = entity.MonthlyRiskContributionAdult + entity.MonthlyMsaContributionAdult.Value;
  if (Math.Abs((decimal)(adultTotal - entity.TotalMonthlyContributionsAdult)) > tolerance)
    return false;

  // Child: Risk + MSA should equal Total
  var childTotal = entity.MonthlyRiskContributionChild + entity.MonthlyMsaContributionChild.Value;
  if (Math.Abs((decimal)(childTotal - entity.TotalMonthlyContributionsChild)) > tolerance)
    return false;

  // Child2: Risk + MSA should equal Total (if present)
  /*if (entity.MonthlyRiskContributionChild2.HasValue && entity.MonthlyMsaContributionChild2.HasValue) {
    var child2Total = entity.MonthlyRiskContributionChild2.Value + entity.MonthlyMsaContributionChild2.Value;
    if (entity.TotalMonthlyContributionsChild2.HasValue) {
      if (Math.Abs(child2Total - entity.TotalMonthlyContributionsChild2.Value) > tolerance)
        return false;
    }
  }
  */

  return true;
}

/// <summary>
/// VALIDATION FOR MSA + PRINCIPAL VARIANTS (Essential)
/// Risk + MSA should equal Total (including Principal validation)
/// </summary>
private static bool ValidateMsaWithPrincipalTotalContributions(UpdateMedicalOptionVariantsDto entity)
{
  const decimal tolerance = 0.01m;

  // Adult: Risk + MSA should equal Total
  var adultTotal = entity.MonthlyRiskContributionAdult + entity.MonthlyMsaContributionAdult.Value;
  if (Math.Abs((decimal)(adultTotal - entity.TotalMonthlyContributionsAdult)) > tolerance)
    return false;

  // Child: Risk + MSA should equal Total
  var childTotal = entity.MonthlyRiskContributionChild + entity.MonthlyMsaContributionChild.Value;
  if (Math.Abs((decimal)(childTotal - entity.TotalMonthlyContributionsChild)) > tolerance)
    return false;

  // Principal: Risk + MSA should equal Total (must be present)
  if (!entity.MonthlyRiskContributionPrincipal.HasValue || !entity.MonthlyMsaContributionPrincipal.HasValue || !entity.TotalMonthlyContributionsPrincipal.HasValue) {
    return false; // Principal contributions required for Essential
  }

  var principalTotal = entity.MonthlyRiskContributionPrincipal.Value + entity.MonthlyMsaContributionPrincipal.Value;
  if (Math.Abs(principalTotal - entity.TotalMonthlyContributionsPrincipal.Value) > tolerance)
    return false;

  // Child2: Risk + MSA should equal Total (if present)
 /* if (entity.MonthlyRiskContributionChild2.HasValue && entity.MonthlyMsaContributionChild2.HasValue) {
    var child2Total = entity.MonthlyRiskContributionChild2.Value + entity.MonthlyMsaContributionChild2.Value;
    if (entity.TotalMonthlyContributionsChild2.HasValue) {
      if (Math.Abs(child2Total - entity.TotalMonthlyContributionsChild2.Value) > tolerance)
        return false; 
    }
  }
*/
  return true;
}

/// <summary>
/// NETWORK CHOICE CHILD CONTRIBUTION VALIDATION
/// Business Rules: Options 1-3: Child2 = R0 (free), Options 4-5: Child2 = Child (same billing)
/// </summary>
private static BulkValidationResult ValidateNetworkChoiceChildContributions(
  string variantName,
  List<MedicalOption> variantOptions,
  IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
{
  var result = new BulkValidationResult();

  try {
    // Only apply to Network Choice variants
    if (!variantName.Contains("Network Choice")) {
      return result; // Not applicable, pass validation
    }

    // Sort options by name to identify 1-5 sequence
    var sortedOptions = variantOptions.OrderBy(o => o.MedicalOptionName).ToList();

    for (int i = 0; i < sortedOptions.Count; i++) {
      var option = sortedOptions[i];
      var optionNumber = i + 1; // 1-based index
      var correspondingDto = bulkUpdateDto.FirstOrDefault(dto => dto.MedicalOptionId == option.MedicalOptionId);

      if (correspondingDto == null) continue; // Skip if not being updated

      // RULE: Network Choice 1-3 - Child2 should be FREE (0)
      if (optionNumber <= 3) {
        if (correspondingDto.MonthlyRiskContributionChild2.HasValue && 
            correspondingDto.MonthlyRiskContributionChild2.Value != 0) {
          
          result.IsValid = false;
          result.ErrorMessage = $"Network Choice {optionNumber} ({option.MedicalOptionName}) must have Child2 contribution of R0 (free). Current: R{correspondingDto.MonthlyRiskContributionChild2.Value:N2}";
          return result;
        }
      }

      // RULE: Network Choice 4-5 - Child2 should equal Child (same billing)
      else if (optionNumber >= 4) {
        var childContribution = correspondingDto.MonthlyRiskContributionChild;
        var child2Contribution = correspondingDto.MonthlyRiskContributionChild2;

        if (child2Contribution.HasValue && childContribution != child2Contribution.Value) {
          result.IsValid = false;
          result.ErrorMessage = $"Network Choice {optionNumber} ({option.MedicalOptionName}) must have Child2 contribution equal to Child contribution. Child: R{childContribution:N2}, Child2: R{child2Contribution.Value:N2}";
          return result;
        }

        if (!child2Contribution.HasValue) {
          result.IsValid = false;
          result.ErrorMessage = $"Network Choice {optionNumber} ({option.MedicalOptionName}) must have Child2 contribution specified (should equal Child contribution: R{childContribution:N2})";
          return result;
        }
      }
    }
  }
  catch (Exception ex) {
    result.IsValid = false;
    result.ErrorMessage = $"Network Choice child contribution validation error: {ex.Message}";
  }

  return result;
}

/// <summary>
/// CONTRIBUTION STRUCTURE CONSISTENCY VALIDATION
/// </summary>
private static BulkValidationResult ValidateVariantContributionStructure(
  List<MedicalOption> variantOptions, 
  string variantName)
{
  var result = new BulkValidationResult();

  try {
    if (variantOptions.Count < 2) return result; // Not applicable for single option

    // Check MSA structure consistency
    var msaStructures = variantOptions
      .Select(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0)
      .Distinct()
      .ToList();

    if (msaStructures.Count > 1) {
      var optionsWithMsa = variantOptions
        .Where(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0)
        .Select(o => o.MedicalOptionName);
      
      var optionsWithoutMsa = variantOptions
        .Where(o => !o.MonthlyMsaContributionAdult.HasValue || o.MonthlyMsaContributionAdult <= 0)
        .Select(o => o.MedicalOptionName);

      result.IsValid = false;
      result.ErrorMessage = $"Inconsistent MSA structure in variant '{variantName}'. With MSA: [{string.Join(", ", optionsWithMsa)}]. Without MSA: [{string.Join(", ", optionsWithoutMsa)}]";
      return result;
    }

    // Check Principal structure consistency
    var principalStructures = variantOptions
      .Select(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0)
      .Distinct()
      .ToList();

    if (principalStructures.Count > 1) {
      var optionsWithPrincipal = variantOptions
        .Where(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0)
        .Select(o => o.MedicalOptionName);
      
      var optionsWithoutPrincipal = variantOptions
        .Where(o => !o.MonthlyRiskContributionPrincipal.HasValue || o.MonthlyRiskContributionPrincipal <= 0)
        .Select(o => o.MedicalOptionName);

      result.IsValid = false;
      result.ErrorMessage = $"Inconsistent Principal structure in variant '{variantName}'. With Principal: [{string.Join(", ", optionsWithPrincipal)}]. Without Principal: [{string.Join(", ", optionsWithoutPrincipal)}]";
      return result;
    }
  }
  catch (Exception ex) {
    result.IsValid = false;
    result.ErrorMessage = $"Contribution structure validation error for variant '{variantName}': {ex.Message}";
  }

  return result;
}

/// <summary>
/// GROUP OPTIONS BY VARIANT using existing factory system
/// </summary>
public static Dictionary<string, List<MedicalOption>> GroupOptionsByVariant(List<MedicalOption> options)
{
  var variantGroups = new Dictionary<string, List<MedicalOption>>();
  
  foreach (var option in options) {
    var variantInfo = MedicalOptionVariantFactory.GetVariantInfoSafe(option);
    var variantKey = variantInfo.CategoryName;
    
    if (!string.IsNullOrEmpty(variantKey)) {
      if (!variantGroups.TryGetValue(variantKey, out List<MedicalOption>? value)) {
        value = new List<MedicalOption>();
        variantGroups[variantKey] = value;
      }
      value.Add(option);
    }
  }
  
  return variantGroups;
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