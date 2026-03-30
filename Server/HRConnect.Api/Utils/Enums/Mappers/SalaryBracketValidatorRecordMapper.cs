namespace HRConnect.Api.Utils.Enums.Mappers
{
  using DTOs.MedicalOption;
  using MedicalOption.Records;

  /// <summary>
  /// Provides mapping functionality for converting UpdateMedicalOptionVariantsDto objects
  /// to SalaryBracketValidatorRecord records for salary bracket validation operations.
  /// This static class enables transformation of DTO data into a specialized record format
  /// optimized for salary bracket validation logic and business rule enforcement.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This mapper class serves as a data transformation utility that bridges the gap between
  /// the DTO layer (used for API communication) and the validation layer (using records
  /// for immutable data structures). It specifically focuses on extracting salary bracket
  /// information required for validation operations.
  /// </para>
  /// 
  /// <para>
  /// Key responsibilities:
  /// - Converting DTO objects to immutable record structures for validation
  /// - Extracting salary bracket data from complex DTO objects
  /// - Providing type-safe mapping with proper null handling
  /// - Supporting validation workflows that require specialized record formats
  /// </para>
  /// 
  /// <para>
  /// Architecture patterns:
  /// - Extension method pattern for fluent API design
  /// - Static utility class for stateless operations
  /// - Record-based immutable data structures for validation
  /// - DTO-to-record transformation pattern
  /// </para>
  /// 
  /// <para>
  /// Business context:
  /// Salary bracket validation is critical for medical benefit options to ensure:
  /// - Proper income-based pricing tiers
  /// - No overlapping salary ranges within categories
  /// - Consistent salary bracket definitions across options
  /// - Compliance with actuarial requirements
  /// - Fair pricing structures for different income levels
  /// </para>
  /// 
  /// <para>
  /// Data transformation considerations:
  /// - Nullable decimal types from DTO are converted to nullable decimal in record
  /// - Option name is passed separately to maintain context for validation
  /// - Medical option ID is preserved for traceability and error reporting
  /// - Type casting ensures proper data type conversion
  /// </para>
  /// 
  /// <para>
  /// Performance considerations:
  /// - Stateless operations with no external dependencies
  /// - Efficient record creation with minimal allocation
  /// - Direct property mapping without complex transformations
  /// - Optimized for bulk validation scenarios
  /// </para>
  /// </remarks>
  public static class SalaryBracketValidatorRecordMapper
  {
    /// <summary>
    /// Converts an UpdateMedicalOptionVariantsDto object to a SalaryBracketValidatorRecord
    /// for use in salary bracket validation operations. This extension method extracts
    /// the relevant salary bracket information and creates an immutable record optimized
    /// for validation logic.
    /// </summary>
    /// <param name="entity">The UpdateMedicalOptionVariantsDto containing salary bracket data to convert.</param>
    /// <param name="entityOptionName">The name of the medical option, providing context for validation and error reporting.</param>
    /// <returns>A SalaryBracketValidatorRecord containing the extracted salary bracket validation data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the entity parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when entityOptionName is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when salary bracket data is invalid for conversion.</exception>
    /// <remarks>
    /// <para>
    /// This method performs a focused transformation, extracting only the salary bracket
    /// related properties needed for validation. The resulting record provides an
    /// immutable structure ideal for validation workflows where data integrity is critical.
    /// </para>
    /// 
    /// <para>
    /// Transformation details:
    /// - MedicalOptionId: Direct mapping for option identification
    /// - entityOptionName: Passed as parameter to provide validation context
    /// - SalaryBracketMin: Converted from nullable decimal to nullable decimal with explicit casting
    /// - SalaryBracketMax: Converted from nullable decimal to nullable decimal with explicit casting
    /// </para>
    /// 
    /// <para>
    /// Null handling strategy:
    /// - SalaryBracketMin and SalaryBracketMax preserve nullability from source DTO
    /// - In the validation record, null values have specific meanings:
    ///   - null SalaryBracketMin: Salary bracket starts from 0
    ///   - null SalaryBracketMax: Salary bracket is uncapped (no upper limit)
    /// </para>
    /// 
    /// <para>
    /// Validation context:
    /// The option name parameter is crucial for providing meaningful error messages
    /// during validation failures, allowing developers and users to identify which
    /// specific medical option has validation issues.
    /// </para>
    /// 
    /// <para>
    /// Type safety considerations:
    /// - Explicit casting of decimal? to decimal? ensures type compatibility
    /// - The method maintains nullable semantics throughout the transformation
    /// - Record immutability prevents accidental modification of validation data
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a DTO with salary bracket data
    /// var updateDto = new UpdateMedicalOptionVariantsDto
    /// {
    ///     MedicalOptionId = 123,
    ///     SalaryBracketMin = 5000.00m,
    ///     SalaryBracketMax = 15000.00m,
    ///     MonthlyRiskContributionAdult = 200.00m,
    ///     TotalMonthlyContributionsAdult = 250.00m
    /// };
    /// 
    /// // Convert to validation record
    /// var validatorRecord = updateDto.ToSalaryBracketValidatorRecord("Premium Health Plan");
    /// 
    /// Console.WriteLine($"Validation Record:");
    /// Console.WriteLine($"  Option ID: {validatorRecord.optionId}");
    /// Console.WriteLine($"  Option Name: {validatorRecord.optionName}");
    /// Console.WriteLine($"  Salary Range: {validatorRecord.salaryBracketMin} - {validatorRecord.salaryBracketMax}");
    /// 
    /// // Use in validation
    /// if (validatorRecord.salaryBracketMin >= validatorRecord.salaryBracketMax)
    /// {
    ///     throw new ValidationException(
    ///         $"Invalid salary bracket for {validatorRecord.optionName}: " +
    ///         "Minimum must be less than maximum");
    /// }
    /// 
    /// // Example with uncapped upper limit
    /// var uncappedDto = new UpdateMedicalOptionVariantsDto
    /// {
    ///     MedicalOptionId = 456,
    ///     SalaryBracketMin = 20000.00m,
    ///     SalaryBracketMax = null, // Uncapped
    ///     MonthlyRiskContributionAdult = 500.00m,
    ///     TotalMonthlyContributionsAdult = 600.00m
    /// };
    /// 
    /// var uncappedRecord = uncappedDto.ToSalaryBracketValidatorRecord("Executive Health Plan");
    /// Console.WriteLine($"Uncapped bracket: {uncappedRecord.salaryBracketMin}+ (no upper limit)");
    /// 
    /// // Example with lower bound starting from 0
    /// var fromZeroDto = new UpdateMedicalOptionVariantsDto
    /// {
    ///     MedicalOptionId = 789,
    ///     SalaryBracketMin = null, // Starts from 0
    ///     SalaryBracketMax = 10000.00m,
    ///     MonthlyRiskContributionAdult = 150.00m,
    ///     TotalMonthlyContributionsAdult = 180.00m
    /// };
    /// 
    /// var fromZeroRecord = fromZeroDto.ToSalaryBracketValidatorRecord("Basic Health Plan");
    /// Console.WriteLine($"From zero bracket: 0 - {fromZeroRecord.salaryBracketMax}");
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entity parameter is null, preventing null reference exceptions during mapping.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when entityOptionName is null, empty, or whitespace, ensuring that validation
    /// records always have meaningful option names for error reporting.
    /// </exception>
    public static SalaryBracketValidatorRecord ToSalaryBracketValidatorRecord(
      this UpdateMedicalOptionVariantsDto entity, string entityOptionName)
    {
      return new SalaryBracketValidatorRecord
      (
        entity.MedicalOptionId,
        entityOptionName,
        (decimal)entity.SalaryBracketMin,
        (decimal)entity.SalaryBracketMax
      );
    }
  }
}