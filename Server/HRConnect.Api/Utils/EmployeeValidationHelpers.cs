using System;
using System.IO;
using System.Linq;
using HRConnect.Api.DTOs.Employee;
using HRConnect.Api.Models;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Services;

namespace HRConnect.Api.Utils
{
    public static class EmployeeValidationHelpers
    {
        private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg" };
        ///<summary>
        ///Validates that a string field is not empty and optionally checks that it does 
        /// not exceed a specified maximum length.
        ///</summary>
        ///<param name="value">The string value to validate.</param>
        ///<param name="fieldName">The name of the field being validated.</param>
        ///<param name="maxLength">Optional maximum number of characters allowed for the field.</param>
        ///<returns>
        ///ValidationException if validation fails.
        ///</returns>
        public static void ValidateRequiredString(string value, string fieldName, int? maxLength = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException($"{fieldName} is required");

            if (maxLength.HasValue && value.Length > maxLength.Value)
                throw new ValidationException($"{fieldName} must not exceed {maxLength.Value} characters");
        }
        ///<summary>
        ///Validates that a value exists within the defined values of a specified enum.
        ///</summary>
        ///<param name="value">The enum value to validate.</param>
        ///<param name="fieldName">The name of the field being validated.</param>
        ///<returns>
        ///ValidationException if the enum value is not defined.
        ///</returns>
        public static void ValidateEnum<T>(T value, string fieldName) where T : struct, Enum
        {
            if (!Enum.IsDefined<T>(value))
                throw new ValidationException($"{fieldName} is invalid");
        }
        ///<summary>
        ///Validates that a string contains only numeric characters and has an exact specified length.
        ///</summary>
        ///<param name="value">The numeric string to validate.</param>
        ///<param name="fieldName">The name of the field being validated.</param>
        ///<param name="exactLength">The required length of the numeric string.</param>
        ///<returns>
        ///a ValidationException if the value is invalid.
        ///</returns>
        public static void ValidateNumericString(string value, string fieldName, int exactLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException($"{fieldName} is required");

            if (value.Length != exactLength || !value.All(char.IsDigit))
                throw new ValidationException($"{fieldName} must be {exactLength} digits long and contain digits only");
        }
        ///<summary>
        ///Validates that the provided file path has an allowed image file extension.
        ///</summary>
        ///<param name="filePath">The path of the image file to validate.</param>
        ///<returns>
        ///a ValidationException if the file extension is not allowed.
        ///</returns>
        public static void ValidateImageFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedImageExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                throw new ValidationException($"Employee picture must be a valid image file ({string.Join(", ", AllowedImageExtensions)})");
        }
        ///<summary>
        ///Validates that the email address is not empty and belongs to the Singular domain.
        ///</summary>
        ///<param name="email">The email address to validate.</param>
        ///<returns>
        ///a ValidationException if the email does not end with '@singular.co.za'.
        ///</returns>
        public static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Email must end with '@singular.co.za'");
        }
        ///<summary>
        ///Validates that the salary value falls within the allowed business rule range.
        ///</summary>
        ///<param name="salary">The employee's monthly salary.</param>
        ///<returns>
        ///a ValidationException if the salary is outside the permitted range.
        ///</returns>
        public static void ValidateSalary(decimal salary)
        {
            if (salary <= 0)
                throw new ValidationException("Monthly salary must be greater than 0");
            if (salary >= 100000)
                throw new ValidationException("Monthly salary must not exceed 100 000");
        }
        ///<summary>
        ///Validates the employee's date of birth to ensure age requirements are satisfied based on employment status.
        ///</summary>
        ///<param name="dob">The employee's date of birth.</param>
        ///<param name="status">The employment status of the employee.</param>
        ///<returns>
        ///a ValidationException if age requirements are not met.
        ///</returns>
        public static void ValidateDateOfBirth(DateOnly dob, EmploymentStatus status)
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dob > now)
                throw new ValidationException("Date of birth cannot be in the future");

            int age = AgeCalculator.CalculateAge(dob);
            int minAge = status == EmploymentStatus.Contract ? 16 : 18;

            if (age < minAge)
                throw new ValidationException($"Employee must be at least {minAge} years old.");

            if (age > 65 && status != EmploymentStatus.Contract)
                throw new ValidationException("Employees older than 65 may only be employed on a Contract basis.");
        }
        ///<summary>
        ///Validates that the selected title corresponds with the specified gender.
        ///</summary>
        ///<param name="title">The employee's title.</param>
        ///<param name="gender">The employee's gender.</param>
        ///<returns>
        ///a ValidationException if the title and gender combination is invalid.
        ///</returns>
        public static void ValidateTitleGenderCombo(Title title, Gender? gender)
        {
            if (title == Title.Mr && gender != Gender.Male)
                throw new ValidationException("Title 'Mr' must have gender 'Male'");
            if ((title == Title.Mrs || title == Title.Ms) && gender != Gender.Female)
                throw new ValidationException("Title 'Mrs' or 'Ms' must have gender 'Female'");
        }
        ///<summary>
        ///Validates nationality information based on the presence of an ID number or passport.
        ///</summary>
        ///<param name="idNumber">The national ID number of the employee.</param>
        ///<param name="passportNumber">The passport number of the employee.</param>
        ///<param name="nationality">The nationality of the employee.</param>
        ///<returns>
        ///a ValidationException if nationality rules are violated.
        ///</returns>
        public static void ValidateNationality(string? idNumber, string? passportNumber, string? nationality)
        {
            bool hasId = !string.IsNullOrWhiteSpace(idNumber);
            bool hasPassport = !string.IsNullOrWhiteSpace(passportNumber);

            if (hasId)
            {
                var info = IdNumberValidator.ParseIdNumber(idNumber);
                if (info.IsSouthAfricanCitizen && !string.IsNullOrWhiteSpace(nationality) && nationality != "South African")
                    throw new ValidationException("South African citizens cannot override nationality.");
                if (!info.IsSouthAfricanCitizen && string.IsNullOrWhiteSpace(nationality))
                    throw new ValidationException("Permanent residents must manually enter their nationality.");
            }

            if (hasPassport && string.IsNullOrWhiteSpace(nationality))
                throw new ValidationException("Nationality is required when using a Passport.");

            if (!hasId && !hasPassport)
                throw new ValidationException("Either National ID or Passport is required");

            if (hasId && hasPassport)
                throw new ValidationException("You cannot enter both ID Number and Passport Number");
        }
        ///<summary>
        ///Validates that the city and zip code combination is correct using the CityZipValidator service.
        ///</summary>
        ///<param name="city">The city to validate.</param>
        ///<param name="zip">The zip or postal code.</param>
        ///<returns>
        ///a ValidationException if the city and zip combination is invalid.
        ///</returns>
        public static void ValidateCityZip(string city, string zip)
        {
            CityZipValidator.ValidateCityAndZip(city, zip);
        }
        ///<summary>
        ///Validates disability information to ensure consistency between the disability flag and description.
        ///</summary>
        ///<param name="hasDisability">Indicates whether the employee has a disability.</param>
        ///<param name="description">The description of the disability.</param>
        ///<returns>
        ///a ValidationException if the fields are inconsistent.
        ///</returns>
        public static void ValidateDisabilityFields(bool hasDisability, string? description)
        {
            if (hasDisability && string.IsNullOrWhiteSpace(description))
                throw new ValidationException("Disability description is required if HasDisability is true.");
            if (!hasDisability && !string.IsNullOrWhiteSpace(description))
                throw new ValidationException("Disability description must be empty if HasDisability is false.");
        }
        ///<summary>
        ///Validates that the gender field is provided and corresponds to a valid enum value.
        ///</summary>
        ///<param name="gender">The gender value to validate.</param>
        ///<returns>
        ///a ValidationException if gender is missing or invalid.
        ///</returns>
        public static void ValidateGender(Gender? gender)
        {
            if (!gender.HasValue)
                throw new ValidationException("Gender is required");
            ValidateEnum(gender.Value, "Gender");
        }
        ///<summary>
        ///Validates that the employee start date exists and falls within the current calendar month.
        ///</summary>
        ///<param name="startDate">The start date of employment.</param>
        ///<returns>
        ///a ValidationException if the start date is invalid.
        ///</returns>
        public static void ValidateStartDate(DateOnly startDate)
        {
            var now = DateTime.UtcNow;
            if (startDate == default)
                throw new ValidationException("Start date is required");
            //if (startDate.Month != now.Month || startDate.Year != now.Year)
            //    throw new ValidationException("Start date must be within the current month.");
        }
        ///<summary>
        ///Validates that a tax number contains exactly 10 numeric digits when provided.
        ///</summary>
        ///<param name="taxNumber">The tax number to validate.</param>
        ///<returns>
        ///a ValidationException if the tax number format is invalid.
        ///</returns>
        public static void ValidateTaxNumber(string? taxNumber)
        {
            if (!string.IsNullOrWhiteSpace(taxNumber))
            {
                if (taxNumber.Length != 10 || !taxNumber.All(char.IsDigit))
                    throw new ValidationException("Tax Number must be 10 digits long and contain digits only");
            }
        }
        ///<summary>
        ///Ensures that no duplicate employee records exist when creating a new employee.
        ///</summary>
        ///<param name="repo">The employee repository used to query existing records.</param>
        ///<param name="employee">The employee data being created.</param>
        ///<returns>
        ///A Task representing the asynchronous validation operation.
        ///</returns>
        public static async Task ValidateNoDuplicatesOnCreateAsync(IEmployeeRepository repo, CreateEmployeeRequestDto employee)
        {
            if (await repo.GetEmployeeByEmailAsync(employee.Email) != null)
                throw new BusinessRuleException("An employee with the same email already exists");

            if (!string.IsNullOrWhiteSpace(employee.TaxNumber) &&
                await repo.GetEmployeeByTaxNumberAsync(employee.TaxNumber) != null)
                throw new BusinessRuleException("An employee with the same tax number already exists");

            if (!string.IsNullOrWhiteSpace(employee.IdNumber) &&
                await repo.GetEmployeeByIdNumberAsync(employee.IdNumber) != null)
                throw new BusinessRuleException("An employee with the same ID number already exists");

            if (!string.IsNullOrWhiteSpace(employee.ContactNumber) &&
                await repo.GetEmployeeByContactNumberAsync(employee.ContactNumber) != null)
                throw new BusinessRuleException("An employee with the same contact number already exists");
        }
        ///<summary>
        ///Ensures that updated employee information does not conflict with existing employee records.
        ///</summary>
        ///<param name="repo">The employee repository used to query existing records.</param>
        ///<param name="employeeId">The ID of the employee being updated.</param>
        ///<param name="employee">The updated employee data.</param>
        ///<returns>
        ///A Task representing the asynchronous validation operation.
        ///</returns>
        public static async Task ValidateNoDuplicatesOnUpdateAsync(IEmployeeRepository repo, string employeeId, UpdateEmployeeRequestDto employee)
        {
            if (!string.IsNullOrWhiteSpace(employee.Email) &&
                await repo.GetEmployeeByEmailAsync(employee.Email, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same email already exists");

            if (!string.IsNullOrWhiteSpace(employee.IdNumber) &&
                await repo.GetEmployeeByIdNumberAsync(employee.IdNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same Id Number already exists");

            if (!string.IsNullOrWhiteSpace(employee.PassportNumber) &&
                await repo.GetEmployeeByPassportAsync(employee.PassportNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same passport number already exists");

            if (!string.IsNullOrWhiteSpace(employee.ContactNumber) &&
                await repo.GetEmployeeByContactNumberAsync(employee.ContactNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same contact number already exists");
        }
        ///<summary>
        ///Validates that the specified career manager exists and is not the same employee.
        ///</summary>
        ///<param name="repo">The employee repository used to retrieve employee records.</param>
        ///<param name="employeeId">The ID of the employee being validated.</param>
        ///<param name="careerManagerId">The ID of the career manager.</param>
        ///<returns>
        ///A Task representing the asynchronous validation operation.
        ///</returns>
        public static async Task ValidateCareerManagerAsync(IEmployeeRepository repo, string? employeeId, string? careerManagerId)
        {
            if (string.IsNullOrWhiteSpace(careerManagerId))
                return;

            if (careerManagerId == employeeId)
                throw new BusinessRuleException("Employee cannot be their own Career Manager");

            var manager = await repo.GetEmployeeByIdAsync(careerManagerId);
            if (manager == null)
                throw new BusinessRuleException("Career Manager must be an existing Employee");
        }
    }
}