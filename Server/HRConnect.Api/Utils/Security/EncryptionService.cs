namespace HRConnect.Api.Utils.Security
{
    using HRConnect.Api.Interfaces;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using HRConnect.Api.Utils.Settings;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// EncryptionService provides methods to encrypt and decrypt sensitive data, such as bank account numbers, using AES encryption.
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    /// <summary>
    /// The EncryptionService class implements AES encryption to securely encrypt and decrypt sensitive information.
    ///  It uses a 256-bit key and a unique initialization vector (IV) for each encryption operation, 
    /// ensuring that the same plaintext will produce different ciphertexts each time. 
    /// The IV is stored alongside the encrypted data to allow for proper decryption. 
    /// This service is designed to protect sensitive data such as bank account numbers in the HRConnect application.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        /// <summary>
        /// Initializes a new instance of the EncryptionService class with the specified encryption key.
        ///  The key must be exactly 32 characters long to ensure it is suitable for AES-256 encryption.
        ///  If the key is invalid, an ArgumentException is thrown. 
        /// This constructor allows the service to be configured with a specific key, which can be stored securely in application settings or environment variables.
        /// </summary>
        /// <param name="key">The encryption key.</param>
        /// <exception cref="ArgumentException"></exception>
        public EncryptionService(IOptions<EncryptionSettings> settings)
        {
            var key = settings.Value.Key;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Encryption key is missing.");

            byte[] keyBytes;

            try
            {
                keyBytes = Convert.FromBase64String(key);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Encryption key must be a valid Base64 string.");
            }

            if (keyBytes.Length != 32)
                throw new ArgumentException("Encryption key must decode to 32 bytes (AES-256 required).");

            _key = keyBytes;
        }

        /// <summary>
        /// Encrypts the specified plaintext using AES encryption. The method generates a new initialization vector (IV) for each encryption operation, which is stored at the beginning of the resulting ciphertext. The plaintext is encrypted using the provided key and IV, and the final output is a Base64-encoded string that combines both the IV and the encrypted data. This approach ensures that even if the same plaintext is encrypted multiple times, it will yield different ciphertexts due to the unique IV used in each operation.
        /// </summary>
        /// <param name="plainText">The plaintext to encrypt.</param>
        /// <returns>The encrypted text.</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using var aes = Aes.Create();

            aes.Key = _key;
            aes.Mode = CipherMode.CBC;      //Secure mode
            aes.Padding = PaddingMode.PKCS7;

            //Generate a NEW IV every time
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();

            //Step 1: Store IV at the beginning
            ms.Write(aes.IV, 0, aes.IV.Length);

            // Step 2: Encrypt data
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            // Step 3: Convert to string for DB storage
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Decrypts the specified ciphertext using AES decryption. The method first extracts the initialization vector (IV) from the beginning of the ciphertext, which is necessary for the decryption process. It then uses the provided key and extracted IV to decrypt the remaining encrypted data. The final output is the original plaintext string. This method ensures that only those with access to the correct key can successfully decrypt the data, maintaining the confidentiality of sensitive information such as bank account numbers.
        /// </summary>
        /// <param name="cipherText">The ciphertext to decrypt.</param>
        /// <returns>The decrypted text.</returns>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();

            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Step 1: Extract IV (first 16 bytes)
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            // Step 2: Read encrypted data AFTER IV
            using var ms = new MemoryStream(fullCipher, 16, fullCipher.Length - 16);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            // Step 3: Return original value
            return sr.ReadToEnd();
        }
    }
}