namespace HRConnect.Api.Utils.MedicalOption
{
  using DTOs.MedicalOption;
  using Enums;
  using Enums.Mappers;
  using Factories;
  using Interfaces;
  using Models;
  using Records;

  /// <summary>
  /// Comprehensive static validator for medical option operations, providing extensive validation
  /// logic for bulk updates, salary brackets, contribution structures, and business rule enforcement.
  /// This class serves as the central validation hub for all medical option modifications,
  /// ensuring data integrity, compliance with business rules, and regulatory requirements.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Architecture Overview:</strong>
  /// This static utility class implements a multi-layered validation strategy with specialized
  /// validation methods for different aspects of medical option management. It follows the
  /// Single Responsibility Principle by organizing validation logic into distinct regions
  /// based on validation type and complexity.
  /// </para>
  /// 
  /// <para>
  /// <strong>Key Validation Areas:</strong>
  /// - Basic validations (period, entity counts, ID existence)
  /// - Salary bracket validations (ranges, overlaps, gaps, restrictions)
  /// - Contribution validations (risk, MSA, total contributions)
  /// - Variant-specific business rules (Network Choice, First Choice, Essential, Double, Alliance)
  /// - Comprehensive category validations with cross-variant consistency checks
  /// </para>
  /// 
  /// <para>
  /// <strong>Business Context:</strong>
  /// Medical option validation is critical for ensuring:
  /// - Compliance with regulatory requirements and actuarial standards
  /// - Consistent pricing structures across benefit categories
  /// - Proper salary bracket definitions for income-based pricing
  /// - Adherence to update period restrictions (November-December)
  /// - Variant-specific contribution structure requirements
  /// </para>
  /// 
  /// <para>
  /// <strong>Validation Strategy:</strong>
  /// The validator employs a fail-fast approach, stopping at the first validation failure
  /// to provide immediate feedback. Each validation method returns a BulkValidationResult
  /// containing success status and detailed error messages for debugging and user feedback.
  /// </para>
  /// 
  /// <para>
  /// <strong>Performance Considerations:</strong>
  /// - In-memory operations for optimal performance during bulk validations
  /// - Efficient data structures (HashSet, Dictionary) for lookups and grouping
  /// - Minimal database calls through repository pattern usage
  /// - Optimized for concurrent validation scenarios
  /// </para>
  /// 
  /// <para>
  /// <strong>Error Handling:</strong>
  /// Comprehensive exception handling with detailed error reporting. All validation methods
  /// wrap operations in try-catch blocks to ensure graceful error handling and meaningful
  /// error messages for debugging and user feedback.
  /// </para>
  /// 
  /// <para>
  /// <strong>Testing Support:</strong>
  /// Many methods support optional testDate parameters for unit testing scenarios,
  /// enabling deterministic validation behavior regardless of current system time.
  /// </para>
  /// </remarks>

  public static class MedicalOptionValidator
  {
    /// <summary>
    /// Global variable storing the current category name during validation operations.
    /// This variable provides context for error messages and debugging across validation methods.
    /// </summary>
    /// <remarks>
    /// Used to maintain validation context across multiple method calls within the same
    /// validation workflow, enabling consistent error reporting and audit trail generation.
    /// </remarks>
    private static string _globalCategoryName = string.Empty;

    //Helper methods

    #region Basic Validations

    /// <summary>
    /// Validates if the update operation is within the allowed period (November-December).
    /// This method enforces business rules that restrict medical option updates to specific
    /// time windows, ensuring compliance with annual enrollment and policy update cycles.
    /// </summary>
    /// <param name="date">The date to validate against the allowed update period.</param>
    /// <returns>True if the date falls within the allowed update period; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Business Rule:</strong>
    /// Medical option updates are only permitted between November 1st and December 31st
    /// to align with annual benefits enrollment periods and ensure stable pricing throughout the year.
    /// </para>
    /// 
    /// <para>
    /// <strong>Usage Context:</strong>
    /// This validation is typically the first check performed in any bulk update operation
    /// to ensure that updates are only performed during authorized business periods.
    /// </para>
    /// 
    /// <para>
    /// <strong>Time Zone Considerations:</strong>
    /// The validation uses the local time zone of the executing environment. For production
    /// deployments, consider using UTC to ensure consistency across different server locations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check if current date allows updates
    /// var canUpdate = MedicalOptionValidator.ValidateUpdatePeriod(DateTime.Now);
    /// if (canUpdate)
    /// {
    ///     Console.WriteLine("Updates are currently allowed");
    ///     // Proceed with update operations
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Updates are not allowed at this time");
    ///     // Block update operations or show appropriate message
    /// }
    /// 
    /// // Test specific dates
    /// var novemberDate = new DateTime(2024, 11, 15);
    /// var marchDate = new DateTime(2024, 3, 15);
    /// 
    /// Console.WriteLine($"November 15: {MedicalOptionValidator.ValidateUpdatePeriod(novemberDate)}"); // True
    /// Console.WriteLine($"March 15: {MedicalOptionValidator.ValidateUpdatePeriod(marchDate)}"); // False
    /// </code>
    /// </example>
    public static bool ValidateUpdatePeriod(DateTime date)
    {
      return DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(date);
    }

    /// <summary>
    /// Validates that the entity count matches between database records and payload data.
    /// This method ensures data consistency by verifying that the number of medical options
    /// being updated matches the expected count from the database.
    /// </summary>
    /// <param name="dbEntityCount">The number of entities retrieved from the database.</param>
    /// <param name="bulkPayloadCount">The number of entities in the update payload.</param>
    /// <returns>True if the counts match; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong>
    /// This validation prevents data integrity issues by ensuring that bulk update operations
    /// include all expected entities and that no entities are missing or duplicated.
    /// </para>
    /// 
    /// <para>
    /// <strong>Usage Context:</strong>
    /// Typically used as an early validation step in bulk update operations to verify
    /// that the payload data aligns with the current database state.
    /// </para>
    /// 
    /// <para>
    /// <strong>Error Scenarios:</strong>
    /// - Mismatch indicates potential data synchronization issues
    /// - May occur if entities were added/removed between payload creation and update
    /// - Could indicate incomplete payload data or database inconsistencies
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After retrieving database data and receiving payload
    /// var dbCount = databaseOptions.Count;
    /// var payloadCount = updatePayload.Count;
    /// 
    /// if (MedicalOptionValidator.ValidateEntityCount(dbCount, payloadCount))
    /// {
    ///     Console.WriteLine("Entity counts match, proceeding with validation");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Entity count mismatch: DB={dbCount}, Payload={payloadCount}");
    ///     throw new ValidationException("Entity count mismatch detected");
    /// }
    /// </code>
    /// </example>
    public static bool ValidateEntityCount(int dbEntityCount, int bulkPayloadCount)
    {
      return dbEntityCount == bulkPayloadCount;
    }

