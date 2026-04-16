namespace HRConnect.Api.Utils
{
  using System.Globalization;

  public static class GenerateUnqiueCode
  {
    public static string GenerateStringCode(string prefix, List<string> matchingCodes)
    {
      string codePrefix = char.IsDigit(prefix[0])
                ? "XXX"
                : ((prefix.Length >= 3)
                    ? prefix[..3].ToUpper(CultureInfo.InvariantCulture)
                    : prefix.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X'));
      int nextNum = 1;

      bool prefixExists = matchingCodes.Any(code => code.StartsWith(codePrefix, StringComparison.InvariantCultureIgnoreCase));

      if (matchingCodes != null && matchingCodes.Count > 0 && prefixExists)
      {
        int maxNum = matchingCodes.Max(code =>
        {
          return code.Length > prefix.Length && int.TryParse(code.AsSpan(prefix.Length), out int num) ? num : 0;
        });
        nextNum = maxNum + 1;
      }

      return $"{codePrefix}{nextNum:D3}";
    }
  }
}
