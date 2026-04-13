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

        public BankingDetailService(IBankingDetailRepository bankingDetailRepo)
        {
            _bankingDetailRepo = bankingDetailRepo;
        }

        public async Task<BankingDetailDto> GetBankingDetailsAsync(string employeeId)
        {
            var details = await _bankingDetailRepo.GetBankingDetailsByEmployeeIdAsync(employeeId);

            if (details == null)
                throw new KeyNotFoundException("Banking details not found for the specified employee ID");

            return MapToBankingDetailDto(details);
        }


      public async Task<BankingDetailDto> CreateBankingDetailsAsync(CreateBankingDetailDto createBankingDetailDto)
{
    ValidateCommonFields(createBankingDetailDto);

    BankDetailsValidations.ValidateBankingDetails(
        createBankingDetailDto.BankName.ToString()!,
        createBankingDetailDto.AccountNumber
    );

    // 1. Check if employee already has banking details
    var existing = await _bankingDetailRepo.GetBankingDetailsByEmployeeIdAsync(createBankingDetailDto.EmployeeId);

    // ======================================================
    // CASE 1: UPDATE existing banking details
    // ======================================================
    if (existing != null)
    {
        existing.BankName = createBankingDetailDto.BankName;
        existing.AccountNumber = createBankingDetailDto.AccountNumber;
        existing.AccountType = createBankingDetailDto.AccountType;
        existing.BranchCode = createBankingDetailDto.BranchCode;
        existing.IdNumber = createBankingDetailDto.IdNumber;
        existing.PassportNumber = createBankingDetailDto.PassportNumber;
        existing.UpdatedAt = DateTime.UtcNow;

        await _bankingDetailRepo.UpdateBankingDetailsAsync(existing);

        return MapToBankingDetailDto(existing);
    }

    // ======================================================
    // CASE 2: CREATE new banking details
    // ======================================================
    var details = new BankingDetail
    {
        EmployeeId = createBankingDetailDto.EmployeeId, // IMPORTANT
        Name = createBankingDetailDto.Name,
        Surname = createBankingDetailDto.Surname,
        IdNumber = createBankingDetailDto.IdNumber,
        PassportNumber = createBankingDetailDto.PassportNumber,
        BankName = createBankingDetailDto.BankName,
        AccountNumber = createBankingDetailDto.AccountNumber,
        AccountType = createBankingDetailDto.AccountType,
        BranchCode = createBankingDetailDto.BranchCode,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    var result = await _bankingDetailRepo.CreateBankingDetailsAsync(details);

    return MapToBankingDetailDto(result);
}


        public async Task<BankingDetailDto> UpdateBankingDetailsAsync(string employeeId, UpdateBankingDetailDto updatebankingDetailsDto)
        {
            var existing = await _bankingDetailRepo.GetBankingDetailsByEmployeeIdAsync(employeeId);

            if (existing == null)
                throw new KeyNotFoundException(
                    $"No banking details found for employee ID: {employeeId}. Please create banking details first before updating."
                );

            ValidateUpdate(updatebankingDetailsDto);

            BankDetailsValidations.ValidateBankingDetails(
                updatebankingDetailsDto.BankName.ToString()!,
                updatebankingDetailsDto.AccountNumber
            );

            existing.BankName = updatebankingDetailsDto.BankName;
            existing.AccountNumber = updatebankingDetailsDto.AccountNumber;
            existing.BranchCode = updatebankingDetailsDto.BranchCode;
            existing.AccountType = updatebankingDetailsDto.AccountType;
            existing.UpdatedAt = DateTime.UtcNow;

            await _bankingDetailRepo.UpdateBankingDetailsAsync(existing);

            return MapToBankingDetailDto(existing);
        }
        // ======================================================
        // CREATE VALIDATION
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

        // ======================================================
        // UPDATE VALIDATION
        // ======================================================
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