namespace HRConnect.Api.Utils
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Utility class used to validate whether a provided
    /// City and Zip Code combination is valid.
    /// </summary>
    public static class CityZipValidator
    {
        /// <summary>
        /// Internal mapping of supported cities and their allowed zip codes.
        /// Case-insensitive to avoid user input issues. When a employee is created or updated, this validator is used to ensure that the provided city and zip code are valid and consistent with each other. 
        /// This helps maintain data integrity and ensures that employee records contain accurate location information.
        /// </summary>
        private static readonly Dictionary<string, List<string>> CityZipMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Johannesburg", new List<string> { "2000", "2001" } },
                { "Cape Town", new List<string> { "8000", "8001" } },
                { "Pretoria", new List<string> { "0001", "0002" } },
                { "Durban", new List<string> { "4000", "4001" } },
                { "Bloemfontein", new List<string> { "9300", "9301" } }
            };
        /// <summary>
        /// Validates that the provided city exists and that
        /// the zip code belongs to that city.
        /// </summary>
        /// <param name="city">City name entered by the user.</param>
        /// <param name="zipCode">Zip / Postal code entered by the user.</param>
        /// <returns>Error Message if validation fails</returns>
        public static void ValidateCityAndZip(string city, string zipCode)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required");

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip Code is required");


            if (!CityZipMap.TryGetValue(city, out var allowedZips))
                throw new ArgumentException($"City '{city}' is not supported");

            if (!allowedZips.Contains(zipCode))
                throw new ArgumentException(
                    $"Zip Code '{zipCode}' is not valid for city '{city}'. " +
                    $"Allowed Zip Codes: {string.Join(", ", allowedZips)}"
                );
        }
    }
}