namespace HRConnect.Api.Utils
{
  using System.Text.RegularExpressions;
  using DTOs.MedicalOption;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;

  public static partial class MedicalOptionUtils
  {
    [GeneratedRegex(@"\d$")]
    private static partial Regex TrailingDigitRegex();
    public static string OptionNameFormatter(string optionName)
    {
      // Remove trailing digit only
      return TrailingDigitRegex().Replace(optionName, ""); 
    }
    // Repo helper Method for filtering through Salary Brackets according to the employees
    // Salary and handles nulls on max which present uncapped
    public static bool IsSalaryInRange(decimal salary, decimal? min, decimal? max)
    {
      /*
      // 1st implementation
      // Handle null min (no lower bound)
      if (!min.HasValue) return !max.HasValue || salary <= max.Value;
      
      // Handle null max (no upper bound - uncapped)
      if (!max.HasValue) return salary >= min.Value;
      
      // Both bounds exist
      return salary >= min.Value && salary <= max.Value;
      */
      
      //2nd implementation
      return salary >= min && (!max.HasValue || salary <= max);

    }
  } 
}