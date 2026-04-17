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
        /// <summary>
        /// Service responsible for handling all business logic related to employee banking details, including creation, retrieval, updating, and locking of banking details.
        /// </summary>
        private readonly IBankingDetailRepository _bankingDetailRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILogger<BankingDetailService> _logger;


        /// <summary>
        /// Constructor for BankingDetailService which injects the necessary repositories for banking details and employee data access.
        /// </summary>
        /// <param name="bankingDetailRepo">The repository for accessing banking detail data.</param>
        /// <param name="employeeRepo">The repository for accessing employee data.</param>
        public BankingDetailService(
            IBankingDetailRepository bankingDetailRepo,
            IEmployeeRepository employeeRepo)
        {
            _bankingDetailRepo = bankingDetailRepo;
            _employeeRepo = employeeRepo;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all banking details from the database and maps them to a list of BankingDetailDto objects. This method allows clients to get a complete list of all banking details stored in the system, which can be useful for administrative purposes or for displaying in a user interface. Each BankingDetail entity is transformed into a BankingDetailDto to ensure that only relevant information is exposed to the client, adhering to data encapsulation principles.
        /// </summary>
        /// <returns> A list of all banking details mapped to DTOs.</returns>
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
        /// Retrieves the banking details of a specific employee by their employee ID. This method first validates the input employee ID to ensure it is not null or empty. It then queries the banking detail repository for a record that matches the provided employee ID. If a matching record is found, it is mapped to a BankingDetailDto and returned; if no record is found, a KeyNotFoundException is thrown. This allows clients to retrieve banking details for individual employees while ensuring that proper error handling is in place for cases where the employee does not have associated banking details.
        /// </summary>
        /// <param name="EmployeeId">The ID of the employee for whom to retrieve banking details.</param>
        /// <returns>The banking detail record mapped to a DTO if found; otherwise, throws an exception.</returns>
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
                _logger.LogError(ex, "Error retrieving banking details for {EmployeeId}", EmployeeId);
                throw;
            }
        }

        /// <summary>
        /// Creates new banking details for an employee or updates existing details if they already exist.
        ///  The method first validates the input DTO to ensure all required fields are present and that the employee ID is provided.
        ///  It then checks if the employee exists and if their employment status allows for banking details to be associated. If the employee is valid, it validates the banking details according to predefined rules. The method then checks if banking details already exist for the employee; if they do, it updates the existing record, otherwise, it creates a new record. Finally, it returns the created or updated banking details as a DTO. This method ensures that banking details are correctly associated with employees and that all necessary validations are performed before any database operations.
        /// </summary>
        /// <param name="createBankingDetailsDto"> The DTO containing the banking details to create or update.</param>
        /// <returns>The created or updated banking detail record as a DTO.</returns>
        /// <exception cref="ValidationException">Thrown when the input DTO is invalid.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the employee is not found.</exception>
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

        /// <summary>
        /// Updates the banking details of an existing employee. 
        /// The method first checks if the employee exists using the provided employee ID. 
        /// If the employee does not exist, a KeyNotFoundException is thrown. 
        /// Next, it checks if banking details already exist for the employee; 
        /// if not, it throws another KeyNotFoundException indicating that banking details must be created first. 
        /// The method then validates the input DTO to ensure all required fields are present and that the bank name is valid. 
        /// If all validations pass, it updates the existing banking details record with the new information from the DTO and saves the changes to the database. 
        /// Finally, it returns the updated banking details as a DTO. 
        /// This method ensures that only valid updates are made to existing banking details and that proper error handling is in place for cases where the employee or their banking details do not exist.
        /// </summary>
        /// <param name="EmployeeId">The ID of the employee whose banking details are to be updated.</param>
        /// <param name="updatebankingDetailsDto">The DTO containing the updated banking details.</param>
        /// <returns>The updated banking detail record as a DTO.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the employee or their banking details are not found.</exception>
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

            if (existing.IsLocked)
                throw new ValidationException("Banking details are locked and cannot be modified");

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

        /// <summary>
        /// Locks all banking details in the system by setting the IsLocked property to true and updating the LockedAt timestamp. 
        /// This method retrieves all banking details from the repository, iterates through each record, and if it is not already locked, it updates the IsLocked flag and sets the LockedAt time to the current UTC time. 
        /// Finally, it saves all the updated records back to the repository. 
        /// This functionality can be used to prevent any further modifications to banking details after a certain point in time, such as after a payroll run has been processed.
        /// </summary>
        /// <returns> A task representing the asynchronous operation.</returns>
        public async Task LockAllBankingDetailsAsync()
        {
            var allBankingDetails = await _bankingDetailRepo.GetAllBankingDetailsAsync();

            foreach (var detail in allBankingDetails)
            {
                if (!detail.IsLocked)
                {
                    detail.IsLocked = true;
                    detail.LockedAt = DateTime.UtcNow;
                }
            }

            await _bankingDetailRepo.UpdateRangeAsync(allBankingDetails);
        }

        /// <summary>
        /// Validates the common fields of the CreateBankingDetailDto to ensure that all required information is present and valid before proceeding with any business logic.
        ///  This method checks if the DTO is null, and if so, throws a ValidationException. 
        /// It also checks if the Name, Surname, AccountNumber, and BranchCode fields are null or whitespace, and if any of these validations fail, 
        /// it throws a ValidationException with an appropriate message. 
        /// This validation step is crucial to prevent invalid data from being processed and to provide clear feedback on what is missing or incorrect in the input.
        /// </summary>
        /// <param name="dto"> The DTO to validate.</param>
        /// <exception cref="ValidationException">Thrown when the DTO is invalid.</exception>
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

        /// <summary>
        /// Validates the fields of the UpdateBankingDetailDto to ensure that all required information is present and valid before proceeding with the update operation.
        ///  This method checks if the DTO is null, and if so, throws a ValidationException.
        ///  It also checks if the BankName is a valid enum value, and if the AccountNumber and BranchCode fields are null or whitespace.
        ///  If any of these validations fail, it throws a ValidationException with an appropriate message.
        ///  This validation step is essential to ensure that only valid data is used to update existing banking details and to provide clear feedback on what is missing or incorrect in the input.
        /// </summary>
        /// <param name="dto"> The DTO to validate.</param>
        /// <exception cref="ValidationException">Thrown when the DTO is invalid.</exception>
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

        /// <summary>
        /// Maps a BankingDetail entity to a BankingDetailDto.
        ///  This method takes a BankingDetail object as input and creates a new BankingDetailDto with the corresponding properties mapped from the entity.
        ///  This mapping is necessary to ensure that only relevant information is exposed to the client and to adhere to data encapsulation principles.
        ///  The method extracts properties such as BankingDetailsId, Name, Surname, IdNumber, PassportNumber, BankName, AccountType, AccountNumber, 
        /// BranchCode, NetSalary, IsActive, CreatedAt, and UpdatedAt from the BankingDetail entity and assigns them to the corresponding fields in the BankingDetailDto.
        ///  This allows the service to return a clean and structured DTO to the client while keeping the internal entity structure hidden.
        /// </summary>
        /// <param name="d"> The BankingDetail entity to map.</param>
        /// <returns>The mapped BankingDetailDto.</returns>
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