namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using HRConnect.Api.Utils.Security;
    using HRConnect.Api.Utils.BankingDetailsValidation;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class BankingDetailService : IBankingDetailService
    {
        private readonly IBankingDetailRepository _bankingDetailRepo;
        private readonly IEncryptionService _encryptionService;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILogger<BankingDetailService> _logger;
        private readonly HashingHelper _hashingHelper;

        public BankingDetailService(
            IBankingDetailRepository bankingDetailRepo,
            IEncryptionService encryptionService,
            ILogger<BankingDetailService> logger,
            IEmployeeRepository employeeRepo,
            HashingHelper hashingHelper)
        {
            _bankingDetailRepo = bankingDetailRepo;
            _encryptionService = encryptionService;
            _employeeRepo = employeeRepo;
            _logger = logger;
            _hashingHelper = hashingHelper;
        }

        // ======================================================
        // GET ALL
        // ======================================================
        public async Task<List<BankingDetailDto>> GetAllBankingDetailsAsync()
        {
            var bankingDetails = await _bankingDetailRepo.GetAllBankingDetailsAsync();

            var dtos = new List<BankingDetailDto>();

            foreach (var detail in bankingDetails)
            {
                dtos.Add(MapToBankingDetailDto(detail));
            }

            return dtos;
        }

        // ======================================================
        // GET BY EMPLOYEE ID
        // ======================================================
        public async Task<BankingDetailDto?> GetBankingDetailsByEmployeeIdAsync(string EmployeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EmployeeId))
                    throw new ValidationException("EmployeeId is required");

                var details = await _bankingDetailRepo
                    .GetBankingDetailsByEmployeeIdAsync(EmployeeId.Trim());

                if (details == null)
                    throw new KeyNotFoundException("Banking details not found");

                return MapToBankingDetailDto(details);
            }
            catch (Exception ex)
            {
                _logError(_logger, EmployeeId, ex);
                throw;
            }
        }

        // ======================================================
        // CREATE (FIXED LOGIC)
        // ======================================================
        public async Task<BankingDetailDto> CreateBankingDetailsAsync(
            CreateBankingDetailDto createBankingDetailsDto)
        {
            ValidateCommonFields(createBankingDetailsDto);

            var employee = await _employeeRepo.GetEmployeeByIdAsync(createBankingDetailsDto.EmployeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            var normalized = _hashingHelper.Normalize(createBankingDetailsDto.AccountNumber);
            var searchHash = _hashingHelper.ComputeSearchHash(normalized);

            var duplicate = await _bankingDetailRepo.AnyAsync(x =>
                x.AccountNumberSearchHash == searchHash);

            if (duplicate)
                throw new ValidationException("Account number already exists for another employee");

            BankDetailsValidations.ValidateBankingDetails(
                createBankingDetailsDto.BankName.ToString(),
                normalized);

            var entity = new BankingDetail
            {
                EmployeeId = createBankingDetailsDto.EmployeeId,
                Name = createBankingDetailsDto.Name,
                Surname = createBankingDetailsDto.Surname,
                IdNumber = createBankingDetailsDto.IdNumber,
                PassportNumber = createBankingDetailsDto.PassportNumber,
                BankName = createBankingDetailsDto.BankName,

                AccountNumberEncrypted = _encryptionService.Encrypt(normalized),
                AccountNumberSearchHash = searchHash,
                AccountNumberLast4Digits = normalized.Length >= 4 ? normalized[^4..] : normalized,

                AccountType = createBankingDetailsDto.AccountType,
                BranchCode = createBankingDetailsDto.BranchCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _bankingDetailRepo.CreateBankingDetailsAsync(entity);

            employee.BankingDetailsId = result.BankingDetailsId;
            await _employeeRepo.UpdateEmployeeAsync(employee);

            return MapToBankingDetailDto(result);
        }

        // ======================================================
        // UPDATE (FIXED LOGIC)
        // ======================================================
        public async Task<BankingDetailDto> UpdateBankingDetailsAsync(
            string EmployeeId,
            UpdateBankingDetailDto updatebankingDetailsDto)
        {
            var normalizedId = EmployeeId?.Trim();

            var employee = await _employeeRepo.GetEmployeeByIdAsync(normalizedId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            var existing = await _bankingDetailRepo
                .GetBankingDetailsByEmployeeIdAsync(normalizedId);

            if (existing == null)
                throw new KeyNotFoundException("Banking details not found");

            if (existing.IsLocked)
                throw new ValidationException("Banking details are locked");

            var normalized = _hashingHelper.Normalize(updatebankingDetailsDto.AccountNumber);
            var searchHash = _hashingHelper.ComputeSearchHash(normalized);

            var duplicate = await _bankingDetailRepo.AnyAsync(x =>
                x.AccountNumberSearchHash == searchHash &&
                x.EmployeeId != normalizedId);

            if (duplicate)
                throw new ValidationException("Account number already exists for another employee");

            BankDetailsValidations.ValidateBankingDetails(
                updatebankingDetailsDto.BankName.ToString(),
                normalized);

            existing.BankName = updatebankingDetailsDto.BankName;
            existing.AccountNumberEncrypted = _encryptionService.Encrypt(normalized);
            existing.AccountNumberSearchHash = searchHash;
            existing.AccountNumberLast4Digits = normalized.Length >= 4 ? normalized[^4..] : normalized;
            existing.AccountType = updatebankingDetailsDto.AccountType;
            existing.BranchCode = updatebankingDetailsDto.BranchCode;
            existing.UpdatedAt = DateTime.UtcNow;

            await _bankingDetailRepo.UpdateBankingDetailsAsync(existing);

            return MapToBankingDetailDto(existing);
        }

        // ======================================================
        // LOCK ALL
        // ======================================================
        public async Task LockAllBankingDetailsAsync()
        {
            var all = await _bankingDetailRepo.GetAllBankingDetailsAsync();

            foreach (var detail in all)
            {
                if (!detail.IsLocked)
                {
                    detail.IsLocked = true;
                    detail.LockedAt = DateTime.UtcNow;

                    await _bankingDetailRepo.UpdateBankingDetailsAsync(detail);
                }
            }
        }

        // ======================================================
        // VALIDATION
        // ======================================================
        private void ValidateCommonFields(CreateBankingDetailDto dto)
        {
            if (dto == null)
                throw new ValidationException("Request cannot be null");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Name required");

            if (string.IsNullOrWhiteSpace(dto.Surname))
                throw new ValidationException("Surname required");

            if (string.IsNullOrWhiteSpace(dto.AccountNumber))
                throw new ValidationException("Account number required");
        }

        // ======================================================
        // MAPPER
        // ======================================================
        private BankingDetailDto MapToBankingDetailDto(BankingDetail d)
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
                AccountNumber = _encryptionService.Decrypt(d.AccountNumberEncrypted),
                BranchCode = d.BranchCode,
                NetSalary = d.NetSalary,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            };
        }

        // ======================================================
        // LOGGER
        // ======================================================
        private static readonly Action<ILogger, string, Exception?> _logError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(GetBankingDetailsByEmployeeIdAsync)),
            "Error retrieving banking details for {EmployeeId}");
    }
}