    /// <summary>
    /// Validates that all medical option IDs in the payload exist in the database.
    /// This method ensures data integrity by verifying that every option being updated
    /// has a corresponding record in the database.
    /// </summary>
    /// <param name="bulkUpdateDto">Collection of update DTOs containing medical option IDs to validate.</param>
    /// <param name="repository">Repository for data access operations (used for additional validation if needed).</param>
    /// <param name="dbData">List of existing medical options from the database for in-memory validation.</param>
    /// <returns>True if all IDs exist in the database; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Performance Optimization:</strong>
    /// This method uses in-memory validation with HashSet for O(1) lookup performance,
    /// avoiding additional database calls during bulk operations.
    /// </para>
    /// 
    /// <para>
    /// <strong>Validation Strategy:</strong>
    /// - Creates a HashSet of existing IDs from database data
    /// - Iterates through payload IDs checking existence in the HashSet
    /// - Returns false immediately upon finding any missing ID (fail-fast approach)
    /// </para>
    /// 
    /// <para>
    /// <strong>Error Prevention:</strong>
    /// Prevents attempts to update non-existent entities, which would result
    /// in database errors and data inconsistency.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate all IDs in payload exist in database
    /// var payload = new List&​lt;UpdateMedicalOptionVariantsDto&​gt;
    /// {
    ///     new() { MedicalOptionId = 1, /* other properties */ },
    ///     new() { MedicalOptionId = 2, /* other properties */ },
    ///     new() { MedicalOptionId = 3, /* other properties */ }
    /// };
    /// 
    /// var dbOptions = await repository.GetAllOptionsUnderCategoryAsync(5);
    /// 
    /// var allIdsExist = await MedicalOptionValidator.ValidateAllIdsExistAsync(
    ///     payload, repository, dbOptions);
    ///     
    /// if (allIdsExist)
    /// {
    ///     Console.WriteLine("All payload IDs exist in database");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Some payload IDs do not exist in database");
    ///     throw new ValidationException("Invalid medical option IDs detected");
    /// }
    /// </code>
    /// </example>
    public static bool ValidateAllIdsExistAsync(
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var existingIds = dbData.Select(o => o.MedicalOptionId).ToHashSet();

      foreach (var entity in bulkUpdateDto)
      {
        if (!existingIds.Contains(entity.MedicalOptionId))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Validates that all entities in the payload belong to the specified category.
    /// This method ensures data integrity by verifying that medical options being updated
    /// are correctly categorized and belong to the expected category.
    /// </summary>
    /// <param name="bulkUpdateDto">Collection of update DTOs to validate.</param>
    /// <param name="categoryId">The category ID that all entities should belong to.</param>
    /// <param name="repository">Repository for data access operations.</param>
    /// <param name="dbData">List of existing medical options from the database.</param>
    /// <returns>True if all entities belong to the specified category; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Business Rule Enforcement:</strong>
    /// Ensures that bulk update operations only affect entities within the intended
    /// category, preventing cross-category data contamination.
    /// </para>
    /// 
    /// <para>
    /// <strong>Security Consideration:</strong>
    /// This validation prevents unauthorized category modifications by ensuring
    /// that update operations are scoped to the correct category boundary.
    /// </para>
    /// 
    /// <para>
    /// <strong>Data Integrity:</strong>
    /// Maintains categorical data integrity by preventing operations that could
    /// misclassify or misplace medical options across different benefit categories.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate all payload entities belong to category 5
    /// var categoryId = 5;
    /// var payload = new List&​lt;UpdateMedicalOptionVariantsDto&​gt;
    /// {
    ///     new() { MedicalOptionId = 1, /* other properties */ },
    ///     new() { MedicalOptionId = 2, /* other properties */ }
    /// };
    /// 
    /// var dbOptions = await repository.GetAllOptionsUnderCategoryAsync(categoryId);
    /// 
    /// var allInCategory = await MedicalOptionValidator.ValidateAllIdsInCategoryAsync(
    ///     payload, categoryId, repository, dbOptions);
    ///     
    /// if (allInCategory)
    /// {
    ///     Console.WriteLine($"All entities belong to category {categoryId}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Some entities do not belong to the specified category");
    ///     throw new ValidationException($"Invalid category membership detected");
    /// }
    /// </code>
    /// </example>
    public static bool ValidateAllIdsInCategoryAsync(
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      int categoryId,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var categoryOptions = dbData
        .Where(o => o.MedicalOptionCategoryId == categoryId).ToHashSet();

      foreach (var entity in bulkUpdateDto)
      {
        if (!categoryOptions.Any(o => o.MedicalOptionId == entity.MedicalOptionId))
          return false;
      }

      return true;
    }

    #endregion

    #region Salary Bracket Validations

    /// <summary>
    /// Validates salary bracket restrictions for Alliance and Double categories.
    /// This method enforces business rules that prevent salary bracket updates for
    /// specific categories that have fixed pricing structures.
    /// </summary>
    /// <param name="categoryName">The name of the medical option category.</param>
    /// <param name="salaryMin">The minimum salary bracket value being updated.</param>
    /// <param name="salaryMax">The maximum salary bracket value being updated.</param>
    /// <returns>True if the update complies with restrictions; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Business Rule:</strong>
    /// Alliance and Double categories have special business rules that prevent
    /// salary bracket modifications because these categories typically have
    /// fixed pricing structures that don't vary based on income levels.
    /// </para>
    /// 
    /// <para>
    /// <strong>Restricted Categories:</strong>
    /// - Alliance: Entry-level category with no salary bracket pricing
    /// - Double: Specialized category with fixed pricing structure
    /// </para>
    /// 
    /// <para>
    /// <strong>Validation Logic:</strong>
    /// - Checks if category is in restricted list
    /// - Validates that no salary bracket updates are being attempted
    /// - Returns false if restricted category has salary updates
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Test restricted category with salary updates
    /// var isAllowed = MedicalOptionValidator.ValidateSalaryBracketRestriction(
    ///     "Alliance", 5000, 15000);
    /// Console.WriteLine($"Alliance with salary updates allowed: {isAllowed}"); // False
    /// 
    /// // Test restricted category without salary updates
    /// var isAllowed2 = MedicalOptionValidator.ValidateSalaryBracketRestriction(
    ///     "Alliance", null, null);
    /// Console.WriteLine($"Alliance without salary updates allowed: {isAllowed2}"); // True
    /// 
    /// // Test unrestricted category
    /// var isAllowed3 = MedicalOptionValidator.ValidateSalaryBracketRestriction(
    ///     "Choice", 5000, 15000);
    /// Console.WriteLine($"Choice with salary updates allowed: {isAllowed3}"); // True
    /// </code>
    /// </example>
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
    /// Validates that there are no gaps between consecutive salary ranges.
    /// This method ensures continuous coverage across salary brackets without
    /// uncovered income ranges that would leave employees without eligible options.
    /// </summary>
    /// <param name="records">List of salary bracket records to validate for gaps.</param>
    /// <returns>True if there are no gaps between salary ranges; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Business Requirement:</strong>
    /// Continuous salary bracket coverage ensures that all employees, regardless
    /// of income level, have access to appropriate medical benefit options.
    /// </para>
    /// 
    /// <para>
    /// <strong>Gap Tolerance:</strong>
    /// Allows for small gaps (up to 1.00) between ranges to account for
    /// rounding and practical implementation considerations.
    /// </para>
    /// 
    /// <para>
    /// <strong>Validation Logic:</strong>
    /// - Sorts records by minimum salary
    /// - Checks that next minimum is within acceptable range of current maximum
    /// - Allows 0.01 + 0.99 tolerance for practical implementation
    /// - Handles uncapped brackets (null maximum) appropriately
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var records = new List&​lt;SalaryBracketValidatorRecord&​gt;
    /// {
    ///     new(1, "Plan A", 0, 10000),
    ///     new(2, "Plan B", 10001, 20000),
    ///     new(3, "Plan C", 20001, null) // Uncapped
    /// };
    /// 
    /// var hasNoGaps = MedicalOptionValidator.ValidateNoGapsInSalaryRanges(records);
    /// Console.WriteLine($"No gaps in salary ranges: {hasNoGaps}"); // True
    /// 
    /// // Example with gap
    /// var recordsWithGap = new List&​lt;SalaryBracketValidatorRecord&​gt;
    /// {
    ///     new(1, "Plan A", 0, 10000),
    ///     new(2, "Plan B", 10500, 20000) // Gap of 500
    /// };
    /// 
    /// var hasNoGaps2 = MedicalOptionValidator.ValidateNoGapsInSalaryRanges(recordsWithGap);
    /// Console.WriteLine($"No gaps with gap example: {hasNoGaps2}"); // False
    /// </code>
    /// </example>
    public static bool ValidateNoGapsInSalaryRanges(
      List<SalaryBracketValidatorRecord> records)
    {
      var sortedRecords = records
        .Where(r => r.salaryBracketMin.HasValue &&
                    (r.salaryBracketMax.HasValue || r.salaryBracketMax is null))
        .OrderBy(r => r.salaryBracketMin)
        .ToList();

      for (int i = 0; i < sortedRecords.Count - 1; i++)
      {
        var current = sortedRecords[i];
        var next = sortedRecords[i + 1];

        if (current.salaryBracketMax != null)
        {
          var expectedNextMin = current.salaryBracketMax.Value + 0.01m;
          if (next.salaryBracketMin > expectedNextMin + 0.99m)
            return false;
        }
        else
        {
          return true; // when current Max is null (uncapped)
        }
      }

      return true;
    }

    /// <summary>
    /// Validates that there are no overlapping salary brackets within the same category.
    /// This method ensures that salary ranges are mutually exclusive, preventing
    /// eligibility ambiguity and ensuring clear income-based option assignment.
    /// </summary>
    /// <param name="records">List of salary bracket records to validate for overlaps.</param>
    /// <returns>True if there are no overlapping brackets; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Business Requirement:</strong>
    /// Non-overlapping salary brackets ensure that each employee falls into
    /// exactly one income tier, preventing eligibility ambiguity and ensuring
    /// consistent pricing structures.
    /// </para>
    /// 
    /// <para>
    /// <strong>Overlap Definition:</strong>
    /// Overlap occurs when the minimum of one bracket is less than or equal
    /// to the maximum of the preceding bracket.
    /// </para>
    /// 
    /// <para>
    /// <strong>Edge Cases:</strong>
    /// - Handles null minimum (starts from 0) and null maximum (uncapped)
    /// - Properly sorts records for sequential validation
    /// - Considers boundary conditions for precise validation
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid non-overlapping ranges
    /// var validRecords = new List&​lt;SalaryBracketValidatorRecord&​gt;
    /// {
    ///     new(1, "Plan A", 0, 10000),
    ///     new(2, "Plan B", 10001, 20000),
    ///     new(3, "Plan C", 20001, 30000)
    /// };
    /// 
    /// var noOverlap = MedicalOptionValidator.ValidateNoOverlappingBrackets(validRecords);
    /// Console.WriteLine($"No overlap: {noOverlap}"); // True
    /// 
    /// // Overlapping ranges
    /// var overlappingRecords = new List&​lt;SalaryBracketValidatorRecord&​gt;
    /// {
    ///     new(1, "Plan A", 0, 10000),
    ///     new(2, "Plan B", 9500, 20000) // Overlaps with Plan A
    /// };
    /// 
    /// var noOverlap2 = MedicalOptionValidator.ValidateNoOverlappingBrackets(overlappingRecords);
    /// Console.WriteLine($"No overlap with overlapping example: {noOverlap2}"); // False
    /// </code>
    /// </example>
    public static bool ValidateNoOverlappingBrackets(
      List<SalaryBracketValidatorRecord> records)
    {
      var sortedRecords = records
        .Where(r => (r.salaryBracketMin.HasValue || r.salaryBracketMin >= 0)
                    && (r.salaryBracketMax.HasValue || r.salaryBracketMax is null))
        .OrderBy(r => r.salaryBracketMin)
        .ToList();

      for (int i = 0; i < sortedRecords.Count - 1; i++)
      {
        var current = sortedRecords[i];
        var next = sortedRecords[i + 1];

        if (current.salaryBracketMax.HasValue && next.salaryBracketMin.HasValue ||
            !(next is null))
        {
          if (next.salaryBracketMin <= current.salaryBracketMax.Value)
            return false;
        }
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
                   dbOption.MonthlyMsaContributionAdult >= 0;
      var hasPrincipal = dbOption.MonthlyRiskContributionPrincipal.HasValue &&
                         (dbOption.MonthlyRiskContributionPrincipal >= 0 &&
                          dbOption.MonthlyRiskContributionPrincipal is not null);

      return ValidateContributionValues(entity, hasMsa, hasPrincipal).IsValid;
    }

    /// <summary>
    /// Validates salary bracket gaps and overlaps within a single variant
    /// </summary>
    private static BulkValidationResult ValidateSingleVariantSalaryBrackets(
      List<MedicalOption> variantOptions)
    {
      var result = new BulkValidationResult() { IsValid = true };

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
      // Adult and Child risk contributions must be >= 0
      if (entity.MonthlyRiskContributionAdult < 0 ||
          entity.MonthlyRiskContributionChild < 0)
        return false;

      // Principal validation (if applicable)
      if (hasPrincipal &&
          (!entity.MonthlyRiskContributionPrincipal.HasValue ||
           entity.MonthlyRiskContributionPrincipal < 0 ||
           entity.MonthlyRiskContributionPrincipal is null))
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

        if (Math.Abs((decimal)(adultTotal
                               - entity.TotalMonthlyContributionsAdult)) > tolerance)
          return false;

        if (Math.Abs((decimal)(childTotal
                               - entity.TotalMonthlyContributionsChild)) > tolerance)
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
        if (Math.Abs((decimal)(entity.MonthlyRiskContributionAdult
                               - entity.TotalMonthlyContributionsAdult)) >
            tolerance)
          return false;

        if (Math.Abs((decimal)(entity.MonthlyRiskContributionChild
                               - entity.TotalMonthlyContributionsChild)) >
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
    public static BulkValidationResult ValidateBulkUpdateAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult { IsValid = true };
      var salaryBracketRecords = new List<SalaryBracketValidatorRecord>();

      try
      {
        // 1. Update Period Validation
        if (!ValidateUpdatePeriod(DateTime.Now))
        {
          result.IsValid = false;
          result.ErrorMessage = "Bulk update operation cannot be executed outside " +
                                "the update period";
          return result;
        }

        // 2. Entity Count Validation
        if (!ValidateEntityCount(dbData.Count, bulkUpdateDto.Count))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options not found in the " +
                                "specified category";
          return result;
        }

        // 3. ID Validations
        if (!ValidateAllIdsExistAsync(bulkUpdateDto, repository, dbData))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical options are invalid";
          return result;
        }

        if (!ValidateAllIdsInCategoryAsync(bulkUpdateDto, categoryId, repository, dbData))
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
          var restricted = ValidateSalaryBracketRestriction(categoryName,
            entity.SalaryBracketMin, entity.SalaryBracketMax);

          if (!restricted)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Salary bracket cannot be updated for category" +
                                  $" '{categoryName}'";
            return result;
          }

          // Contribution validation
          if (!ValidateContributionValuesWithContext(entity, dbOption))
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
        if (!ValidateUpdatePeriod(DateTime.Now))
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
        var variantGroups = GroupOptionsByVariant(bulkUpdateDto, dbData);

        // Validate each variant individually with ALL validations
        foreach (var variantGroup in variantGroups)
        {
          var variantValidation = await ValidateSingleVariantWithAllRulesAsync(
            variantGroup.Key,
            variantGroup.Value,
            bulkUpdateDto,
            repository,
            categoryId, dbData);

          if (!variantValidation.IsValid)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Variant '{variantGroup.Key}' validation failed: " +
                                  $"{variantValidation.ErrorMessage}";
            return result;
          }
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

        if (!ValidateEntityCount(variantOptions.Count, variantUpdateDtos.Count))
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
          if (dbOption != null && !ValidateContributionValuesWithContext(dto, dbOption))
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

        var salaryValidation = ValidateSingleVariantSalaryBrackets(dbVariantOptions);
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
        var variantRulesValidation = ValidateVariantSpecificBusinessRules(variantName, variantOptions,
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
      var result = new BulkValidationResult() { IsValid = true };

      try
      {
        if (variantOptions.Count < 2) return result; // Not applicable for single option

        // Get first option for expected structure
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
            result.ErrorMessage = $"Inconsistent MSA structure within variant: Option " +
                                  $"{firstOption.MedicalOptionId} has MSA, but Option " +
                                  $"{option.MedicalOptionId} does not";
            return result;
          }

          if (expectedHasPrincipal != actualHasPrincipal)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Inconsistent Principal structure within variant: Option " +
                                  $"{firstOption.MedicalOptionId} has Principal, but Option " +
                                  $"{option.MedicalOptionId} does not";
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

    #endregion

    #region Cross-Variant Validation
    private static BulkValidationResult ValidateCrossVariantConsistency(
        Dictionary<string, List<UpdateMedicalOptionVariantsDto>> variantGroups)
    {
      var result = new BulkValidationResult { IsValid = true };
      var variants = variantGroups.ToList();

      // If there is only one variant, skip cross-variant checks but still validate within variant
      if (variantGroups.Count < 2)
      {
        // Still validate consistency within the single variant
        foreach (var variant in variants)
        {
          var withinVariantResult = ValidateWithinVariantConsistency(variant.Value, variant.Key);
          if (!withinVariantResult.IsValid)
          {
            return withinVariantResult;
          }
        }
        return result; // Valid - no other variants to compare against
      }

      // Step 1: Validate consistency within each variant first
      foreach (var variant in variants)
      {
        var withinVariantResult = ValidateWithinVariantConsistency(variant.Value, variant.Key);
        if (!withinVariantResult.IsValid)
        {
          return withinVariantResult;
        }
      }

      // Step 2: Cross-variant validation - compare different variants
      foreach (var currentVariant in variants)
      {
        foreach (var otherVariant in variants)
        {
          // Skip self-comparison
          if (currentVariant.Key == otherVariant.Key) continue;

          // Check if contribution amounts are identical across all business rules
          if (AreContributionAmountsIdentical(currentVariant.Value, otherVariant.Value))
          {
            result.IsValid = false;
            result.ErrorMessage =
                $"Variant '{currentVariant.Key}' has identical contribution amounts as variant" +
                $" '{otherVariant.Key}' - variants must have distinct contribution structures";
            return result;
          }
        }
      }

      return result;
    }

    // New method to validate consistency within a single variant
    private static BulkValidationResult ValidateWithinVariantConsistency(
        List<UpdateMedicalOptionVariantsDto> variantOptions, string variantName)
    {
      var result = new BulkValidationResult { IsValid = true };

      if (variantOptions.Count < 2)
      {
        return result; // Single option is always consistent
      }

      // Sort options by MedicalOptionId for consistent comparison
      var sortedOptions = variantOptions
        .OrderBy(o => o.MedicalOptionId).ToList();

      // Check for consistent contribution structure within the variant
      var firstOption = sortedOptions.First();

      for (int i = 1; i < sortedOptions.Count; i++)
      {
        var currentOption = sortedOptions[i];

        // Validate that the contribution structure is consistent
        // if first option has MSA, all options should have MSA
        var firstHasMsa = firstOption.MonthlyMsaContributionAdult.HasValue;
        var currentHasMsa = currentOption.MonthlyMsaContributionAdult.HasValue;

        if (firstHasMsa != currentHasMsa)
        {
          result.IsValid = false;
          result.ErrorMessage =
              $"Inconsistent MSA structure in variant '{variantName}': Option " +
              $"{firstOption.MedicalOptionId} has MSA, but Option " +
              $"{currentOption.MedicalOptionId} does not";
          return result;
        }

        var firstHasPrincipal = firstOption.MonthlyRiskContributionPrincipal.HasValue;
        var currentHasPrincipal = currentOption.MonthlyRiskContributionPrincipal.HasValue;

        if (firstHasPrincipal != currentHasPrincipal)
        {
          result.IsValid = false;
          result.ErrorMessage =
              $"Inconsistent Principal structure in variant '{variantName}': Option " +
              $"{firstOption.MedicalOptionId} has Principal, but Option " +
              $"{currentOption.MedicalOptionId} does not";
          return result;
        }
      }

      return result;
    }
    private static bool AreContributionAmountsIdentical(
      List<UpdateMedicalOptionVariantsDto> variant1, List<UpdateMedicalOptionVariantsDto> variant2)
    {
      // Sort options by MedicalOptionId to ensure proper comparison
      var sortedVariant1 = variant1
        .OrderBy(o => o.MedicalOptionId).ToList();
      var sortedVariant2 = variant2
        .OrderBy(o => o.MedicalOptionId).ToList();

      // Check if same number of options
      if (sortedVariant1.Count != sortedVariant2.Count) return false;

      for (int i = 0; i < sortedVariant1.Count; i++)
      {
        var opt1 = sortedVariant1[i];
        var opt2 = sortedVariant2[i];

        // Compare RISK contributions (Principal + Adult + Child + Child2)
        if (!CompareRiskContributions(opt1, opt2))
          return false;

        // Compare MSA contributions (Principal + Adult + Child + Child2) 
        if (!CompareMsaContributions(opt1, opt2))
          return false;

        // Compare TOTAL contributions
        if (!CompareTotalContributions(opt1, opt2))
          return false;
      }

      return true; // All amounts are identical
    }

    private static bool CompareRiskContributions(UpdateMedicalOptionVariantsDto opt1,
      UpdateMedicalOptionVariantsDto opt2)
    {
      // Principal Risk
      if (opt1.MonthlyRiskContributionPrincipal != opt2.MonthlyRiskContributionPrincipal)
        return false;

      // Adult Risk
      if (opt1.MonthlyRiskContributionAdult != opt2.MonthlyRiskContributionAdult)
        return false;

      // Child Risk
      if (opt1.MonthlyRiskContributionChild != opt2.MonthlyRiskContributionChild)
        return false;

      // Child2 Risk (if exists)
      if (opt1.MonthlyRiskContributionChild2 != opt2.MonthlyRiskContributionChild2)
        return false;

      return true;
    }

    private static bool CompareMsaContributions(UpdateMedicalOptionVariantsDto opt1,
      UpdateMedicalOptionVariantsDto opt2)
    {
      // Principal MSA
      if (opt1.MonthlyMsaContributionPrincipal != opt2.MonthlyMsaContributionPrincipal)
        return false;

      // Adult MSA
      if (opt1.MonthlyMsaContributionAdult != opt2.MonthlyMsaContributionAdult)
        return false;

      // Child MSA
      if (opt1.MonthlyMsaContributionChild != opt2.MonthlyMsaContributionChild)
        return false;

      return true;
    }

    private static bool CompareTotalContributions(UpdateMedicalOptionVariantsDto opt1,
      UpdateMedicalOptionVariantsDto opt2)
    {
      // Principal Total
      if ((decimal)opt1.TotalMonthlyContributionsPrincipal != opt2.TotalMonthlyContributionsPrincipal)
        return false;

      // Adult Total  
      if (opt1.TotalMonthlyContributionsAdult != opt2.TotalMonthlyContributionsAdult)
        return false;

      // Child Total
      if (opt1.TotalMonthlyContributionsChild != opt2.TotalMonthlyContributionsChild)
        return false;

      // Child2 Total (if exists)
      if (opt1.TotalMonthlyContributionsChild2 != opt2.TotalMonthlyContributionsChild2)
        return false;

      return true;
    }

    #endregion

    #region Comprehensive Category Variant Validation

    /// <summary>
    /// Comprehensive validation for all category variants including update period, ID existence, and business rules
    /// </summary>
    /// <param name="categoryId">The category ID to validate against</param>
    /// <param name="bulkUpdateDto">Collection of update DTOs to validate</param>
    /// <param name="repository">Repository for data access operations</param>
    /// <param name="dbData">Existing database data for validation context</param>
    /// <param name="testDate">Optional test date for unit testing (defaults to current UTC time)</param>
    /// <returns>BulkValidationResult indicating success or failure with detailed error message</returns>
    /// <remarks>
    /// This method performs comprehensive validation including:
    /// - Update period restrictions (November-December)
    /// - ID existence validation
    /// - Business rule validation
    /// - Contribution structure validation
    /// 
    /// The optional testDate parameter allows for deterministic testing by overriding DateTime.UtcNow
    /// </remarks>
    public static async Task<BulkValidationResult> ValidateAllCategoryVariantsComprehensiveAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository,
      List<MedicalOption> dbData,
      DateTime? testDate = null) // Add optional test date parameter
    {
      var result = new BulkValidationResult() { IsValid = true };

      try
      {
        // 1. PERIOD VALIDATION (Global requirement)
        var dateToValidate = testDate ?? DateTime.UtcNow;

        // Use dateToValidate instead of DateTime.UtcNow in update period validation
        if (!ValidateUpdatePeriod(dateToValidate))
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
        // 1.5. PRE-FILTER ID VALIDATION (Before variant grouping)
        if (!ValidateAllIdsExistAsync(bulkUpdateDto, repository, dbData))
        {
          result.IsValid = false;
          result.ErrorMessage = "One or more medical option IDs do not exist in the database";
          return result;
        }
        // 2. CATEGORY BUSINESS RULES (FIRST - handles restricted categories)
        var categoryValidation = await ValidateCategoryBusinessRulesAsync(
          categoryId, bulkUpdateDto, repository, dbData);

        if (!categoryValidation.IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = categoryValidation.ErrorMessage;
          return result;
        }

        // 3. GROUP BY VARIANT using existing factory
        var variantGroups = GroupOptionsByVariant(bulkUpdateDto, dbData);

        if (variantGroups.Count == 0)
        {
          result.IsValid = false;
          result.ErrorMessage = $"No valid variants found for category ID: {categoryId}";
          return result;
        }

        // 4. VALIDATE EACH VARIANT INDIVIDUALLY
        foreach (var variantGroup in variantGroups)
        {
          var variantValidation = ValidateSingleVariantWithExistingLogicAsync(
            variantGroup.Key,
            variantGroup.Value,
            bulkUpdateDto,
            repository,
            categoryId, dbData);

          if (!variantValidation.IsValid)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Variant '{variantGroup.Key}' validation failed: " +
                                  $"{variantValidation.ErrorMessage}";
            return result;
          }
        }
        // 4.5. Validate if all variant group combined == dbData.Count

        // 5. Validate for Cross-variant Consistency (Looking for duplication of
        // contributions within category across variants)
        // if (!ValidateCrossVariantConsistency(variantGroups).IsValid)
        // {
        //   return result;
        // }

        // 6. SUCCESS
        result.IsValid = true;
      }
      catch (Exception ex)
      {
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
        _globalCategoryName = variantInfo.CategoryName;
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

    /// <summary>
    /// SINGLE VARIANT VALIDATION USING EXISTING LOGIC
    /// </summary>
    private static BulkValidationResult ValidateSingleVariantWithExistingLogicAsync(
      string variantName,
      List<UpdateMedicalOptionVariantsDto> variantOptions,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository,
      int categoryId, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult() { IsValid = true };

      try
      {
        // Filter DTOs for this variant
        var variantUpdateDtos = bulkUpdateDto
          .Where(dto => variantOptions.Any(opt => opt.MedicalOptionId == dto.MedicalOptionId))
          .ToList();

        // 1. Entity count validation 
        if (!ValidateEntityCount(variantOptions.Count, variantUpdateDtos.Count))
        {
          result.IsValid = false;
          result.ErrorMessage = $"Entity count mismatch for variant '{variantName}'. Expected: {variantOptions.Count}, Provided: {variantUpdateDtos.Count}";
          return result;
        }

        // 2. ID existence validation 
        if (!ValidateAllIdsExistAsync(variantUpdateDtos, repository, dbData))
        {
          result.IsValid = false;
          result.ErrorMessage = $"One or more medical option IDs do not exist in variant '{variantName}'";
          return result;
        }

        // 3. Category membership validation 
        if (!ValidateAllIdsInCategoryAsync(variantUpdateDtos, categoryId, repository, dbData))
        {
          result.IsValid = false;
          result.ErrorMessage = $"One or more medical option IDs do not belong to category {categoryId} in variant '{variantName}'";
          return result;
        }

        // 4. Individual contribution validation 
        foreach (var dto in variantUpdateDtos)
        {
          // Get the corresponding database option for THIS payload item
          var dbOption = dbData.FirstOrDefault(o => o.MedicalOptionId == dto.MedicalOptionId);

          if (dbOption != null && !ValidateContributionValuesWithContext(dto, dbOption))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Invalid contribution values for option {dto.MedicalOptionId} ({dbOption.MedicalOptionName}) in variant '{variantName}'";
            return result;
          }
        }

        // 5. Salary bracket validation 
        var salaryBracketRecords = new List<SalaryBracketValidatorRecord>();
        foreach (var dto in variantUpdateDtos)
        {
          var dbOption = dbData.FirstOrDefault(o => o.MedicalOptionId == dto.MedicalOptionId);
          if (dbOption != null && ((dto.SalaryBracketMin.HasValue || dto.SalaryBracketMin >= 0) || (dto.SalaryBracketMax.HasValue || dto.SalaryBracketMax > 0)))
          {
            //if first option min must be 0
            if ((dto == variantUpdateDtos.First()) && dto.SalaryBracketMin != 0)
            {
              result.IsValid = false;
              result.ErrorMessage =
                $"First option's Salary Bracket Min cannot be greater than 0 for '{variantName}'";
              return result;
            }
            //else continue
            salaryBracketRecords.Add(new SalaryBracketValidatorRecord(
              dto.MedicalOptionId, dbOption.MedicalOptionName, dto.SalaryBracketMin, dto.SalaryBracketMax));
          }
        }

        if (salaryBracketRecords.Count > 0)
        {
          if (!ValidateNoGapsInSalaryRanges(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Gaps detected in salary ranges for variant '{variantName}'";
            return result;
          }

          if (!ValidateNoOverlappingBrackets(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = $"Overlapping salary brackets detected in variant '{variantName}'";
            return result;
          }
        }

        // 6. Contribution structure consistency within variant
        var structureValidation = ValidateVariantContributionStructure(variantOptions, variantName, dbData);
        if (!structureValidation.IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = structureValidation.ErrorMessage;
          return result;
        }

        // 7. Variant-specific business rules
        var variantRulesValidation = ValidateVariantSpecificBusinessRules(_globalCategoryName, variantOptions, dbData);
        if (!variantRulesValidation.IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = variantRulesValidation.ErrorMessage;
          return result;
        }

        // 8. Network Choice specific child contribution rules
        if (variantName.Contains("Network Choice"))
        {
          var childContributionValidation = ValidateNetworkChoiceChildContributions(
            variantName, variantOptions, bulkUpdateDto, dbData);

          if (!childContributionValidation.IsValid)
          {
            result.IsValid = false;
            result.ErrorMessage = childContributionValidation.ErrorMessage;
            return result;
          }
        }

        // New Addition
        // 9. Contribution structure consistency validation
        /*
        var structureResult = ValidateVariantContributionStructure(variantOptions, variantName, dbData);
        if (!structureResult.IsValid) {
          result.IsValid = false;
          result.ErrorMessage = structureResult.ErrorMessage;
          return result;
          */
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Single variant validation error for '{variantName}': {ex.Message}";
      }

      return result;
    }

    // Validate count on variant groups
    private static BulkValidationResult ValidateVarientGroupCountAgainstDbCount(
      List<UpdateMedicalOptionVariantsDto> variantGroups, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult() { IsValid = true };

      var variants = variantGroups.ToList();

      if (variants.Count != dbData.Count)
      {
        result.IsValid = false;
        result.ErrorMessage =
          $"Total option count across the category : '{_globalCategoryName}' does not match with the existing data";
        return result;
      }
      //if true 
      return result;
    }

    /// <summary>
    /// VARIANT-SPECIFIC BUSINESS RULES
    /// Corrected for Network Choice (Risk + Principal, NO MSA)
    /// </summary>
    public static BulkValidationResult ValidateVariantSpecificBusinessRules(
      string variantName,
      List<UpdateMedicalOptionVariantsDto> variantOptions, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult() { IsValid = true };

      try
      {
        // Network Choice variants - Risk + Principal (NO MSA)
        if (variantName.Contains("Network Choice"))
        {
          var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
          var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

          if (hasMsa)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Network Choice variant '{variantName}' should NOT have MSA contributions (Risk + Principal only)";
            return result;
          }

          if (!hasPrincipal)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Network Choice variant '{variantName}' must have Principal contributions";
            return result;
          }
        }

        // First Choice variants - Risk only (no MSA, no Principal)
        else if (variantName.Contains("First Choice"))
        {
          var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
          var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

          if (hasMsa)
          {
            result.IsValid = false;
            result.ErrorMessage = $"First Choice variant '{variantName}' should NOT have MSA contributions (Risk only)";
            return result;
          }

          if (hasPrincipal)
          {
            result.IsValid = false;
            result.ErrorMessage = $"First Choice variant '{variantName}' should NOT have Principal contributions (Risk only)";
            return result;
          }
        }

        // Essential variants - Risk + MSA + Principal
        else if (variantName.Contains("Essential"))
        {
          var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
          var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

          if (!hasMsa)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Essential variant '{variantName}' must have MSA contributions";
            return result;
          }

          if (!hasPrincipal)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Essential variant '{variantName}' must have Principal contributions";
            return result;
          }
        }



        // Double variants - Risk + MSA (no Principal)
        else if (variantName.Contains("Double"))
        {
          var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
          var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

          if (!hasMsa)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Double variant '{variantName}' must have MSA contributions";
            return result;
          }

          if (hasPrincipal)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Double variant '{variantName}' should NOT have Principal contributions (Risk + MSA only)";
            return result;
          }
        }

        // Alliance variants - Risk + MSA (no Principal)
        else if (variantName.Contains("Alliance"))
        {
          var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
          var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

          if (!hasMsa)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Alliance variant '{variantName}' must have MSA contributions";
            return result;
          }

          if (hasPrincipal)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Alliance variant '{variantName}' should NOT have Principal contributions (Risk + MSA only)";
            return result;
          }
        }

