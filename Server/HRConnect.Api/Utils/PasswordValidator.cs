namespace HRConnect.Api.Utils
{
  using System.Text.RegularExpressions;

  public static partial class PasswordValidator
  {
    [GeneratedRegex(
      @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
      RegexOptions.None,
      matchTimeoutMilliseconds: 100)]
    private static partial Regex PasswordRegexGenerated();
    private static readonly Regex PasswordRegex = PasswordRegexGenerated();

    public static bool IsValidPassword(string? password)
    {
      if (string.IsNullOrWhiteSpace(password))
      {
        return false;
      }

      return PasswordRegex.IsMatch(password);
    }

  }
}


