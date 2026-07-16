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
    using HRConnect.Api.Utils;
    using Microsoft.AspNetCore.Identity;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Mappers;
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

        public async Task<List<MedicalAidDependentDTO>> GetAllMedicalAidDependentsAsync()
        {
            var dependents = await _medicalDependentRepo.GetAllMedicalAidDependentsAsync();
            return dependents.Select(d => d.ToMedicalAidDependentDto()).ToList();

        }
        public async Task<MedicalAidDependentDTO> GetMedicalAidDependentsByIdAsync(string dependentId)
        {
            var dependent = await _medicalDependentRepo.GetMedicalAidDependentByIdAsync(dependentId);

            if (dependent == null)
                throw new ValidationException("Dependent does not exist");

            return dependent?.ToMedicalAidDependentDto();

        }
        public async Task<MedicalAidDependentDTO> CreateMedicalAidDependentAsync(string employeeId, CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            ValidateCommonFields(medicalAidDependentRequestDto);

            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
                throw new ValidationException("Employee does not exist");

            ExtractIdInfo(medicalAidDependentRequestDto);

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

        private static void ValidateCommonFields(CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            if (medicalAidDependentRequestDto == null)
                throw new ValidationException("Request cannot be null.");
            if (medicalAidDependentRequestDto.IdNumber != null && medicalAidDependentRequestDto.IdNumber.Length != 13)
                throw new ValidationException("ID Number must be 13 digits long.");

            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.FirstName))
                throw new ValidationException("First name is required.");

            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.LastName))
                throw new ValidationException("Last name is required.");

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

        private static void ExtractIdInfo(CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            if (string.IsNullOrWhiteSpace(medicalAidDependentRequestDto.IdNumber))
                return;

            var info = IdNumberValidator.ParseIdNumber(medicalAidDependentRequestDto.IdNumber);

            medicalAidDependentRequestDto.Gender = info.Gender;
            medicalAidDependentRequestDto.DateOfBirth = info.DateOfBirth.ToDateTime(TimeOnly.MinValue);
        }
        private static void ValidateChildAge(CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            if (medicalAidDependentRequestDto.Relationship != Relationship.Child)
                return;

            if (!medicalAidDependentRequestDto.DateOfBirth.HasValue)
                throw new ValidationException(
                    "Date of birth is required.");

            int age = AgeCalculator.CalculateAge(
                DateOnly.FromDateTime(medicalAidDependentRequestDto.DateOfBirth.Value));

            if (age > 21)
            {
                throw new ValidationException(
                    "A Child dependent cannot be older than 21 years.");
            }
        }

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
                d.Relationship == Relationship.Spouse ||
                d.Relationship == Relationship.Parent ||
                d.Relationship == Relationship.Sibling ||
                d.Relationship == Relationship.Other);

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

    }
}