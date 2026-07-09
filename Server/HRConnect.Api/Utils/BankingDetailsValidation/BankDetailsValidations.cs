namespace HRConnect.Api.Utils.BankingDetailsValidation
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using HRConnect.Api.Utils;
    using HRConnect.Api.Services;

    public static partial class BankDetailsValidations
    {
        // -----------------------------
        // GENERATED REGEX (Performance safe)
        // -----------------------------
        [GeneratedRegex(@"^\d+$")]
        private static partial Regex DigitsOnly();

        [GeneratedRegex(@"^\d{6}$")]
        private static partial Regex SixDigits();

        [GeneratedRegex(@"^\d{10}$")]
        private static partial Regex TenDigits();

        [GeneratedRegex(@"^\d{9,11}$")]
        private static partial Regex NineToElevenDigits();

        [GeneratedRegex(@"^\d{8,13}$")]
        private static partial Regex EightToThirteenDigits();

        [GeneratedRegex(@"^\d{10,11}$")]
        private static partial Regex TenToElevenDigits();

        [GeneratedRegex(@"^\d{9,10}$")]
        private static partial Regex NineToTenDigits();

        [GeneratedRegex(@"^\d{8,12}$")]
        private static partial Regex EightToTwelveDigits();

        // -----------------------------
        // BASIC VALIDATION
        // -----------------------------
        public static bool IsValidBankName(string bankName)
        {
            return !string.IsNullOrWhiteSpace(bankName);
        }

        public static bool IsValidAccountNumber(string accountNumber)
        {
            return !string.IsNullOrWhiteSpace(accountNumber)
                   && DigitsOnly().IsMatch(accountNumber);
        }

        public static bool IsValidBranchCode(string branchCode)
        {
            return !string.IsNullOrWhiteSpace(branchCode)
                   && DigitsOnly().IsMatch(branchCode);
        }

        // -----------------------------
        // BUSINESS VALIDATION RULES
        // -----------------------------
        public static void ValidateBankingDetails(string bankName, string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(bankName))
                throw new ValidationException("Bank name is required");

            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ValidationException("Account number is required");

            bankName = bankName.Trim()
                               .Replace(" ", "")
                               .ToLower(CultureInfo.InvariantCulture);

            switch (bankName)
            {
                case "capitec":
                    Ensure(accountNumber, TenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;

                case "nedbank":
                    Ensure(accountNumber, TenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;

                case "fnb":
                case "absa":
                case "standardbank":
                    Ensure(accountNumber, NineToElevenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;

                case "investec":
                    Ensure(accountNumber, EightToThirteenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;

                case "africanbank":
                    Ensure(accountNumber, TenToElevenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;

                case "bidvestbank":
                    Ensure(accountNumber, NineToTenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;

                case "grindrodbank":
                    Ensure(accountNumber, EightToTwelveDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;

                case "discoverybank":
                    Ensure(accountNumber, NineToElevenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;
                case "tymebank":
                    Ensure(accountNumber, TenDigits(), "The account number entered does not match our records. Please verify and try again.");
                    break;
            }
        }

        // -----------------------------
        // HELPER
        // -----------------------------
        private static void Ensure(string value, Regex pattern, string message)
        {
            if (!pattern.IsMatch(value))
                throw new ValidationException(message);
        }
    }
}