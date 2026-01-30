namespace HRConnect.Api.Utils
{
    using System.Text.RegularExpressions;
    public static partial class PasswordValidator
    {
        private static readonly Regex PasswordRegex = PasswordRegexGenerated();

        public static bool IsValidPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return PasswordRegex.IsMatch(password);
        }

        [GeneratedRegex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
            RegexOptions.None,
            matchTimeoutMilliseconds: 100)]
        private static partial Regex PasswordRegexGenerated();
    }
}


// using System.Text.RegularExpressions;

// namespace HRConnect.Api.Utils
// {
//     /// <summary>
//     /// Provides password validation utilities.
//     /// </summary>
//     public static class PasswordValidator
//     {
//         /// <summary>
//         /// Regular expression used to validate password strength.
//         /// Ensures a minimum of 8 characters, including at least one uppercase letter,
//         /// one lowercase letter, one digit, and one special character.
//         /// </summary>
//         private static readonly Regex _regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");

//         public static bool IsValidPassword(string? password)
//         {
//             if (string.IsNullOrWhiteSpace(password)) return false;
//             return _regex.IsMatch(password);
//         }
//     }
// }
