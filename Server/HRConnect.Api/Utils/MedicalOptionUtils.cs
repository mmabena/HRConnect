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
  } 
}