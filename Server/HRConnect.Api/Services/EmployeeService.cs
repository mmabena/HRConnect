namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using System.Globalization;
  using HRConnect.Api.Mappers;
  using System.Data.Common;
  using System.Runtime.InteropServices;

  //Inline Custom Exceptions for better error handling and clarity
  public class ValidationException : Exception
  {
    public ValidationException(string message) : base(message) { }
  }

  public class BusinessRuleException : Exception
  {
    public BusinessRuleException(string message) : base(message) { }
  }

  public class NotFoundException : Exception
  {
    public NotFoundException(string message) : base(message) { }
  }
  /// <summary>
  /// Handles business logic related to Employee operations.
  /// This layer is the bridge between the Controller and Repository.
  /// Responsible for validation, duplicate checks, ID generation and email notifications.
  /// </summary>
  public class EmployeeService : IEmployeeService
  {
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IPositionRepository _positionRepo;
    private readonly IEmailService _emailService;
    public EmployeeService(IEmployeeRepository employeeRepo, IEmailService emailService, IPositionRepository positionRepo)
    {
      _employeeRepo = employeeRepo;
      _emailService = emailService;
      _positionRepo = positionRepo;
    }
    /// <summary>
    /// Retrieves all employees from the repository.
    /// </summary>
    /// <returns>A list of all employees.</returns>
    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
      var employees = await _employeeRepo.GetAllEmployeesAsync();
      return employees.Select(e => e.ToEmployeeDto()).ToList();
    }
    /// <summary>
    /// Retrieves a single employee by their Employee ID.
    /// </summary>
    /// <param name="EmployeeId">The employee identifier.</param>
    /// <returns>The employee if found; otherwise null.</returns>
    public async Task<EmployeeDto?> GetEmployeeByIdAsync(string employeeId)
    {
      var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
      return employee?.ToEmployeeDto();
    }
    /// <summary>
    /// Creates a new employee after validating input, checking duplicates,
    /// generating a unique Employee ID, auto generate DOB and Gender if ID is provided and sending a welcome email.
    /// </summary>
    /// <param name="employeeRequestDto">Employee creation request data.</param>
    /// <returns>The newly created employee entity and the Welcome email sent.</returns>
    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequestDto employeeRequestDto)
    {
      // Validate Common Fields
      ValidateCommonFields(employeeRequestDto);
      // Validate Create function
      ValidateCreate(employeeRequestDto);
      // Ensure no duplicates exist
      await CheckDuplicates(employeeRequestDto);

      await ValidateCareerManagerAsync(null, employeeRequestDto.CareerManagerID);
      // If ID number exists, auto-extract DOB and Gender
      ExtractIdInfo(employeeRequestDto);
      // Ensure Title and Gender combination is valid
      ValidateTitleAndGender(employeeRequestDto);
      employeeRequestDto.EmployeeId = await GenerateUniqueEmpId(employeeRequestDto.Surname);

      var position = await _positionRepo.GetPositionByIdAsync(employeeRequestDto.PositionId);
      if (position == null)
        throw new ValidationException($"Position with ID {employeeRequestDto.PositionId} does not exist.");

      var new_employee = employeeRequestDto.ToEmployeeFromCreateDTO();

      new_employee.PositionId = position.PositionId;
      new_employee.Position = position;

      using var transaction = await _employeeRepo.BeginTransactionAsync();
      try
      {
        var createdEmployee = await _employeeRepo.CreateEmployeeAsync(new_employee);
        // Send welcome email notification
        await SendWelcomeEmail(createdEmployee);
        await transaction.CommitAsync();
        return createdEmployee.ToEmployeeDto();
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        // You can also this to a retry queue or background job
        Console.WriteLine($"Failed to create employee or send email: {ex.Message}");
        throw;
      }
    }
    /// <summary>
    /// Updates an existing employee after validation and duplicate checks.
    /// </summary>
    /// <param name="EmployeeId">The employee identifier.</param>
    /// <param name="employeeDto">Updated employee data.</param>
    /// <returns>The updated employee or null if not found.</returns>
    public async Task<EmployeeDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeRequestDto employeeDto)
    {
      var existingEmployee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
      if (existingEmployee == null)
        throw new NotFoundException("Employee not found");

      // Validate input fields
      ValidateCommonFields(employeeDto);
      ValidateUpdate(employeeDto);
      //Check for duplicate entries
      await CheckDuplicateOnUpdate(employeeId, employeeDto);

      var position = await _positionRepo.GetPositionByIdAsync(employeeDto.PositionId);
      if (position == null)
        throw new ValidationException($"Position with ID {employeeDto.PositionId} does not exist.");

      existingEmployee.PositionId = position.PositionId;
      existingEmployee.Position = position;

      await ValidateCareerManagerAsync(employeeId, employeeDto.CareerManagerID);

      existingEmployee.Title = employeeDto.Title;
      existingEmployee.Name = employeeDto.Name;
      existingEmployee.Surname = employeeDto.Surname;
      existingEmployee.ContactNumber = employeeDto.ContactNumber;
      existingEmployee.Email = employeeDto.Email;
      existingEmployee.City = employeeDto.City;
      existingEmployee.ZipCode = employeeDto.ZipCode;
      existingEmployee.Branch = employeeDto.Branch;
      existingEmployee.PositionId = employeeDto.PositionId;
      existingEmployee.MonthlySalary = employeeDto.MonthlySalary;
      existingEmployee.CareerManagerID = employeeDto.CareerManagerID;
      existingEmployee.ProfileImage = employeeDto.ProfileImage;
      existingEmployee.HasDisability = employeeDto.HasDisability;
      existingEmployee.DisabilityDescription = employeeDto.DisabilityDescription;
      existingEmployee.UpdatedAt = DateTime.UtcNow;

      var updatedEmployee = await _employeeRepo.UpdateEmployeeAsync(existingEmployee);
      return updatedEmployee?.ToEmployeeDto();
    }
    /// <summary>
    /// Deletes an employee if they exist and were started within the current month.
    /// </summary>
    /// <param name="EmployeeId">The employee identifier.</param>
    /// <returns>True if deletion successful.</returns>
    public async Task<bool> DeleteEmployeeAsync(string employeeId)
    {
      var existingEmployee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

      if (existingEmployee == null)
        throw new NotFoundException("Employee not found");

      var now = DateTime.UtcNow;
      // Business rule: Only allow deletion within the same start month
      if (existingEmployee.StartDate.Year != now.Year || existingEmployee.StartDate.Month != now.Month)
      {
        throw new ArgumentException("Employee can only be deleted in the same month they started.");
      }
      return await _employeeRepo.DeleteEmployeeAsync(employeeId);
    }
    /// <summary>
    /// Generates a unique Employee ID based on surname prefix and existing IDs.
    /// Example: SMI001 for surname Smith.
    /// </summary>
    /// <param name="lastName">The employee last name</param>
    /// <returns>New unique employee ID</returns>
    private async Task<string> GenerateUniqueEmpId(string lastName)
    {   // Create 3-letter prefix, by exstracting the first 3 letters of the last name and converting to uppercase. 
        // If last name is less than 3 letters, pad with 'X'.
      string prefix = lastName.Length >= 3
          ? lastName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture)
          : lastName.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X');
      int nextNum = 1;
      // Fetch existing IDs with same prefix to determine the next number to use.
      var existingIds = await _employeeRepo.GetAllEmployeeIdsWithPrefix(prefix);
      if (existingIds.Count > 0)
      {
        // Extract numeric portion and increment to get the next number
        var maxNum = existingIds
                .Select(id => int.Parse(id.AsSpan(3), CultureInfo.InvariantCulture))
                .Max();

        nextNum = maxNum + 1;
      }

      return $"{prefix}{nextNum:D3}";
    }
    /// <summary>
    /// Handles business logic related to Employee operations.
    /// This layer is the bridge between the Controller and Repository.
    /// Responsible for validation, duplicate checks, ID generation and email notifications.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IPositionRepository _positionRepo;
        private readonly IEmailService _emailService;
        public EmployeeService(IEmployeeRepository employeeRepo, IEmailService emailService, IPositionRepository positionRepo)
        {
            _employeeRepo = employeeRepo;
            _emailService = emailService;
            _positionRepo = positionRepo;
        }
        /// <summary>
        /// Retrieves all employees from the repository.
        /// </summary>
        /// <returns>A list of all employees.</returns>
        public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepo.GetAllEmployeesAsync();
            return employees.Select(e => e.ToEmployeeDto()).ToList();
        }
        /// <summary>
        /// Retrieves a single employee by their Employee ID.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier.</param>
        /// <returns>The employee if found; otherwise null.</returns>
        public async Task<EmployeeDto?> GetEmployeeByIdAsync(string employeeId)
        {
            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
            return employee?.ToEmployeeDto();
        }
        /// <summary>
        /// Creates a new employee after validating input, checking duplicates,
        /// generating a unique Employee ID, auto generate DOB and Gender if ID is provided and sending a welcome email.
        /// </summary>
        /// <param name="employeeRequestDto">Employee creation request data.</param>
        /// <returns>The newly created employee entity and the Welcome email sent.</returns>
        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequestDto employeeRequestDto)
        {
            // Validate Common Fields
            ValidateCommonFields(employeeRequestDto);
            // Validate Create function
            ValidateCreate(employeeRequestDto);
            // Ensure no duplicates exist
            await CheckDuplicates(employeeRequestDto);

            await ValidateCareerManagerAsync(null, employeeRequestDto.CareerManagerID);
            // If ID number exists, auto-extract DOB and Gender
            ExtractIdInfo(employeeRequestDto);
            // Ensure Title and Gender combination is valid
            ValidateTitleAndGender(employeeRequestDto);
            employeeRequestDto.EmployeeId = await GenerateUniqueEmpId(employeeRequestDto.Surname);

            var position = await _positionRepo.GetPositionByIdAsync(employeeRequestDto.PositionId);
            if (position == null)
                throw new ValidationException($"Position with ID {employeeRequestDto.PositionId} does not exist.");

            var new_employee = employeeRequestDto.ToEmployeeFromCreateDTO();

            new_employee.PositionId = position.PositionId;
            new_employee.Position = position;

            using var transaction = await _employeeRepo.BeginTransactionAsync();
            try
            {
                var createdEmployee = await _employeeRepo.CreateEmployeeAsync(new_employee);
                // Send welcome email notification
                await SendWelcomeEmail(createdEmployee);
                await transaction.CommitAsync();
                return createdEmployee.ToEmployeeDto();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // You can also this to a retry queue or background job
                Console.WriteLine($"Failed to create employee or send email: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Updates an existing employee after validation and duplicate checks.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier.</param>
        /// <param name="employeeDto">Updated employee data.</param>
        /// <returns>The updated employee or null if not found.</returns>
        public async Task<EmployeeDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeRequestDto employeeDto)
        {
            var existingEmployee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
            if (existingEmployee == null)
                throw new NotFoundException("Employee not found");

            // Validate input fields
            ValidateCommonFields(employeeDto);
            ValidateUpdate(employeeDto);
            //Check for duplicate entries
            await CheckDuplicateOnUpdate(employeeId, employeeDto);

            var position = await _positionRepo.GetPositionByIdAsync(employeeDto.PositionId);
            if (position == null)
                throw new ValidationException($"Position with ID {employeeDto.PositionId} does not exist.");

            existingEmployee.PositionId = position.PositionId;
            existingEmployee.Position = position;

            await ValidateCareerManagerAsync(employeeId, employeeDto.CareerManagerID);

            existingEmployee.Title = employeeDto.Title;
            existingEmployee.Name = employeeDto.Name;
            existingEmployee.Surname = employeeDto.Surname;
            existingEmployee.ContactNumber = employeeDto.ContactNumber;
            existingEmployee.Email = employeeDto.Email;
            existingEmployee.City = employeeDto.City;
            existingEmployee.ZipCode = employeeDto.ZipCode;
            existingEmployee.Branch = employeeDto.Branch;
            existingEmployee.PositionId = employeeDto.PositionId;
            existingEmployee.MonthlySalary = employeeDto.MonthlySalary;
            existingEmployee.CareerManagerID = employeeDto.CareerManagerID;
            existingEmployee.UpdatedAt = DateTime.UtcNow;

            var updatedEmployee = await _employeeRepo.UpdateEmployeeAsync(existingEmployee);
            return updatedEmployee?.ToEmployeeDto();
        }
        /// <summary>
        /// Deletes an employee if they exist and were started within the current month.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier.</param>
        /// <returns>True if deletion successful.</returns>
        public async Task<bool> DeleteEmployeeAsync(string employeeId)
        {
            var existingEmployee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

            if (existingEmployee == null)
                throw new NotFoundException("Employee not found");

            var now = DateTime.UtcNow;
            // Business rule: Only allow deletion within the same start month
            if (existingEmployee.StartDate.Year != now.Year || existingEmployee.StartDate.Month != now.Month)
            {
                throw new ArgumentException("Employee can only be deleted in the same month they started.");
            }
            return await _employeeRepo.DeleteEmployeeAsync(employeeId);
        }
        /// <summary>
        /// Generates a unique Employee ID based on surname prefix and existing IDs.
        /// Example: SMI001 for surname Smith.
        /// </summary>
        /// <param name="lastName">The employee last name</param>
        /// <returns>New unique employee ID</returns>
        private async Task<string> GenerateUniqueEmpId(string lastName)
        {   // Create 3-letter prefix, by exstracting the first 3 letters of the last name and converting to uppercase. 
            // If last name is less than 3 letters, pad with 'X'.
            string prefix = lastName.Length >= 3
                ? lastName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture)
                : lastName.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X');
            int nextNum = 1;
            // Fetch existing IDs with same prefix to determine the next number to use.
            var existingIds = await _employeeRepo.GetAllEmployeeIdsWithPrefix(prefix);
            if (existingIds.Count > 0)
            {
                // Extract numeric portion and increment to get the next number
                var maxNum = existingIds
                        .Select(id => int.Parse(id.AsSpan(3), CultureInfo.InvariantCulture))
                        .Max();

                nextNum = maxNum + 1;
            }

            return $"{prefix}{nextNum:D3}";
        }
        /// <summary>
        /// Sends a welcome email to a newly created employee's email address.
        /// </summary>
        /// <param name="employee">The employee object</param>
        /// <returns>True; If email was sent successfully</returns>
        private async Task SendWelcomeEmail(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Email))
                return;

            var subject = "Welcome to HRConnect";

            var body = $@"
                Hello {employee.Name} {employee.Surname},
                
                Welcome to HRConnect!
                
                Your employee ID: {employee.EmployeeId}
                Position: {employee.PositionId}
                Branch: {employee.Branch}
                
                We are glad to have you onboard. :-)";

      await _emailService.SendEmailAsync(employee.Email, subject, body);
    }
    /// <summary>
    /// Extracts Date of Birth and Gender from the provided ID Number.
    /// </summary>
    /// <param name="employeeRequestDto">The employee creation request DTO</param>
    /// <returns>Validation error if ID information is invalid</returns>
    private static void ExtractIdInfo(CreateEmployeeRequestDto employeeRequestDto)
    {
      if (string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber)) return;

      var employeeInfo = IdNumberValidator.ParseIdNumber(employeeRequestDto.IdNumber);

      EnsureEmployeeMeetsAgePolicy(employeeInfo.DateOfBirth, employeeRequestDto.EmploymentStatus);

      employeeRequestDto.Gender = employeeInfo.Gender;
      employeeRequestDto.DateOfBirth = employeeInfo.DateOfBirth;

      if (employeeInfo.IsSouthAfricanCitizen)
      {
        employeeRequestDto.Nationality = "South African";
      }
    }
    /// <summary>
    /// Ensures the employee meets the minimum age requirement based on employment status.
    /// </summary>
    /// <param name="dateOfBirth">The employee date of birth</param>
    /// <param name="employmentStatus">The employee employment status</param>
    /// <returns>Validation error if age requirement is not met</returns>

    private static void EnsureEmployeeMeetsAgePolicy(DateOnly dateOfBirth, EmploymentStatus employmentStatus)
    {
      EmployeeValidationHelpers.ValidateDateOfBirth(dateOfBirth, employmentStatus);
    }
    /// <summary>
    /// Validates that the employee title matches the provided gender.
    /// </summary>
    /// <param name="employeeRequestDto">The employee creation request DTO</param>
    /// <returns>Validation error if title and gender are not logically valid</returns>
    private static void ValidateTitleAndGender(CreateEmployeeRequestDto employeeRequestDto)
    {
      EmployeeValidationHelpers.ValidateTitleGenderCombo(employeeRequestDto.Title, employeeRequestDto.Gender);
    }
    /// <summary>
    /// Checks for duplicate employee records during creation.
    /// </summary>
    /// <param name="employeeRequestDto">The employee creation request DTO</param>
    /// <returns>Error message if duplicate record is found</returns>
    private async Task CheckDuplicates(CreateEmployeeRequestDto employeeRequestDto)
    {
      await EmployeeValidationHelpers.ValidateNoDuplicatesOnCreateAsync(_employeeRepo, employeeRequestDto);
    }
    /// <summary>
    /// Checks for duplicate employee records during update.
    /// </summary>
    /// <param name="employeeId">The employee identifier</param>
    /// <param name="employeeRequestDto">The employee update request DTO</param>
    /// <returns>Error message if duplicate record is found</returns>
    private async Task CheckDuplicateOnUpdate(string employeeId, UpdateEmployeeRequestDto employeeRequestDto)
    {
      await EmployeeValidationHelpers.ValidateNoDuplicatesOnUpdateAsync(_employeeRepo, employeeId, employeeRequestDto);
    }
    /// <summary>
    /// Validates common employee input fields for both create and update operations.
    /// </summary>
    /// <param name="employeeRequestDto">The employee base request DTO</param>
    /// <returns>Validation error if input fields are invalid</returns>
    private static void ValidateCommonFields(EmployeeBaseRequestDto employeeRequestDto)
    {
      EmployeeValidationHelpers.ValidateRequiredString(employeeRequestDto.Name, "Employee name", 50);
      EmployeeValidationHelpers.ValidateRequiredString(employeeRequestDto.Surname, "Employee surname", 100);
      EmployeeValidationHelpers.ValidateEmail(employeeRequestDto.Email);
      EmployeeValidationHelpers.ValidateNumericString(employeeRequestDto.ContactNumber, "Contact number", 10);
      EmployeeValidationHelpers.ValidateEnum(employeeRequestDto.Title, "Title");
      EmployeeValidationHelpers.ValidateEnum(employeeRequestDto.Branch, "Branch");
      EmployeeValidationHelpers.ValidateEnum(employeeRequestDto.EmploymentStatus, "Employment status");
      EmployeeValidationHelpers.ValidateSalary(employeeRequestDto.MonthlySalary);
      EmployeeValidationHelpers.ValidateImageFile(employeeRequestDto.ProfileImage);
      EmployeeValidationHelpers.ValidateDisabilityFields(employeeRequestDto.HasDisability, employeeRequestDto.DisabilityDescription);
      EmployeeValidationHelpers.ValidateNationality(employeeRequestDto.IdNumber, employeeRequestDto.PassportNumber, employeeRequestDto.Nationality);
      EmployeeValidationHelpers.ValidateCityZip(employeeRequestDto.City, employeeRequestDto.ZipCode);
      if (employeeRequestDto.PositionId <= 0)
        throw new ValidationException("Position ID must be greater than 0");
    }
    /// <summary>
    /// Performs additional validation rules specific to employee creation.
    /// </summary>
    /// <param name="employeeRequestDto">The employee creation request DTO</param>
    /// <returns>Validation error if create rules are not satisfied</returns>
    private static void ValidateCreate(CreateEmployeeRequestDto employeeRequestDto)
    {
      EmployeeValidationHelpers.ValidateStartDate(employeeRequestDto.StartDate);
      EmployeeValidationHelpers.ValidateTaxNumber(employeeRequestDto.TaxNumber);
      if (!string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber) && string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber))
      {
        EnsureEmployeeMeetsAgePolicy(employeeRequestDto.DateOfBirth, employeeRequestDto.EmploymentStatus);
        EmployeeValidationHelpers.ValidateGender(employeeRequestDto.Gender);
      }
    }
    // <summary>
    /// Performs additional validation rules specific to employee updates.
    /// </summary>
    /// <param name="employeeRequestDto">The employee update request DTO</param>
    /// <returns>Validation error if update rules are not satisfied</returns>
    private static void ValidateUpdate(UpdateEmployeeRequestDto employeeRequestDto)
    {
      EmployeeValidationHelpers.ValidateSalary(employeeRequestDto.MonthlySalary);
    }
    private async Task ValidateCareerManagerAsync(string employeeId, string? careerManagerId)
    {
      await EmployeeValidationHelpers.ValidateCareerManagerAsync(_employeeRepo, employeeId, careerManagerId);
    }

     public async Task<EmployeeDto?> GetEmployeeByEmailAsync(string employeeEmail)
    {
      Employee? employee = await _employeeRepo.GetEmployeeByEmailAsync(employeeEmail);
      return employee?.ToEmployeeDto();
    }
}