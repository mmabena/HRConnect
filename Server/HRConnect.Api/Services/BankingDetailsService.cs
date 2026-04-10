namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using System;
    using System.Threading.Tasks;


    public class BankingDetailsService : IBankingDetailsService
    {
        private readonly ApplicationDBContext _context;

        private readonly IBankingDetailsRepository _bankingDetailsRepo;
       

        public BankingDetailsService(ApplicationDBContext context, IBankingDetailsRepository bankingDetailsRepo)
        {
            _context = context;
            _bankingDetailsRepo = bankingDetailsRepo;
            
        }

        // ======================================================
        // POST - CREATE
        // ======================================================
        public async Task<BankingDetailsDto> CreateBankingDetailsAsync(CreateBankingDetailsDto createbankingDetailsDto)
        {
            ValidateCommonFields(createbankingDetailsDto);

            BankDetailsValidations.ValidateBankingDetails(createbankingDetailsDto);
            BankDetailsValidations.ValidateIdentification(createbankingDetailsDto);

            await CheckDuplicates(createbankingDetailsDto);
            await ValidateTempEmployeeAsync(createbankingDetailsDto.TempEmployeeId);

            var entity = new BankingDetails
            {
                TempEmployeeId = createbankingDetailsDto.TempEmployeeId,
                Name = createbankingDetailsDto.Name,
                Surname = createbankingDetailsDto.Surname,
                IdNumber = createbankingDetailsDto.IdNumber,
                PassportNumber = createbankingDetailsDto.PassportNumber,
                BankName = createbankingDetailsDto.BankName,
                AccountNumber = createbankingDetailsDto.AccountNumber,
                BranchCode = createbankingDetailsDto.BranchCode,
                NetSalary = createbankingDetailsDto.NetSalary,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var result = await _bankingDetailsRepo.AddAsync(entity);
            await _bankingDetailsRepo.SaveChangesAsync();

            return BankingDetailsMapper.ToDto(result);
        }

        // ======================================================
        // GET - BY TEMP EMPLOYEE ID
        // ======================================================
        public async Task<BankingDetailsDto> GetByTempEmployeeIdAsync(int tempEmployeeId)
        {
            var entity = await _bankingDetailsRepo.GetByTempEmployeeIdAsync(tempEmployeeId);

            if (entity == null)
                throw new Exception("Banking details not found");

            return BankingDetailsMapper.ToDto(entity);
        }

        // ======================================================
        // PUT - UPDATE
        // ======================================================
        public async Task<BankingDetailsDto> UpdateBankingDetailsAsync(int tempEmployeeId, UpdateBankingDetailsDto updatebankingDetailsDto)
        {
            var existing = await _bankingDetailsRepo.GetByTempEmployeeIdAsync(tempEmployeeId);

            if (existing == null)
                throw new Exception("Banking details not found");

            // Validate updated fields
            ValidateUpdate(updatebankingDetailsDto);

            BankDetailsRules.ValidateBankingDetails(updatebankingDetailsDto);
            BankDetailsRules.ValidateIdentification(updatebankingDetailsDto);

            // Update fields
            existing.Name = updatebankingDetailsDto.Name;
            existing.Surname = updatebankingDetailsDto.Surname;
            existing.BankName = updatebankingDetailsDto.BankName;
            existing.AccountNumber = updatebankingDetailsDto.AccountNumber;
            existing.BranchCode = updatebankingDetailsDto.BranchCode;
            existing.NetSalary = updatebankingDetailsDto.NetSalary;
            existing.IdNumber = updatebankingDetailsDto.IdNumber;
            existing.PassportNumber = updatebankingDetailsDto.PassportNumber;
            existing.UpdatedDate = DateTime.UtcNow;

            await _bankingDetailsRepo.UpdateAsync(existing);
            await _bankingDetailsRepo.SaveChangesAsync();

            return BankingDetailsMapper.ToDto(existing);
        }

        // ======================================================
        // COMMON VALIDATION (CREATE)
        // ======================================================
        private void ValidateCommonFields(CreateBankingDetailsDto dto)
        {
            if (dto == null)
                throw new ValidationException("Request cannot be null");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Name is required");

            if (string.IsNullOrWhiteSpace(dto.Surname))
                throw new ValidationException("Surname is required");

            if (string.IsNullOrWhiteSpace(dto.BankName))
                throw new ValidationException("Bank name is required");

            if (string.IsNullOrWhiteSpace(dto.AccountNumber))
                throw new ValidationException("Account number is required");

            if (string.IsNullOrWhiteSpace(dto.BranchCode))
                throw new ValidationException("Branch code is required");

            if (dto.TempEmployeeId <= 0)
                throw new ValidationException("Invalid temporary employee ID");
        }

        // ======================================================
        // UPDATE VALIDATION
        // ======================================================
        private void ValidateUpdate(UpdateBankingDetailsDto dto)
        {
            if (dto == null)
                throw new ValidationException("Request cannot be null");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Name is required");

            if (string.IsNullOrWhiteSpace(dto.Surname))
                throw new ValidationException("Surname is required");

            if (string.IsNullOrWhiteSpace(dto.BankName))
                throw new ValidationException("Bank name is required");
        }

        // ======================================================
        // DUPLICATES CHECK
        // ======================================================
        private async Task CheckDuplicates(CreateBankingDetailsDto dto)
        {
            var exists = await _bankingDetailsRepo.ExistsAsync(x =>
                x.AccountNumber == dto.AccountNumber);

            if (exists)
                throw new ValidationException("Account number already exists");

            var tempExists = await _bankingDetailsRepo.ExistsAsync(x =>
                x.TempEmployeeId == dto.TempEmployeeId);

            if (tempExists)
                throw new ValidationException("Banking details already exist for this employee");
        }

       
    }
}