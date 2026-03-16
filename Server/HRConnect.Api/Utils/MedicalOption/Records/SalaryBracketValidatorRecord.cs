namespace HRConnect.Api.Utils.MedicalOption.Records
{
  /// <summary>
  /// Represents an immutable data structure for salary bracket validation operations.
  /// This record encapsulates the essential information required for validating
  /// medical option salary brackets, ensuring data integrity and providing
  /// context for validation error reporting.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Purpose:</strong>
  /// This record is designed specifically for salary bracket validation workflows,
  /// providing a clean, immutable data structure that contains all necessary
  /// information for validation logic and error reporting.
  /// </para>
  /// 
  /// <para>
  /// <strong>Immutability Benefits:</strong>
  /// - Thread-safe operations in concurrent validation scenarios
  /// - Guaranteed data integrity during validation processes
  /// - Predictable behavior in validation pipelines
  /// - Prevention of accidental data modification
  /// </para>
  /// 
  /// <para>
  /// <strong>Validation Context:</strong>
  /// The record provides both the data for validation (salary brackets) and the
  /// context needed for meaningful error messages (option ID and name), enabling
  /// precise error reporting and debugging.
  /// </para>
  /// 
  /// <para>
  /// <strong>Business Rules Supported:</strong>
  /// - Salary bracket range validation (min &​lt; max)
  /// - Overlapping bracket detection within categories
  /// - Consistency checks across multiple options
  /// - Compliance with actuarial requirements
  /// </para>
  /// 
  /// <para>
  /// <strong>Null Value Semantics:</strong>
  /// - salaryBracketMin: null indicates the bracket starts from 0 (no lower bound)
  /// - salaryBracketMax: null indicates the bracket is uncapped (no upper bound)
  /// - This design enables flexible salary bracket definitions
  /// </para>
  /// 
  /// <para>
  /// <strong>Performance Considerations:</strong>
  /// - Lightweight structure with minimal memory footprint
  /// - Value-based equality for efficient comparison operations
  /// - Optimized for bulk validation scenarios
  /// - No external dependencies or complex initialization
  /// </para>
  /// </remarks>
  public record SalaryBracketValidatorRecord(
    /// <summary>
    /// The unique identifier of the medical option.
    /// This ID is used for tracking, error reporting, and database operations
    /// during validation workflows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Usage:</strong>
    /// - Identifies the specific medical option being validated
    /// - Enables precise error reporting with option identification
    /// - Supports database lookups and update operations
    /// - Facilitates audit trails and logging
    /// </para>
    /// 
    /// <para>
    /// <strong>Validation Role:</strong>
    /// The option ID is crucial for validation error messages, allowing
    /// developers and users to identify exactly which medical option
    /// has validation issues.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var record = new SalaryBracketValidatorRecord(123, "Premium Plan", 5000, 15000);
    /// Console.WriteLine($"Validating option ID: {record.optionId}");
    /// // Output: Validating option ID: 123
    /// </code>
    /// </example>
    int optionId,

    /// <summary>
    /// The human-readable name of the medical option.
    /// This name provides context for validation operations and enables
    /// user-friendly error reporting and debugging.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Usage:</strong>
    /// - Provides descriptive context for validation operations
    /// - Enables user-friendly error messages
    /// - Supports debugging and troubleshooting
    /// - Enhances audit trail readability
    /// </para>
    /// 
    /// <para>
    /// <strong>Error Reporting:</strong>
    /// The option name is essential for creating meaningful validation
    /// error messages that users and developers can understand.
    /// </para>
    /// 
    /// <para>
    /// <strong>Format Considerations:</strong>
    /// Should be a descriptive, human-readable name that clearly
    /// identifies the medical option to end users.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var record = new SalaryBracketValidatorRecord(123, "Premium Health Plan", 5000, 15000);
    /// Console.WriteLine($"Validating: {record.optionName}");
    /// // Output: Validating: Premium Health Plan
    /// 
    /// // Error reporting example
    /// if (record.salaryBracketMin >= record.salaryBracketMax)
    /// {
    ///     throw new ValidationException(
    ///         $"Invalid salary bracket for '{record.optionName}': " +
    ///         "Minimum must be less than maximum");
    /// }
    /// </code>
    /// </example>
    string optionName,

    /// <summary>
    /// The minimum salary threshold for the medical option bracket.
    /// When null, indicates that the bracket starts from 0 (no lower bound).
    /// This value defines the lowest salary level eligible for this medical option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Null Semantics:</strong>
    /// - null value: Salary bracket starts from 0 (no minimum requirement)
    /// - decimal value: Specific minimum salary threshold
    /// - This flexibility supports various pricing strategies
    /// </para>
    /// 
    /// <para>
    /// <strong>Validation Rules:</strong>
    /// - Must be less than salaryBracketMax when both are non-null
    /// - Cannot be negative when specified
    /// - Should align with company salary structure
    /// - Must be consistent across related options
    /// </para>
    /// 
    /// <para>
    /// <strong>Business Context:</strong>
    /// The minimum salary threshold determines eligibility for the medical
    /// option based on employee compensation levels, ensuring appropriate
    /// cost-sharing arrangements.
    /// </para>
    /// 
    /// <para>
    /// <strong>Use Cases:</strong>
    /// - Entry-level plans: null (starts from 0)
    /// - Mid-tier plans: specific minimum (e.g., 5000.00)
    /// - Executive plans: higher minimum (e.g., 20000.00)
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Entry-level plan (no minimum)
    /// var entryLevel = new SalaryBracketValidatorRecord(1, "Basic Plan", null, 10000);
    /// Console.WriteLine($"Entry-level starts from: {(entryLevel.salaryBracketMin ?? 0):C}");
    /// // Output: Entry-level starts from: $0.00
    /// 
    /// // Mid-tier plan (with minimum)
    /// var midTier = new SalaryBracketValidatorRecord(2, "Standard Plan", 5000, 15000);
    /// Console.WriteLine($"Mid-tier minimum: {midTier.salaryBracketMin:C}");
    /// // Output: Mid-tier minimum: $5,000.00
    /// 
    /// // Validation example
    /// if (midTier.salaryBracketMin.HasValue && midTier.salaryBracketMin < 0)
    /// {
    ///     throw new ValidationException("Minimum salary cannot be negative");
    /// }
    /// </code>
    /// </example>
    decimal? salaryBracketMin, // null = starts from 0

    /// <summary>
    /// The maximum salary threshold for the medical option bracket.
    /// When null, indicates that the bracket is uncapped (no upper limit).
    /// This value defines the highest salary level eligible for this medical option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Null Semantics:</strong>
    /// - null value: Salary bracket is uncapped (no maximum limit)
    /// - decimal value: Specific maximum salary threshold
    /// - This flexibility supports various pricing strategies
    /// </para>
    /// 
    /// <para>
    /// <strong>Validation Rules:</strong>
    /// - Must be greater than salaryBracketMin when both are non-null
    /// - Cannot be negative when specified
    /// - Should not overlap with adjacent brackets
    /// - Must align with company compensation structure
    /// </para>
    /// 
    /// <para>
    /// <strong>Business Context:</strong>
    /// The maximum salary threshold ensures that medical options are
    /// appropriately priced for different compensation levels, preventing
    /// cross-subsidization between employee groups.
    /// </para>
    /// 
    /// <para>
    /// <strong>Use Cases:</strong>
    /// - Standard plans: specific maximum (e.g., 15000.00)
    /// - Executive plans: null (uncapped)
    /// - Tiered structures: multiple brackets with different maxima
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard plan (with maximum)
    /// var standard = new SalaryBracketValidatorRecord(1, "Standard Plan", 0, 15000);
    /// Console.WriteLine($"Standard plan maximum: {standard.salaryBracketMax:C}");
    /// // Output: Standard plan maximum: $15,000.00
    /// 
    /// // Executive plan (uncapped)
    /// var executive = new SalaryBracketValidatorRecord(2, "Executive Plan", 20000, null);
    /// Console.WriteLine($"Executive plan: {(executive.salaryBracketMax.HasValue ? $"up to {executive.salaryBracketMax:C}" : "no maximum limit")}");
    /// // Output: Executive plan: no maximum limit
    /// 
    /// // Range validation example
    /// if (standard.salaryBracketMin.HasValue && standard.salaryBracketMax.HasValue)
    /// {
    ///     if (standard.salaryBracketMin >= standard.salaryBracketMax)
    ///     {
    ///         throw new ValidationException(
    ///             $"Invalid range for {standard.optionName}: " +
    ///             "Minimum must be less than maximum");
    ///     }
    ///     
    ///     Console.WriteLine($"Valid range: {standard.salaryBracketMin:C} - {standard.salaryBracketMax:C}");
    ///     // Output: Valid range: $0.00 - $15,000.00
    /// }
    /// </code>
    /// </example>
    decimal? salaryBracketMax // null = uncapped
  );
}