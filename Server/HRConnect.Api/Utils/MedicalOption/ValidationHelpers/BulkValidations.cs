namespace HRConnect.Api.Utils.MedicalOption.ValidationHelpers
{
  using Records;

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