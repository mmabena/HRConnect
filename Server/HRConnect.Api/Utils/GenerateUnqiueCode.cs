namespace HRConnect.Api.Utils
{
  using System.Globalization;

  public static class GenerateUnqiueCode
  {
    public static string GenerateStringCode(string inputData, List<string> matchingCodes)
    {
      int nextNum = 1;
      string prefix = (inputData.Length >= 3) ? inputData[..3].ToUpper(CultureInfo.InvariantCulture)
        : inputData.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X');

      if ((matchingCodes != null) && (matchingCodes.Count > 0))
      {
        int maxNum = matchingCodes.Max(code =>
        {
          return code.Length > prefix.Length && int.TryParse(code.AsSpan(prefix.Length), out int num) ? num : 0;
        });
      }


    }
  }
}
