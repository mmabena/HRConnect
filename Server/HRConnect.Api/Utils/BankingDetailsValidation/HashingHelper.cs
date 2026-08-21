namespace HRConnect.Api.Utils.BankingDetailsValidation
{
    using System.Security.Cryptography;
    using System.Text;
    public class HashingHelper
    {
        private readonly byte[] _secretKey;

        public HashingHelper(IConfiguration config)
        {
            var key = config["EncryptionSettings:Key"];

            if (string.IsNullOrWhiteSpace(key))
               throw new InvalidOperationException("Encryption key is not configured.");

            _secretKey = Convert.FromBase64String(key);
        }

        // ======================================================
        // Normalize input (VERY IMPORTANT)
        // ======================================================
        public string Normalize(string input)
        {
            return input.Trim().Replace(" ", "");
        }

        // ======================================================
        // HMAC-SHA256 (USED FOR DUPLICATE DETECTION)
        // ======================================================
        public string ComputeSearchHash(string input)
        {
            var normalized = Normalize(input);

            using var hmac = new HMACSHA256(_secretKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalized));

            return Convert.ToHexString(hash);
        }
    }
}