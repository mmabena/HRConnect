namespace HRConnect.Api.Utils.MedicalOption.ValidationHelpers
{
  using DTOs.MedicalOption;
  using Models;
  using Records;

  public static class MedicalOptionContributionValidations
  {
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

      return MedicalOptionValidator.ValidateContributionValues(entity, hasMsa, hasPrincipal).IsValid;
    }
    
    /// <summary>
    /// Validates salary bracket gaps and overlaps within a single variant
    /// </summary>
    public static BulkValidationResult ValidateSingleVariantSalaryBrackets(
      List<MedicalOption> variantOptions)
    {
      var result = new BulkValidationResult() { IsValid = true}; 

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
          if (!MedicalOptionSalaryBracketValidations.ValidateNoGapsInSalaryRanges(salaryBracketRecords))
          {
            result.IsValid = false;
            result.ErrorMessage = "Gaps detected in salary ranges within variant";
            return result;
          }

          if (!MedicalOptionSalaryBracketValidations.ValidateNoOverlappingBrackets(salaryBracketRecords))
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
    public static bool ValidateRiskContributions(
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
  }
}