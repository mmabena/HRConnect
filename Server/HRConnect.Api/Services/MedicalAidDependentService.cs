namespace HRConnect.Api.Services
{
    using System;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.MedicalAidDependent;
    using HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
    using HRConnect.Api.Data;
    using HRConnect.Api.Models;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models.Payroll;
    using HRConnect.Api.Utils;
    using Microsoft.AspNetCore.Identity;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Mappers;
    using System.Text.RegularExpressions;
    using HRConnect.Api.Repository;
    using System.ComponentModel.DataAnnotations;

    public class MedicalAidDependentService : IMedicalAidDependentService
    {
        private readonly ApplicationDBContext _context;
        private readonly IMedicalAidDependentRepository _medicalDependentRepo;
        private readonly IMedicalAidDeductionRepository _medicalAidDeductionRepository;
        private readonly IMedicalAidDeductionService _medicalAidDeductionService;
        private readonly IEmployeeRepository _employeeRepo;

        public MedicalAidDependentService(ApplicationDBContext context, IMedicalAidDeductionRepository medicalAidDeductionRepository, IMedicalAidDependentRepository medicalDependentRepo, IEmployeeRepository employeeRepo, IMedicalAidDeductionService medicalAidDeductionService)
        {
            _context = context;
            _medicalDependentRepo = medicalDependentRepo;
            _medicalAidDeductionRepository = medicalAidDeductionRepository;
            _employeeRepo = employeeRepo;
            _medicalAidDeductionService = medicalAidDeductionService;
        }
        /// <summary>
        /// Retrieves all Medical Aid dependents from the database.
        /// </summary>
        /// <returns>
        /// A list of MedicalAidDependentDTO objects.
        /// </returns>
        public async Task<List<MedicalAidDependentDTO>> GetAllMedicalAidDependentsAsync()
        {
            var dependents = await _medicalDependentRepo.GetAllMedicalAidDependentsAsync();
            return dependents.Select(d => d.ToMedicalAidDependentDto()).ToList();

        }
        /// <summary>
        /// Retrieves a Medical Aid dependent by their dependent ID.
        /// </summary>
        /// <param name="dependentId">The dependent ID.</param>
        /// <returns>
        /// The MedicalAidDependentDTO object if found.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the dependent does not exist.</exception>
        public async Task<MedicalAidDependentDTO> GetMedicalAidDependentsByIdAsync(string dependentId)
        {
            var dependent = await _medicalDependentRepo.GetMedicalAidDependentByIdAsync(dependentId);

            if (dependent == null)
                throw new ValidationException("Dependent does not exist");

            return dependent?.ToMedicalAidDependentDto();

        }
        /// <summary>
        /// Creates a new Medical Aid dependent for an employee.
        /// Validates the dependent information, generates a unique dependent ID, saves the dependent, and updates the employee's Medical Aid deduction.
        /// </summary>
        /// <param name="employeeId">The employee ID associated with the dependent.</param>
        /// <param name="medicalAidDependentRequestDto">The Medical Aid dependent model containing the dependent's details.</param>
        /// <returns>
        /// The created MedicalAidDependentDTO object.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the dependent information is invalid or the employee does not exist.</exception>
        public async Task<MedicalAidDependentDTO> CreateMedicalAidDependentAsync(string employeeId, CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            ValidateCommonFields(medicalAidDependentRequestDto);

            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
                throw new ValidationException("Employee does not exist");

            ExtractIdInfo(medicalAidDependentRequestDto);
            ValidateAdultAge(medicalAidDependentRequestDto);

            ValidateChildAge(medicalAidDependentRequestDto);


            var dependentId = await GenerateDependentId(employeeId);

            var newDependent =
                medicalAidDependentRequestDto.ToMedicalAidDependentFromCreateDTO();

            newDependent.DependentId = dependentId;
            newDependent.EmployeeId = employeeId;

            var createdDependent =
                await _medicalDependentRepo.CreateMedicalAidDependentAsync(newDependent);

            await UpdateMedicalAidDeduction(employeeId);

            return createdDependent.ToMedicalAidDependentDto();

        }
        /// <summary>
        /// Retrieves all Medical Aid dependents associated with a specific employee.
        /// </summary>
        /// <param name="employeeId">The employee ID.</param>
        /// <returns>
        /// A list of MedicalAidDependentDTO objects associated with the employee.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the employee does not exist.</exception>
        public async Task<List<MedicalAidDependentDTO>> GetMedicalAidDependentsByEmployeeIdAsync(string employeeId)
        {
            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
                throw new ValidationException("Employee does not exist");

            var dependent = await _medicalDependentRepo.GetMedicalAidDependentsByEmployeeIdAsync(employeeId);

            return dependent
                .Select(d => d.ToMedicalAidDependentDto())
                .ToList();
        }
        /// <summary>
        /// Validates the Medical Aid dependent information before creating a dependent.
        /// Performs common field validation, ID information extraction, and age validation.
        /// </summary>
        /// <param name="employeeId">The employee ID associated with the dependent.</param>
        /// <param name="medicalAidDependentRequestDto">The Medical Aid dependent model to be validated.</param>
        /// <returns>
        /// A MedicalAidDependentDTO object containing the validated dependent information.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the dependent information is invalid.</exception>
        public async Task<MedicalAidDependentDTO> ValidateMedicalAidDependentAsync(string employeeId, CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            ValidateCommonFields(medicalAidDependentRequestDto);

            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

            ExtractIdInfo(medicalAidDependentRequestDto);
            ValidateAdultAge(medicalAidDependentRequestDto);

            ValidateChildAge(medicalAidDependentRequestDto);

            var newDependent =
                medicalAidDependentRequestDto.ToMedicalAidDependentFromCreateDTO();

            newDependent.EmployeeId = employeeId;

            return newDependent.ToMedicalAidDependentDto();
        }
        /// <summary>
        /// Generates a unique dependent ID based on the employee ID and existing dependent records.
        /// </summary>
        /// <param name="employeeId">The employee ID associated with the dependent.</param>
        /// <returns>
        /// A unique dependent ID for the employee.
        /// </returns>
        private async Task<string> GenerateDependentId(string employeeId)
        {
            var existingIds =
                await _medicalDependentRepo.GetMedicalAidDependentsByEmployeeIdAsync(employeeId);

            int nextNumber = 1;

            if (existingIds.Count > 0)
            {
                nextNumber = existingIds
                    .Select(d => int.Parse(
                        d.DependentId.Split("-D")[1],
                        CultureInfo.InvariantCulture))
                    .Max() + 1;
            }

            return $"{employeeId}-D{nextNumber:D3}";
        }
        /// <summary>
        /// Validates the required Medical Aid dependent fields.
        /// Ensures the ID number or passport number is valid and that required personal information is provided.
        /// </summary>
        /// <param name="medicalAidDependentRequestDto">The Medical Aid dependent model to be validated.</param>
        /// <exception cref="ValidationException">Thrown when the request is null or a required field is missing or invalid.</exception>
        private static void ValidateCommonFields(CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            if (medicalAidDependentRequestDto == null)
                throw new ValidationException("Request cannot be null.");
            if (!string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.IdNumber))
            {
                if (medicalAidDependentRequestDto.IdNumber.Length != 13)
                {
                    throw new ValidationException("ID Number must be 13 digits long.");
                }
            }

            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.FirstName))
                throw new ValidationException("First name is required.");

            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.LastName))
                throw new ValidationException("Last name is required.");

            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.Relationship.ToString()))
            {
                throw new ValidationException(
                    "Relationship is required.");
            }
            if (!string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.PassportNumber))
            {
                string passport = medicalAidDependentRequestDto.PassportNumber.Trim();

                if (passport.Length < 6 ||
                    passport.Length > 20 ||
                    !passport.All(char.IsLetterOrDigit))
                {
                    throw new ValidationException(
                        "Passport Number must contain only letters and numbers between 6 and 20 characters.");
                }
            }

            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.IdNumber) &&
                string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.PassportNumber))
            {
                throw new ValidationException(
                    "Either ID Number or Passport Number is required.");
            }
            if (!string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.IdNumber) &&
                !string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.PassportNumber))
            {
                throw new ValidationException(
                    "Only one of ID Number or Passport Number may be supplied.");
            }
        }
        /// <summary>
        /// Extracts gender and date of birth information from the dependent's ID number.
        /// </summary>
        /// <param name="medicalAidDependentRequestDto">The Medical Aid dependent model containing the ID number.</param>
        private static void ExtractIdInfo(CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.IdNumber))
                return;

            var info = IdNumberValidator.ParseIdNumber(medicalAidDependentRequestDto.IdNumber);

            medicalAidDependentRequestDto.Gender = info.Gender;
            medicalAidDependentRequestDto.DateOfBirth = info.DateOfBirth.ToDateTime(TimeOnly.MinValue);
        }
        /// <summary>
        /// Validates the age of a child dependent.
        /// Ensures that the dependent has a date of birth and is younger than 21 years old.
        /// </summary>
        /// <param name="medicalAidDependentRequestDto">The Medical Aid dependent model to be validated.</param>
        /// <exception cref="ValidationException">Thrown when the date of birth is missing or the child dependent is 21 years or older.</exception>
        private static void ValidateChildAge(CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            if (medicalAidDependentRequestDto.Relationship != Relationship.Child)
                return;

            if (!medicalAidDependentRequestDto.DateOfBirth.HasValue)
                throw new ValidationException(
                    "Date of birth is required.");

            int age = AgeCalculator.CalculateAge(
                DateOnly.FromDateTime(medicalAidDependentRequestDto.DateOfBirth.Value));

            if (age >= 21)
            {
                throw new ValidationException(
                    "A Child dependent cannot be older than 21 years.");
            }

        }
        /// <summary>
        /// Validates the age of an adult dependent.
        /// Ensures that the dependent has a date of birth and is older than 21 years old.
        /// </summary>
        /// <param name="medicalAidDependentRequestDto">The Medical Aid dependent model to be validated.</param>
        /// <exception cref="ValidationException">Thrown when the date of birth is missing or the adult dependent is 21 years or younger.</exception>
        private static void ValidateAdultAge(CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            if (medicalAidDependentRequestDto.Relationship != Relationship.Adult)
                return;

            if (!medicalAidDependentRequestDto.DateOfBirth.HasValue)
                throw new ValidationException(
                    "Date of birth is required.");

            int age = AgeCalculator.CalculateAge(
                DateOnly.FromDateTime(medicalAidDependentRequestDto.DateOfBirth.Value));

            if (age <= 21)
            {
                throw new ValidationException(
                    "A Adult dependent cannot be younger than 21 years.");
            }

        }
        /// <summary>
        /// Updates the Medical Aid deduction for an employee based on their current dependents.
        /// Recalculates the number of adult and child dependents covered by the Medical Aid deduction.
        /// </summary>
        /// <param name="employeeId">The employee ID associated with the Medical Aid deduction.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        private async Task UpdateMedicalAidDeduction(string employeeId)
        {

            Console.WriteLine("====================Updating medical deduction====================");
            var deduction = await _medicalAidDeductionRepository
                .GetActiveMedicalAidDeductionByEmpIdAsync(employeeId);

            Console.WriteLine($"Deduction is null? {deduction == null}");

            if (deduction == null)
                return;



            var dependents = await _medicalDependentRepo.GetMedicalAidDependentsByEmployeeIdAsync(employeeId);

            Console.WriteLine("-------------DEPENDENTS-------------");

            foreach (var d in dependents)
            {
                Console.WriteLine(
                    $"{d.FirstName} {d.LastName} | {d.Relationship}");
            }

            Console.WriteLine("------------------------------------");

            int adultCount =
            dependents.Count(d =>
                d.Relationship == Relationship.Adult);

            int childCount =
              dependents.Count(d =>
              d.Relationship == Relationship.Child);

            Console.WriteLine($"Dependents found: {dependents.Count}====================");
            Console.WriteLine($"AdultCount: {adultCount}====================");
            Console.WriteLine($"ChildCount: {childCount}====================");


            var updateRequest =
              new UpdateMedicalAidDeductionRequestDto
              {
                  MedicalOptionId = deduction.MedicalOptionId,
                  MedicalCategoryId = deduction.MedicalCategoryId,
                  OptionName = deduction.OptionName,
                  OptionCategory = deduction.OptionCategoryName,

                  PrincipalCount = 1,
                  AdultCount = adultCount,
                  ChildrenCount = childCount
              };

            Console.WriteLine("Sending update request...====================");
            Console.WriteLine($"MedicalOptionId: {updateRequest.MedicalOptionId}");
            Console.WriteLine($"AdultCount: {updateRequest.AdultCount}");
            Console.WriteLine($"ChildCount: {updateRequest.ChildrenCount}");
            await _medicalAidDeductionService
      .UpdateDeductionsByEmpIdAsync(employeeId, updateRequest);
        }
        /// <summary>
        /// Converts child dependents who have reached the age of 21 to adult dependents
        /// based on the next payroll period.
        /// </summary>
        /// <param name="currentRun">The current payroll run used to determine the next payroll period.</param>
        /// <returns>A task representing the asynchronous conversion operation.</returns>
        public async Task ConvertChildrenTurning21Async(PayrollRun currentRun)
        {
            Console.WriteLine("========== ConvertChildrenTurning21Async START ==========");

            // Current payroll run number
            int currentRunNumber = currentRun.PayrollRunNumber;

            // Determine the next payroll run
            int nextRunNumber = currentRunNumber == 12
                ? 1
                : currentRunNumber + 1;

            // Convert payroll run to calendar month
            int nextPayrollMonth = PayrollRunToCalendarMonth(nextRunNumber);

            // Determine the calendar year for the next payroll
            int nextPayrollYear;

            if (nextPayrollMonth >= 4)
            {
                // April - December belong to the financial year's start year
                nextPayrollYear = currentRun.Period.StartDate.Year;
            }
            else
            {
                // January - March belong to the following calendar year
                nextPayrollYear = currentRun.Period.StartDate.Year + 1;
            }

            DateTime payrollDate = new DateTime(nextPayrollYear, nextPayrollMonth, 1);

            Console.WriteLine($"Payroll Date Used: {payrollDate:d}");

            var dependents = await _medicalDependentRepo.GetAllMedicalAidDependentsAsync();

            foreach (var dep in dependents)
            {
                Console.WriteLine(
                    $"{dep.DependentId} | {dep.FirstName} | {dep.Relationship} | {dep.DateOfBirth}");

                if (dep.Relationship != Relationship.Child)
                    continue;

                if (!dep.DateOfBirth.HasValue)
                    continue;

                DateTime twentyFirstBirthday = dep.DateOfBirth.Value.AddYears(21);

                Console.WriteLine($"21st Birthday : {twentyFirstBirthday:d}");

                if (twentyFirstBirthday <= payrollDate)
                {
                    Console.WriteLine($"Converting {dep.DependentId} to Adult");

                    dep.Relationship = Relationship.Adult;
                    dep.UpdatedDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            Console.WriteLine("========== ConvertChildrenTurning21Async END ==========");
        }
        /// <summary>
        /// Converts a payroll run number to its corresponding calendar month.
        /// </summary>
        /// <param name="payrollRunNumber">The payroll run number from 1 to 12.</param>
        /// <returns>
        /// The calendar month associated with the payroll run number.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the payroll run number is outside the range of 1 to 12.</exception>
        private int PayrollRunToCalendarMonth(int payrollRunNumber)
        {
            return payrollRunNumber switch
            {
                1 => 4,   // April
                2 => 5,
                3 => 6,
                4 => 7,
                5 => 8,
                6 => 9,
                7 => 10,
                8 => 11,
                9 => 12,
                10 => 1, // January
                11 => 2,
                12 => 3, // March
                _ => throw new ArgumentOutOfRangeException(nameof(payrollRunNumber))
            };
        }

    }
}