        // Vital variants - Risk only (no MSA, no Principal)
        else if (variantName.Contains("Vital") || variantName.Contains("Plus") || variantName.Contains("Network"))
        {
          var hasMsa = variantOptions.Any(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0);
          var hasPrincipal = variantOptions.Any(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0);

          if (hasMsa)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Vital variant '{variantName}' should NOT have MSA contributions (Risk only)";
            return result;
          }

          if (hasPrincipal)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Vital variant '{variantName}' should NOT have Principal contributions (Risk only)";
            return result;
          }
        }
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Variant-specific business rules validation error for '{variantName}': {ex.Message}";
      }

      return result;
    }

    /// <summary>
    /// CORE CONTRIBUTION VALIDATION LOGIC
    /// Updated for correct Network Choice handling (Risk + Principal, NO MSA)
    /// </summary>
    public static BulkValidationResult ValidateContributionValues(
      UpdateMedicalOptionVariantsDto entity,
      bool hasMsa,
      bool hasPrincipal)
    {
      var result = new BulkValidationResult() { IsValid = true };

      // Determine variant type based on contribution structure
      var isRiskOnlyVariant = !hasMsa && !hasPrincipal;           // First Choice, Vital
      var isRiskPlusPrincipalVariant = !hasMsa && hasPrincipal;     // Network Choice
      var isMsaOnlyVariant = hasMsa && !hasPrincipal;              // Alliance, Double
      var isMsaWithPrincipalVariant = hasMsa && hasPrincipal;      // Essential

      // Validate Risk contributions (all options must have these)
      if (!ValidateRiskContributions(entity, hasPrincipal))
      {
        result.IsValid = false;
        result.ErrorMessage = "Risk contributions validation failed";
        return result;
      }

      // Apply validation logic based on variant type
      if (isRiskOnlyVariant)
      {
        // Risk-only variants: Risk should equal Total
        if (!ValidateRiskOnlyTotalContributions(entity).IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = "Risk-only variant: Risk contribution must equal Total contribution";
        }
      }
      else if (isRiskPlusPrincipalVariant)
      {
        // Risk + Principal variants: Risk should equal Total
        if (!ValidateRiskPlusPrincipalTotalContributions(entity).IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = "Risk + Principal variant: Risk contribution must equal Total contribution";
        }
      }
      else if (isMsaOnlyVariant)
      {
        // MSA-only variants: Risk + MSA should equal Total
        if (!ValidateMsaOnlyTotalContributions(entity).IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = "MSA-only variant: Risk + MSA contributions must equal Total contribution";
        }
      }
      else if (isMsaWithPrincipalVariant)
      {
        // MSA + Principal variants: Risk + MSA should equal Total
        if (!ValidateMsaWithPrincipalTotalContributions(entity).IsValid)
        {
          result.IsValid = false;
          result.ErrorMessage = "MSA + Principal variant: Risk + MSA contributions must equal Total contribution";
        }
      }
      else
      {
        result.IsValid = false;
        result.ErrorMessage = "Unknown variant type - unable to determine contribution validation rules";
      }

      return result;
    }

    /// <summary>
    /// VALIDATION FOR RISK + PRINCIPAL VARIANTS (Network Choice)
    /// Risk should equal Total (NO MSA validation)
    /// </summary>
    private static BulkValidationResult ValidateRiskPlusPrincipalTotalContributions(UpdateMedicalOptionVariantsDto entity)
    {
      var result = new BulkValidationResult { IsValid = true };
      const decimal tolerance = 0.01m;

      // Adult: Risk should equal Total
      if (Math.Abs((decimal)(entity.MonthlyRiskContributionAdult - entity.TotalMonthlyContributionsAdult)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Adult: Risk contribution must equal Total contribution for Network Choice variant";
        return result;
      }

      // Child: Risk should equal Total
      if (Math.Abs((decimal)(entity.MonthlyRiskContributionChild - entity.TotalMonthlyContributionsChild)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Child: Risk contribution must equal Total contribution for Network Choice variant";
        return result;
      }

      // Principal: Risk should equal Total (must be present)
      if (!entity.MonthlyRiskContributionPrincipal.HasValue || !entity.TotalMonthlyContributionsPrincipal.HasValue)
      {
        result.IsValid = false;
        result.ErrorMessage = "Principal contributions are required for Network Choice variant";
        return result;
      }

      if (Math.Abs(entity.MonthlyRiskContributionPrincipal.Value - entity.TotalMonthlyContributionsPrincipal.Value) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Principal: Risk contribution must equal Total contribution for Network Choice variant";
        return result;
      }

      return result;
    }

    /// <summary>
    /// VALIDATION FOR RISK-ONLY VARIANTS (First Choice, Vital)
    /// Risk = Total for Adult, Child, and Child2 (if present)
    /// </summary>
    private static BulkValidationResult ValidateRiskOnlyTotalContributions(UpdateMedicalOptionVariantsDto entity)
    {
      var result = new BulkValidationResult { IsValid = true };
      const decimal tolerance = 0.01m;

      // Adult: Risk should equal Total
      if (Math.Abs((decimal)(entity.MonthlyRiskContributionAdult - entity.TotalMonthlyContributionsAdult)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Adult: Risk contribution must equal Total contribution for Risk-only variant";
        return result;
      }

      // Child: Risk should equal Total
      if (Math.Abs((decimal)(entity.MonthlyRiskContributionChild - entity.TotalMonthlyContributionsChild)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Child: Risk contribution must equal Total contribution for Risk-only variant";
        return result;
      }

      // Child2: Risk should equal Total (if present)
      if ((entity.MonthlyRiskContributionChild2.HasValue && entity.MonthlyRiskContributionChild2 > 0 && entity.MonthlyRiskContributionChild2 is not null) &&
          (entity.TotalMonthlyContributionsChild2.HasValue && entity.TotalMonthlyContributionsChild2 > 0 && entity.TotalMonthlyContributionsChild2 is not null))
      {
        if (Math.Abs(entity.MonthlyRiskContributionChild2.Value - entity.TotalMonthlyContributionsChild2.Value) > tolerance)
        {
          result.IsValid = false;
          result.ErrorMessage = "Child2: Risk contribution must equal Total contribution for Risk-only variant";
          return result;
        }
      }

      return result;
    }

    /// <summary>
    /// VALIDATION FOR MSA-ONLY VARIANTS (Alliance, Double)
    /// Risk + MSA should equal Total (no Principal validation)
    /// </summary>
    private static BulkValidationResult ValidateMsaOnlyTotalContributions(UpdateMedicalOptionVariantsDto entity)
    {
      var result = new BulkValidationResult { IsValid = true };
      const decimal tolerance = 0.01m;

      // Adult: Risk + MSA should equal Total
      var adultTotal = entity.MonthlyRiskContributionAdult + entity.MonthlyMsaContributionAdult.Value;
      if (Math.Abs((decimal)(adultTotal - entity.TotalMonthlyContributionsAdult)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Adult: Risk + MSA contributions must equal Total contribution for MSA-only variant";
        return result;
      }

      // Child: Risk + MSA should equal Total
      var childTotal = entity.MonthlyRiskContributionChild + entity.MonthlyMsaContributionChild.Value;
      if (Math.Abs((decimal)(childTotal - entity.TotalMonthlyContributionsChild)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Child: Risk + MSA contributions must equal Total contribution for MSA-only variant";
        return result;
      }

      return result;
    }

    /// <summary>
    /// VALIDATION FOR MSA + PRINCIPAL VARIANTS (Essential)
    /// Risk + MSA should equal Total (including Principal validation)
    /// </summary>
    private static BulkValidationResult ValidateMsaWithPrincipalTotalContributions(UpdateMedicalOptionVariantsDto entity)
    {
      var result = new BulkValidationResult { IsValid = true };
      const decimal tolerance = 0.01m;

      // Adult: Risk + MSA should equal Total
      var adultTotal = entity.MonthlyRiskContributionAdult + entity.MonthlyMsaContributionAdult.Value;
      if (Math.Abs((decimal)(adultTotal - entity.TotalMonthlyContributionsAdult)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Adult: Risk + MSA contributions must equal Total contribution for MSA + Principal variant";
        return result;
      }

      // Child: Risk + MSA should equal Total
      var childTotal = entity.MonthlyRiskContributionChild + entity.MonthlyMsaContributionChild.Value;
      if (Math.Abs((decimal)(childTotal - entity.TotalMonthlyContributionsChild)) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Child: Risk + MSA contributions must equal Total contribution for MSA + Principal variant";
        return result;
      }

      // Principal: Risk + MSA should equal Total (must be present)
      if (!entity.MonthlyRiskContributionPrincipal.HasValue || !entity.MonthlyMsaContributionPrincipal.HasValue || !entity.TotalMonthlyContributionsPrincipal.HasValue)
      {
        result.IsValid = false;
        result.ErrorMessage = "Principal contributions (Risk, MSA, and Total) are required for Essential variant";
        return result;
      }

      var principalTotal = entity.MonthlyRiskContributionPrincipal.Value + entity.MonthlyMsaContributionPrincipal.Value;
      if (Math.Abs(principalTotal - entity.TotalMonthlyContributionsPrincipal.Value) > tolerance)
      {
        result.IsValid = false;
        result.ErrorMessage = "Principal: Risk + MSA contributions must equal Total contribution for MSA + Principal variant";
        return result;
      }

      return result;
    }

    /// <summary>
    /// NETWORK CHOICE CHILD CONTRIBUTION VALIDATION
    /// Business Rules: Options 1-3: Child2 = R0 (free), Options 4-5: Child2 = Child (same billing)
    /// </summary>
    /// <summary>
    /// Network Choice child contribution validation
    /// </summary>
    private static BulkValidationResult ValidateNetworkChoiceChildContributions(
      string variantName,
      List<UpdateMedicalOptionVariantsDto> variantOptions,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      List<MedicalOption> dbData) // Add dbData parameter
    {
      var result = new BulkValidationResult();

      try
      {
        // Only apply to Network Choice variants
        if (!variantName.Contains("Network Choice"))
        {
          return result; // Not applicable, pass validation
        }

        // Get database options for sorting
        var dbVariantOptions = dbData.Where(o =>
          variantOptions.Any(dto => dto.MedicalOptionId == o.MedicalOptionId)).ToList();

        // Sort options by name to identify 1-5 sequence
        var sortedDbOptions = dbVariantOptions.OrderBy(o => o.MedicalOptionName).ToList();

        for (int i = 0; i < sortedDbOptions.Count; i++)
        {
          var dbOption = sortedDbOptions[i];
          var optionNumber = i + 1; // 1-based index
          var correspondingDto = bulkUpdateDto.FirstOrDefault(dto => dto.MedicalOptionId == dbOption.MedicalOptionId);

          if (correspondingDto == null) continue; // Skip if not being updated

          // RULE: Network Choice 1-3 - Child2 should be FREE (0)
          if (optionNumber <= 3)
          {
            if (correspondingDto.MonthlyRiskContributionChild2.HasValue &&
                correspondingDto.MonthlyRiskContributionChild2.Value != 0)
            {

              result.IsValid = false;
              result.ErrorMessage = $"Network Choice {optionNumber} ({dbOption.MedicalOptionName}) must have Child2 contribution of R0 (free). Current: R{correspondingDto.MonthlyRiskContributionChild2.Value:N2}";
              return result;
            }
          }

          // RULE: Network Choice 4-5 - Child2 should equal Child (same billing)
          else if (optionNumber >= 4)
          {
            var childContribution = correspondingDto.MonthlyRiskContributionChild;
            var child2Contribution = correspondingDto.MonthlyRiskContributionChild2;

            if (child2Contribution.HasValue && childContribution != child2Contribution.Value)
            {
              result.IsValid = false;
              result.ErrorMessage = $"Network Choice {optionNumber} ({dbOption.MedicalOptionName}) must have Child2 contribution equal to Child contribution. Child: R{childContribution:N2}, Child2: R{child2Contribution.Value:N2}";
              return result;
            }

            if (!child2Contribution.HasValue)
            {
              result.IsValid = false;
              result.ErrorMessage = $"Network Choice {optionNumber} ({dbOption.MedicalOptionName}) must have Child2 contribution specified (should equal Child contribution: R{childContribution:N2})";
              return result;
            }
          }
        }
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Network Choice child contribution validation error: {ex.Message}";
      }

      return result;
    }

    /// <summary>
    /// CONTRIBUTION STRUCTURE CONSISTENCY VALIDATION
    /// </summary>
    private static BulkValidationResult ValidateVariantContributionStructure(
      List<UpdateMedicalOptionVariantsDto> variantOptions,
      string variantName, List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult() { IsValid = true };

      try
      {
        if (variantOptions.Count < 2) return result; // Not applicable for single option

        // Check MSA structure consistency
        var msaStructures = variantOptions
          .Select(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0)
          .Distinct()
          .ToList();

        if (msaStructures.Count > 1)
        {
          var optionsWithMsa = variantOptions
            .Where(o => o.MonthlyMsaContributionAdult.HasValue && o.MonthlyMsaContributionAdult > 0)
            .Select(dto =>
            {
              var dbOption = dbData.FirstOrDefault(dbo => dbo.MedicalOptionId == dto.MedicalOptionId);
              return dbOption?.MedicalOptionName ?? $"Option {dto.MedicalOptionId}";
            });

          var optionsWithoutMsa = variantOptions
            .Where(o => !o.MonthlyMsaContributionAdult.HasValue || o.MonthlyMsaContributionAdult <= 0)
            .Select(dto =>
            {
              var dbOption = dbData.FirstOrDefault(dbo => dbo.MedicalOptionId == dto.MedicalOptionId);
              return dbOption?.MedicalOptionName ?? $"Option {dto.MedicalOptionId}";
            });

          result.IsValid = false;
          result.ErrorMessage = $"Inconsistent MSA structure in variant '{variantName}'. With MSA: [{string.Join(", ", optionsWithMsa)}]. Without MSA: [{string.Join(", ", optionsWithoutMsa)}]";
          return result;
        }

        // Check Principal structure consistency
        var principalStructures = variantOptions
          .Select(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0)
          .Distinct()
          .ToList();

        if (principalStructures.Count > 1)
        {
          var optionsWithPrincipal = variantOptions
            .Where(o => o.MonthlyRiskContributionPrincipal.HasValue && o.MonthlyRiskContributionPrincipal > 0)
            .Select(dto =>
            {
              var dbOption = dbData.FirstOrDefault(dbo => dbo.MedicalOptionId == dto.MedicalOptionId);
              return dbOption?.MedicalOptionName ?? $"Option {dto.MedicalOptionId}";
            });

          var optionsWithoutPrincipal = variantOptions
            .Where(o => !o.MonthlyRiskContributionPrincipal.HasValue || o.MonthlyRiskContributionPrincipal <= 0)
            .Select(dto =>
            {
              var dbOption = dbData.FirstOrDefault(dbo => dbo.MedicalOptionId == dto.MedicalOptionId);
              return dbOption?.MedicalOptionName ?? $"Option {dto.MedicalOptionId}";
            });


          result.IsValid = false;
          result.ErrorMessage = $"Inconsistent Principal structure in variant '{variantName}'. With Principal: [{string.Join(", ", optionsWithPrincipal)}]. Without Principal: [{string.Join(", ", optionsWithoutPrincipal)}]";
          return result;
        }
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Contribution structure validation error for variant '{variantName}': {ex.Message}";
      }

      return result;
    }

    /// <summary>
    /// Groups payload DTOs by variant using existing data for context
    /// </summary>
    public static Dictionary<string, List<UpdateMedicalOptionVariantsDto>> GroupOptionsByVariant(
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      List<MedicalOption> dbData)
    {
      var variantGroups = new Dictionary<string, List<UpdateMedicalOptionVariantsDto>>();

      foreach (var dto in bulkUpdateDto)
      {
        // Find corresponding database option to get variant info
        var dbOption = dbData.FirstOrDefault(o => o.MedicalOptionId == dto.MedicalOptionId);
        if (dbOption == null) continue;

        // Get variant info from database option
        var (_, variantName, filterName) = MedicalOptionVariantFactory.GetVariantInfoSafe(dbOption);

        if (!variantGroups.TryGetValue(variantName, out List<UpdateMedicalOptionVariantsDto>? value))
        {
          value = new List<UpdateMedicalOptionVariantsDto>();
          variantGroups[variantName] = value;
        }

        value.Add(dto);
      }

      return variantGroups;
    }

    #endregion
  }

  /// <summary>
  /// Result object for bulk validation operations containing validation status and error information.
  /// This class provides a standardized result format for all validation operations,
  /// enabling consistent error handling and reporting across the validation framework.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Usage Pattern:</strong>
  /// All validation methods return instances of this class to provide
  /// standardized success/failure reporting with detailed error context.
  /// </para>
  /// 
  /// <para>
  /// <strong>Error Handling:</strong>
  /// The ErrorMessage property provides detailed error information
  /// for debugging, user feedback, and audit trail purposes.
  /// </para>
  /// 
  /// <para>
  /// <strong>Extended Information:</strong>
  /// The SalaryBracketRecords property can be used to provide
  /// additional context for salary bracket-specific validation failures.
  /// </para>
  /// </remarks>
  public class BulkValidationResult
  {
    /// <summary>
    /// Gets or sets a value indicating whether the validation operation was successful.
    /// True indicates all validations passed; false indicates at least one validation failure.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the detailed error message when validation fails.
    /// This property provides specific information about what validation rule failed
    /// and why, enabling debugging and user feedback.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the collection of salary bracket records involved in validation.
    /// This property provides context for salary bracket-specific validation failures
    /// and can be used for detailed error reporting and debugging.
    /// </summary>
    public List<SalaryBracketValidatorRecord> SalaryBracketRecords { get; set; } = new();
  }
}
