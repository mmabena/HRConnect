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

        /// <summary>
        /// Retrieves all banking details from the database.
        /// </summary>
        /// <returns>
        /// A list of BankingDetailDto objects.
        /// </returns>
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
        /// <summary>
        /// Retrieves all bank branch codes from the database.
        /// </summary>
        /// <returns>
        /// A list of BankBranchCodeDto objects.
        /// </returns>
        public async Task<List<BankBranchCodeDto>> GetAllBankBranchCodesAsync()
        {
            var branchCodes = await _bankingDetailRepo.GetAllBankBranchCodesAsync();

            var dtos = branchCodes.Select(b => new BankBranchCodeDto
            {
                BankBranchCodeId = b.BankBranchCodeId,
                BankName = b.BankName,
                UniversalCode = b.UniversalCode
            }).ToList();

            return dtos;
        }
        /// <summary>
        /// Retrieves banking details associated with a specific employee.
        /// </summary>
        /// <param name="EmployeeId">The employee ID.</param>
        /// <returns>
        /// The BankingDetailDto object if found, otherwise null.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the employee ID is empty or null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when banking details are not found for the employee.</exception>
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

        /// <summary>
        /// Creates new banking details for an employee.
        /// Validates the banking details, checks for duplicate account numbers, encrypts sensitive information, and associates the banking details with the employee.
        /// </summary>
        /// <param name="createBankingDetailsDto">The banking details model to be created.</param>
        /// <returns>
        /// The created BankingDetailDto object.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the banking details are invalid or the account number already exists.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the employee does not exist.</exception>
        public async Task<BankingDetailDto> CreateBankingDetailsAsync(
            CreateBankingDetailDto createBankingDetailsDto)
        {
            ValidateCommonFields(createBankingDetailsDto);

            var employee = await _employeeRepo.GetEmployeeByIdAsync(createBankingDetailsDto.EmployeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            // Normalize and hash the account number for duplicate checking
            var normalized = _hashingHelper.Normalize(createBankingDetailsDto.AccountNumber);
            var searchHash = _hashingHelper.ComputeSearchHash(normalized);

            // Check for duplicates across all records, excluding the current employees record (if updating)
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

                BankBranchCodeId = createBankingDetailsDto.BankBranchCodeId,
                AccountType = createBankingDetailsDto.AccountType,

                PaymentMethod = createBankingDetailsDto.PaymentMethod,
                PayFrequency = createBankingDetailsDto.PayFrequency,
                ReferenceType = createBankingDetailsDto.ReferenceType,

                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _bankingDetailRepo.CreateBankingDetailsAsync(entity);

            employee.BankingDetailsId = result.BankingDetailsId;
            await _employeeRepo.UpdateEmployeeAsync(employee);

            return MapToBankingDetailDto(result);
        }

        /// <summary>
        /// Updates banking details for a given employee. 
        /// Validates input, checks for duplicates, and ensures that the banking details are not locked before allowing updates.
        /// </summary>
        /// <param name="EmployeeId"> </param>
        /// <param name="updatebankingDetailsDto"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
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
            existing.BankBranchCodeId = updatebankingDetailsDto.BankBranchCodeId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _bankingDetailRepo.UpdateBankingDetailsAsync(existing);

            return MapToBankingDetailDto(existing);
        }
        /// <summary>
        /// Validates banking details before they are created or saved.
        /// Checks required fields, duplicate account numbers, and banking detail validation rules.
        /// </summary>
        /// <param name="createBankingDetailsDto">The banking details model to be validated.</param>
        /// <exception cref="ValidationException">Thrown when the banking details are invalid or the account number already exists.</exception>
        public async Task ValidateBankingDetailsAsync(CreateBankingDetailDto createBankingDetailsDto)
        {
            ValidateCommonFields(createBankingDetailsDto);

            // Normalize and hash the account number for duplicate checking
            var normalized = _hashingHelper.Normalize(createBankingDetailsDto.AccountNumber);
            var searchHash = _hashingHelper.ComputeSearchHash(normalized);

            // Check for duplicates across all records, excluding the current employees record (if updating)
            var duplicate = await _bankingDetailRepo.AnyAsync(x =>
                x.AccountNumberSearchHash == searchHash);

            if (duplicate)
                throw new ValidationException("Account number already exists for another employee");

            BankDetailsValidations.ValidateBankingDetails(
                createBankingDetailsDto.BankName.ToString(),
                normalized);
        }
        /// <summary>
        /// Locks all banking details in the database.
        /// Only banking details that are not already locked will be updated and locked.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LockAllBankingDetailsAsync()
        {
            var all = await _bankingDetailRepo.GetAllBankingDetailsAsync();

            foreach (var detail in all)
            {
                if (!detail.IsLocked)
                {
                    detail.IsLocked = true;
                    detail.LockedAt = DateTime.Now;

                    await _bankingDetailRepo.UpdateBankingDetailsAsync(detail);
                }
            }
        }

        /// <summary>
        /// Validates the required banking detail fields.
        /// </summary>
        /// <param name="dto">The banking details model to be validated.</param>
        /// <exception cref="ValidationException">Thrown when the request is null or a required field is missing.</exception>
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

        /// <summary>
        /// Maps a BankingDetail entity to a BankingDetailDto object.
        /// Decrypts the account number before returning the DTO.
        /// </summary>
        /// <param name="d">The BankingDetail entity to be mapped.</param>
        /// <returns>
        /// A BankingDetailDto object containing the banking details.
        /// </returns>
        private BankingDetailDto MapToBankingDetailDto(BankingDetail d)
        {
            return new BankingDetailDto
            {

                BankingDetailsId = d.BankingDetailsId,
                EmployeeId = d.EmployeeId,
                Name = d.Name,
                Surname = d.Surname,
                IdNumber = d.IdNumber,
                PassportNumber = d.PassportNumber,
                BankName = d.BankName,
                AccountType = d.AccountType,
                AccountNumber = _encryptionService.Decrypt(d.AccountNumberEncrypted),
                BranchCode = d.BankBranchCode?.UniversalCode ?? string.Empty,
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