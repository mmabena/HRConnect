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

        public static void ValidateRequiredString(string value, string fieldName, int? maxLength = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException($"{fieldName} is required");

            if (maxLength.HasValue && value.Length > maxLength.Value)
                throw new ValidationException($"{fieldName} must not exceed {maxLength.Value} characters");
        }

        public static void ValidateEnum<T>(T value, string fieldName) where T : struct, Enum
        {
            if (!Enum.IsDefined<T>(value))
                throw new ValidationException($"{fieldName} is invalid");
        }

        public static void ValidateNumericString(string value, string fieldName, int exactLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException($"{fieldName} is required");

            if (value.Length != exactLength || !value.All(char.IsDigit))
                throw new ValidationException($"{fieldName} must be {exactLength} digits long and contain digits only");
        }

        public static void ValidateImageFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedImageExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                throw new ValidationException($"Employee picture must be a valid image file ({string.Join(", ", AllowedImageExtensions)})");
        }

        public static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Email must end with '@singular.co.za'");
        }

        public static void ValidateSalary(decimal salary)
        {
            if (salary <= 0)
                throw new ValidationException("Monthly salary must be greater than 0");
            if (salary >= 100000)
                throw new ValidationException("Monthly salary must not exceed 100 000");
        }

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

        public static void ValidateTitleGenderCombo(Title title, Gender? gender)
        {
            if (title == Title.Mr && gender != Gender.Male)
                throw new ValidationException("Title 'Mr' must have gender 'Male'");
            if ((title == Title.Mrs || title == Title.Ms) && gender != Gender.Female)
                throw new ValidationException("Title 'Mrs' or 'Ms' must have gender 'Female'");
        }


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

        public static void ValidateCityZip(string city, string zip)
        {
            CityZipValidator.ValidateCityAndZip(city, zip);
        }

        public static void ValidateDisabilityFields(bool hasDisability, string? description)
        {
            if (hasDisability && string.IsNullOrWhiteSpace(description))
                throw new ValidationException("Disability description is required if HasDisability is true.");
            if (!hasDisability && !string.IsNullOrWhiteSpace(description))
                throw new ValidationException("Disability description must be empty if HasDisability is false.");
        }

        public static void ValidateGender(Gender? gender)
        {
            if (!gender.HasValue)
                throw new ValidationException("Gender is required");
            ValidateEnum(gender.Value, "Gender");
        }
        public static void ValidateStartDate(DateOnly startDate)
        {
            var now = DateTime.UtcNow;
            if (startDate == default)
                throw new ValidationException("Start date is required");
            if (startDate.Month != now.Month || startDate.Year != now.Year)
                throw new ValidationException("Start date must be within the current month.");
        }
        public static void ValidateTaxNumber(string? taxNumber)
        {
            if (!string.IsNullOrWhiteSpace(taxNumber))
            {
                if (taxNumber.Length != 10 || !taxNumber.All(char.IsDigit))
                    throw new ValidationException("Tax Number must be 10 digits long and contain digits only");
            }
        }

        public static async Task ValidateNoDuplicatesOnCreateAsync(IEmployeeRepository repo, CreateEmployeeRequestDto employee)
        {
            if (await repo.GetEmployeeByEmailAsync(employee.Email) != null)
                throw new BusinessRuleException("Email is already in use");

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