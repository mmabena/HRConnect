namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class BankingDetailService : IBankingDetailService
    {
        private readonly IBankingDetailRepository _bankingDetailRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public BankingDetailService(
            IBankingDetailRepository bankingDetailRepo,
            IEmployeeRepository employeeRepo)
        {
            _bankingDetailRepo = bankingDetailRepo;
            _employeeRepo = employeeRepo;
        }

        // ======================================================
        // GET
        // ======================================================
        public async Task<BankingDetailDto> GetBankingDetailsAsync(string EmployeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EmployeeId))
                    throw new ValidationException("EmployeeId is required");

                var normalizedId = EmployeeId.Trim();

                var details = await _bankingDetailRepo
                    .GetBankingDetailsByEmployeeIdAsync(normalizedId);

                if (details == null)
                    throw new KeyNotFoundException($"No banking details found for {normalizedId}");

                return MapToBankingDetailDto(details);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
                throw; // don't hide real error
            }
        }

        // ======================================================
        // CREATE OR UPDATE
        // ======================================================
        public async Task<BankingDetailDto> CreateBankingDetailsAsync(
            CreateBankingDetailDto createBankingDetailsDto)
        {
            ValidateCommonFields(createBankingDetailsDto);

            if (string.IsNullOrWhiteSpace(createBankingDetailsDto.EmployeeId))
                throw new ValidationException("EmployeeId is required");

            // ======================================================
            // CHECK EMPLOYEE EXISTS
            // ======================================================
            var employee = await _employeeRepo.GetEmployeeByIdAsync(createBankingDetailsDto.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {createBankingDetailsDto.EmployeeId} not found");

            // ======================================================
            // CHECK EMPLOYMENT STATUS
            // ======================================================
            if (employee.EmploymentStatus != EmploymentStatus.Permanent &&
                employee.EmploymentStatus != EmploymentStatus.FixedTerm)
            {
                throw new ValidationException(
                    "Banking details are only allowed for Permanent or Fixed-Term employees"
                );
            }

            // ======================================================
            // VALIDATE BANKING RULES
            // ======================================================
            BankDetailsValidations.ValidateBankingDetails(
                createBankingDetailsDto.BankName.ToString()!,
                createBankingDetailsDto.AccountNumber
            );

            // ======================================================
            // CHECK IF EXISTS
            // ======================================================
            var existing = await _bankingDetailRepo
                .GetBankingDetailsByEmployeeIdAsync(createBankingDetailsDto.EmployeeId);

            // ======================================================
            // UPDATE EXISTING
            // ======================================================
            if (existing != null)
            {
                existing.Name = createBankingDetailsDto.Name;
                existing.Surname = createBankingDetailsDto.Surname;
                existing.IdNumber = createBankingDetailsDto.IdNumber;
                existing.PassportNumber = createBankingDetailsDto.PassportNumber;
                existing.BankName = createBankingDetailsDto.BankName;
                existing.AccountNumber = createBankingDetailsDto.AccountNumber;
                existing.AccountType = createBankingDetailsDto.AccountType;
                existing.BranchCode = createBankingDetailsDto.BranchCode;
                existing.UpdatedAt = DateTime.UtcNow;

                await _bankingDetailRepo.UpdateBankingDetailsAsync(existing);

                return MapToBankingDetailDto(existing);
            }

            // ======================================================
            // CREATE NEW
            // ======================================================
            var details = new BankingDetail
            {
                EmployeeId = createBankingDetailsDto.EmployeeId,
                Name = createBankingDetailsDto.Name,
                Surname = createBankingDetailsDto.Surname,
                IdNumber = createBankingDetailsDto.IdNumber,
                PassportNumber = createBankingDetailsDto.PassportNumber,
                BankName = createBankingDetailsDto.BankName,
                AccountNumber = createBankingDetailsDto.AccountNumber,
                AccountType = createBankingDetailsDto.AccountType,
                BranchCode = createBankingDetailsDto.BranchCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _bankingDetailRepo.CreateBankingDetailsAsync(details);


            employee.BankingDetailsId = result.BankingDetailsId;
            await _employeeRepo.UpdateEmployeeAsync(employee);

            return MapToBankingDetailDto(result);
        }

        // ======================================================
        // UPDATE ONLY
        // ======================================================
        public async Task<BankingDetailDto> UpdateBankingDetailsAsync(string EmployeeId, UpdateBankingDetailDto updatebankingDetailsDto)
        {
            var normalizedEmployeeId = EmployeeId?.Trim();
            // 1. Check employee exists FIRST
            var employee = await _employeeRepo.GetEmployeeByIdAsync(normalizedEmployeeId);

            if (employee == null)
                throw new KeyNotFoundException($"Employee {normalizedEmployeeId} does not exist");

            // 2. Check banking details exist
            var existing = await _bankingDetailRepo
                .GetBankingDetailsByEmployeeIdAsync(normalizedEmployeeId);

            if (existing == null)
                throw new KeyNotFoundException(
                    $"Banking details not found for employee {normalizedEmployeeId}. Create them first."
                );

            // 3. Validate
            ValidateUpdate(updatebankingDetailsDto);

            BankDetailsValidations.ValidateBankingDetails(
                updatebankingDetailsDto.BankName.ToString()!,
                updatebankingDetailsDto.AccountNumber
            );

            // 4. Update
            existing.BankName = updatebankingDetailsDto.BankName;
            existing.AccountNumber = updatebankingDetailsDto.AccountNumber;
            existing.BranchCode = updatebankingDetailsDto.BranchCode;
            existing.AccountType = updatebankingDetailsDto.AccountType;
            existing.UpdatedAt = DateTime.UtcNow;

            await _bankingDetailRepo.UpdateBankingDetailsAsync(existing);

            return MapToBankingDetailDto(existing);
        }
        // ======================================================
        // VALIDATIONS
        // ======================================================
        private void ValidateCommonFields(CreateBankingDetailDto dto)
        {
            if (dto == null)
                throw new ValidationException("Request cannot be null");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Name is required");

            if (string.IsNullOrWhiteSpace(dto.Surname))
                throw new ValidationException("Surname is required");

            if (string.IsNullOrWhiteSpace(dto.AccountNumber))
                throw new ValidationException("Account number is required");

            if (string.IsNullOrWhiteSpace(dto.BranchCode))
                throw new ValidationException("Branch code is required");
        }

        private void ValidateUpdate(UpdateBankingDetailDto dto)
        {
            if (dto == null)
                throw new ValidationException("Request cannot be null");

            if (!Enum.IsDefined<BankName>(dto.BankName))
                throw new ValidationException("Invalid bank name");

            if (string.IsNullOrWhiteSpace(dto.AccountNumber))
                throw new ValidationException("Account number is required");

            if (string.IsNullOrWhiteSpace(dto.BranchCode))
                throw new ValidationException("Branch code is required");
        }

        // ======================================================
        // MAPPER
        // ======================================================
        private static BankingDetailDto MapToBankingDetailDto(BankingDetail d)
        {
            return new BankingDetailDto
            {
                BankingDetailsId = d.BankingDetailsId,
                Name = d.Name,
                Surname = d.Surname,
                IdNumber = d.IdNumber,
                PassportNumber = d.PassportNumber,
                BankName = d.BankName,
                AccountType = d.AccountType,
                AccountNumber = d.AccountNumber,
                BranchCode = d.BranchCode,
                NetSalary = d.NetSalary,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            };
        }
    }
}