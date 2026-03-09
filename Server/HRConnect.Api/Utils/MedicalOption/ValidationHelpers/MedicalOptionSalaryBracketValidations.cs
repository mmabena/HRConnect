namespace HRConnect.Api.Utils.MedicalOption.ValidationHelpers
{
  using Enums;
  using Records;

  public static class MedicalOptionSalaryBracketValidations
  {
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
        .Where(r => r.salaryBracketMin.HasValue && 
                    (r.salaryBracketMax.HasValue || r.salaryBracketMax is null))
        .OrderBy(r => r.salaryBracketMin)
        .ToList();

      for (int i = 0; i < sortedRecords.Count - 1; i++)
      {
        var current = sortedRecords[i];
        var next = sortedRecords[i + 1];

        // Allow for 0.01 or 1.00 difference between ranges
        if (current.salaryBracketMax != null)
        {
          var expectedNextMin = current.salaryBracketMax.Value + 0.01m;
          if (next.salaryBracketMin > expectedNextMin + 0.99m) // Allow up to 1.00 gap
            return false;  
        }
        else
        {
          return true; // when current Max is == null (uncapped)
        }
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
        .Where(r => (r.salaryBracketMin.HasValue || r.salaryBracketMin >= 0 ) 
                    && (r.salaryBracketMax.HasValue || r.salaryBracketMax is null))
        .OrderBy(r => r.salaryBracketMin)
        .ToList();

      for (int i = 0; i < sortedRecords.Count - 1; i++)
      {
        var current = sortedRecords[i];
        var next = sortedRecords[i + 1];

        // Check for overlap (next.min should be > current.max)
        if (current.salaryBracketMax.HasValue && next.salaryBracketMin.HasValue || 
            !(next is null)) {
          if (next.salaryBracketMin <= current.salaryBracketMax.Value)
            return false;
        }
        
        // When current.max is null (uncapped), it can't overlap with anything -
        // what if there is a next value that has a less value ? [Resolved]
      }
      return true;
    }
    #endregion
  }
}