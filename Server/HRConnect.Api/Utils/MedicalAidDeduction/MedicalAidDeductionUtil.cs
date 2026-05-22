namespace HRConnect.Api.Utils.MedicalAidDeduction
{
  using DTOs.MedicalOption;

  public static class MedicalAidDeductionUtil
  {
    public static bool EffectDateBeforeMidMonth(DateTime queryDate)
    {

      return queryDate.Day <= 15;
    }
    /// <summary>
    /// Calculates the principal premium based on total monthly contributions.
    /// </summary>
    public static decimal CalculatePrincipalPremium(MedicalOptionDto? option)
    {
      return (decimal)(option?.TotalMonthlyContributionsPrincipal ?? option?.TotalMonthlyContributionsAdult)! ;
    }

    /// <summary>
    /// Calculates the adult premium based on number of adults and per-adult contribution.
    /// </summary>
    public static decimal CalculateAdultPremium(MedicalOptionDto? option, int numberOfAdults)
    {
      if (numberOfAdults <= 0) return 0m;

      decimal adultContribution = (decimal)option?.TotalMonthlyContributionsAdult!;
      return adultContribution * numberOfAdults;
    }

    /// <summary>
    /// Calculates the child premium based on number of children and per-child contribution.
    /// </summary>
    public static decimal CalculateChildPremium(MedicalOptionDto? option, int numberOfChildren)
    {
      if (numberOfChildren <= 0) return 0m;

      decimal childContribution = (decimal)option?.TotalMonthlyContributionsChild!;
      if (option.MedicalOptionName.Contains("Network") &&
          (int.TryParse(option.MedicalOptionName[^1].ToString(), out int index) && index > 0 && index < 4))
      {
        return childContribution;
      }
      return childContribution * numberOfChildren;
    }

    public static decimal CalculateTotalPremium(decimal principalPremium, decimal adultPremium,
      decimal childPremium)
    {
      return Math.Abs(principalPremium + adultPremium + childPremium);
    }

    /// <summary>
    /// Get Effective date based on date supplied.
    /// </summary>
    /// <remarks>
    /// In accordance to the business rules, if the date is before mid-month, the effective date is the start date of the employee
    /// Else the effective date is the 1st of the following month within the current year.
    /// </remarks>
    /// 
    public static DateTime GetEffectiveDate(DateTime date)
    {
      return date.Day <= 15
        ? date
        : new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 1);
    }

  }
}

