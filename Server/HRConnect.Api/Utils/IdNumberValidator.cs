namespace HRConnect.Api.Utils
{
    using System;
    using System.Globalization;
    using System.Linq;
    using HRConnect.Api.Models;
    /// <summary>
    /// Utility class responsible for validating and extracting information
    /// from a South African ID Number. Responsible for ensuring the ID number is in the correct format, extracting the date of birth and gender information, and validating that the employee is an adult. 
    /// This class is used during employee creation to populate the DateOfBirth and Gender fields based on the provided ID number, 
    /// and to ensure that the ID number is valid and that the employee meets the minimum age requirement.
    /// </summary>
    public static class IdNumberValidator
    {   /// <summary>
        /// Parses a South African ID number and extracts
        /// Date of Birth and Gender information.
        /// </summary>
        /// <param name="IdNumber">The employee ID Number</param>
        /// <returns>EmployeeInfo object containing extracted Date of Birth and Gender</returns>
        public static EmployeeInfo ParseIdNumber(string IdNumber)
        {
            // Validate the ID number format and content before decontructing it 
            ValidateIdNumber(IdNumber);
            // Extract year, month, day from Id Number to construct date of birth
            string yy = IdNumber.Substring(0, 2);
            string mm = IdNumber.Substring(2, 2);
            string dd = IdNumber.Substring(4, 2);

            int year = int.Parse(yy, CultureInfo.InvariantCulture);
            int currentYear = DateTime.Now.Year % 100;
            // Determine the full year based on the current year to handle both 1900s and 2000s birth years
            int fullYear = year <= currentYear ? 2000 + year : 1900 + year;
            // Create DateOnly object for Date of Birth
            DateOnly dateOfBirth = new DateOnly
            (
                fullYear, int.Parse(mm, CultureInfo.InvariantCulture),
                int.Parse(dd, CultureInfo.InvariantCulture)
            );
            // Extract gender code and determine gender
            int genderCode = int.Parse(IdNumber.AsSpan(6, 4), CultureInfo.InvariantCulture);
            Gender gender = genderCode < 5000 ? Gender.Female : Gender.Male;

            return new EmployeeInfo
            {
                Gender = gender,
                DateOfBirth = dateOfBirth
            };
        }
        /// <summary>
        /// Validates that the ID number is present, numeric,
        /// and exactly 13 digits long.
        /// </summary>
        /// <param name="IdNumber">The employee ID Number</param>
        /// <returns>Error message if validation fails</returns>
        private static void ValidateIdNumber(string IdNumber)
        {
            if (string.IsNullOrWhiteSpace(IdNumber))
                throw new ArgumentException("ID Number is required");

            if (IdNumber.Length != 13)
                throw new ArgumentException("ID number must be 13 digits");

            if (!IdNumber.All(char.IsDigit))
                throw new ArgumentException("ID number must contain only digits");
        }


    }
    /// <summary>
    /// Contains helper methods related to age calculations and validation.
    /// </summary>
    public static class AgeCalculator
    {
        /// <summary>
        /// Calculates age based on Date of Birth.
        /// </summary>
        /// <param name="dateOfBirth">The employee Date of Birth</param>
        /// <returns>Age of the employee</returns>
        public static int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth > today.AddYears(-age))
                age--;

            return age;
        }
    }
    /// <summary>
    /// Represents basic personal information extracted from an ID number.
    /// </summary>
    public class EmployeeInfo
    {
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}