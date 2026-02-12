namespace HRConnect.Api.Utils
{
  /// <summary>
  /// Defines medical option categories that are restricted from Salary Bracket updates.
  /// This enum is used to prevent updating the salary bracket of medical options
  /// that are part of a restricted category.
  /// </summary>
  /// <remarks>
  /// This enum is used to validate update operations on medical options to ensure that certain
  /// protected categories cannot have their salary brackets modified.
  ///
  /// And this has been implemented to cater for any future changes to the policy structure.
  /// where more policy would require such restriction. Therefore, this enum makes it easier to track and 
  /// manage any future changes to the policy structure (categories).
  /// 
  /// Example usage:
  /// <code>
  /// if (Enum.IsDefined(typeof(NoUpdateOnMedicalOptionCategory), category.Name))
  /// {
  ///   // Block the update operation
  ///   throw new InvalidOperationException("Salary bracket cannot be updated for this category");
  /// }
  /// </code>
  /// </remarks>
  public enum NoUpdateOnMedicalOptionCategory
  {
    /// <summary>
    /// Alliance medical option category - protected from salary bracket updates.
    /// This category typically has no salary bracket pricing structure (entry-level), thus cannot
    /// be modified as that is how the policy is structured.
    /// </summary>
    Alliance,
    
    /// <summary>
    /// Double medical option category - protected from salary bracket updates.
    /// This category typically has no salary bracket pricing structure (entry-level), thus cannot
    /// be modified as that is how the policy is structured.
    /// <para>
    /// The @ prefix is used to avoid naming conflicts with reserved keywords in C#.
    /// </para>
    /// </summary>
    @Double
  }
}