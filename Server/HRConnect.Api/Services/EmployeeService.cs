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
        private readonly IEmailService _emailService;
        public EmployeeService(IEmployeeRepository employeeRepo, IEmailService emailService)
        {
            _employeeRepo = employeeRepo;
            _emailService = emailService;
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
            var new_employee = employeeRequestDto.ToEmployeeFromCreateDTO();

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
        }
        /// <summary>
        /// Ensures the employee meets the minimum age requirement based on employment status.
        /// </summary>
        /// <param name="dateOfBirth">The employee date of birth</param>
        /// <param name="employmentStatus">The employee employment status</param>
        /// <returns>Validation error if age requirement is not met</returns>

        private static void EnsureEmployeeMeetsAgePolicy(DateOnly dateOfBirth, EmploymentStatus employmentStatus)
        {
            if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ValidationException("Date of birth cannot be in the future");

            int age = AgeCalculator.CalculateAge(dateOfBirth);

            // Example policy: contracts can start at 16, others 18
            int minimumAge = employmentStatus == EmploymentStatus.Contract ? 16 : 18;

            if (age < minimumAge)
                throw new ValidationException($"Employee must be at least {minimumAge} years old.");
        }
        /// <summary>
        /// Validates that the employee title matches the provided gender.
        /// </summary>
        /// <param name="employeeRequestDto">The employee creation request DTO</param>
        /// <returns>Validation error if title and gender are not logically valid</returns>
        private static void ValidateTitleAndGender(CreateEmployeeRequestDto employeeRequestDto)
        {
            if (employeeRequestDto.Title == Title.Mr)
            {
                if (employeeRequestDto.Gender != Gender.Male)
                    throw new ValidationException("Title 'Mr' must have gender 'Male'");
            }
            else if (employeeRequestDto.Title == Title.Mrs || employeeRequestDto.Title == Title.Ms)
            {
                if (employeeRequestDto.Gender != Gender.Female)
                    throw new ValidationException("Title 'Mrs' or 'Ms' must have gender 'Female'");
            }
        }
        /// <summary>
        /// Checks for duplicate employee records during creation.
        /// </summary>
        /// <param name="employeeRequestDto">The employee creation request DTO</param>
        /// <returns>Error message if duplicate record is found</returns>
        private async Task CheckDuplicates(CreateEmployeeRequestDto employeeRequestDto)
        {
            var existing = await _employeeRepo.GetEmployeeByEmailAsync(employeeRequestDto.Email);
            if (existing != null)
                throw new BusinessRuleException("Email is already in use");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.TaxNumber) && await _employeeRepo.GetEmployeeByTaxNumberAsync(employeeRequestDto.TaxNumber) != null)
                throw new BusinessRuleException("An employee with the same tax number already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) && await _employeeRepo.GetEmployeeByIdNumberAsync(employeeRequestDto.IdNumber) != null)
                throw new BusinessRuleException("An employee with the same ID number already exists");

            if (await _employeeRepo.GetEmployeeByContactNumberAsync(employeeRequestDto.ContactNumber) != null)
                throw new BusinessRuleException("An employee with the same contact number already exists");
        }
        /// <summary>
        /// Checks for duplicate employee records during update.
        /// </summary>
        /// <param name="employeeId">The employee identifier</param>
        /// <param name="employeeRequestDto">The employee update request DTO</param>
        /// <returns>Error message if duplicate record is found</returns>
        private async Task CheckDuplicateOnUpdate(string employeeId, UpdateEmployeeRequestDto employeeRequestDto)
        {
            if (!string.IsNullOrWhiteSpace(employeeRequestDto.Email) &&
        await _employeeRepo.GetEmployeeByEmailAsync(employeeRequestDto.Email, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same email already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) &&
                await _employeeRepo.GetEmployeeByIdNumberAsync(employeeRequestDto.IdNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same Id Number already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber) &&
                await _employeeRepo.GetEmployeeByPassportAsync(employeeRequestDto.PassportNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same passport number already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.ContactNumber) &&
                await _employeeRepo.GetEmployeeByContactNumberAsync(employeeRequestDto.ContactNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same contact number already exists");
        }
        /// <summary>
        /// Validates common employee input fields for both create and update operations.
        /// </summary>
        /// <param name="employeeRequestDto">The employee base request DTO</param>
        /// <returns>Validation error if input fields are invalid</returns>
        private static void ValidateCommonFields(EmployeeBaseRequestDto employeeRequestDto)
        {
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            bool isIdNumberProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber);
            bool isPassportProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber);
            if (!isIdNumberProvided && !isPassportProvided)
                throw new ValidationException("Either National ID or Passport is required");

            if (isIdNumberProvided && isPassportProvided)
                throw new ValidationException("You cannot enter both ID Number and Passport Number");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber))
            {
                if (employeeRequestDto.IdNumber.Length != 13)
                    throw new ValidationException("ID Number must be 13 digits long and must contain digits only");

                if (!employeeRequestDto.IdNumber.All(char.IsDigit))
                    throw new ValidationException("ID Number must contain digits only");
            }

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Name))
                throw new ValidationException("Employee name is required");

            if (employeeRequestDto.Name.Length > 50)
                throw new ValidationException("Employee name must not exceed 50 characters");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Surname))
                throw new ValidationException("Employee surname is required");

            if (employeeRequestDto.Surname.Length > 100)
                throw new ValidationException("Employee surname must not exceed 100 characters");

            if (employeeRequestDto.HasDisability && string.IsNullOrWhiteSpace(employeeRequestDto.DisabilityDescription))
                throw new ValidationException("Disability description is required if HasDisability is true.");

            if (!employeeRequestDto.HasDisability && !string.IsNullOrWhiteSpace(employeeRequestDto.DisabilityDescription))
                throw new ValidationException("Disability description must be empty if HasDisability is false.");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.ContactNumber))
                throw new ValidationException("Employee contact number is required");

            if (!Enum.IsDefined<Title>(employeeRequestDto.Title))
                throw new ValidationException("Employee Title is invalid");

            if (employeeRequestDto.ContactNumber.Length != 10)
                throw new ValidationException("Contact number must be 10 digits long");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Email) || !employeeRequestDto.Email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Email must end with '@singular.co.za'");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.PhysicalAddress))
                throw new ValidationException("Employee physical address is required");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.City))
                throw new ValidationException("Employee City is required");

            CityZipValidator.ValidateCityAndZip(
                employeeRequestDto.City,
                employeeRequestDto.ZipCode
            );

            if (!Enum.IsDefined<Branch>(employeeRequestDto.Branch))
                throw new ValidationException("Branch must either be 'Johannesburg', 'Cape Town' or 'UK'");

            if (!Enum.IsDefined<EmploymentStatus>(employeeRequestDto.EmploymentStatus))
                throw new ValidationException("Employment status must either be 'Permanent', 'Fixed-Term' or 'Contract'");

            if (employeeRequestDto.MonthlySalary <= 0)
                throw new ValidationException("Monthly salary must be greater than 0");

            if (employeeRequestDto.MonthlySalary >= 100000)
                throw new ValidationException("Monthly salary must not exceed 100 000");

            if (employeeRequestDto.PositionId <= 0)
                throw new ValidationException("Position ID must be greater than 0");


            var extension = Path.GetExtension(employeeRequestDto.ProfileImage);

            if (string.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new ValidationException("Employee picture must be a valid image file (.png, .jpg, .jpeg, .gif, .bmp, .webp)");
            }
        }
        /// <summary>
        /// Performs additional validation rules specific to employee creation.
        /// </summary>
        /// <param name="employeeRequestDto">The employee creation request DTO</param>
        /// <returns>Validation error if create rules are not satisfied</returns>
        private static void ValidateCreate(CreateEmployeeRequestDto employeeRequestDto)
        {
            var now = DateTime.UtcNow;
            bool isIdNumberProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber);
            bool isPassportProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber);

            if (employeeRequestDto.StartDate == default)
                throw new ValidationException("Start date is required");

            if (employeeRequestDto.StartDate.Month != now.Month || employeeRequestDto.StartDate.Year != now.Year)
            {
                throw new ValidationException(
                    "Start date must be within the current month."
                );
            }

            if (isPassportProvided && !isIdNumberProvided)
            {
                if (employeeRequestDto.DateOfBirth == default)
                    throw new ValidationException("Date of Birth is required if ID Number is not provided");

                int age = AgeCalculator.CalculateAge(employeeRequestDto.DateOfBirth);
                if (age < 18)
                    throw new ValidationException("Employee must be at least 18 years old.");

                if (!employeeRequestDto.Gender.HasValue)
                    throw new ValidationException("Gender is required when using Passport");

                if (!Enum.IsDefined(employeeRequestDto.Gender.Value))
                    throw new ValidationException("Gender must be either Male or Female");

            }

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.TaxNumber))
            {
                if (employeeRequestDto.TaxNumber.Length != 10)
                    throw new ValidationException("Tax Number must be 10 digits long and must contain digits only");

                if (!employeeRequestDto.TaxNumber.All(char.IsDigit))
                    throw new ValidationException("Tax Number must contain digits only");
            }

        }
        // <summary>
        /// Performs additional validation rules specific to employee updates.
        /// </summary>
        /// <param name="employeeRequestDto">The employee update request DTO</param>
        /// <returns>Validation error if update rules are not satisfied</returns>
        private static void ValidateUpdate(UpdateEmployeeRequestDto employeeRequestDto)
        {
            if (employeeRequestDto.MonthlySalary <= 0)
                throw new ValidationException("Monthly salary must be greater than 0");

            if (employeeRequestDto.MonthlySalary >= 100000)
                throw new ValidationException("Monthly salary must not exceed 100 000");
        }

        /// <summary>
        /// Checks for duplicate records when updating an employee.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <param name="employeeRequestDto">The employee update request DTO</param>
        /// <returns>Error message if duplicate is found</returns>
        private async Task CheckForDuplicatesonUpdate(string employeeId, UpdateEmployeeRequestDto employeeRequestDto)
        {
            var existingEmployee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

            if (existingEmployee == null)
                throw new NotFoundException("Employee not found");

            if (await _employeeRepo.GetEmployeeByEmailAsync(employeeRequestDto.Email, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same email already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) && await _employeeRepo.GetEmployeeByIdNumberAsync(employeeRequestDto.IdNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same ID number already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber) && await _employeeRepo.GetEmployeeByPassportAsync(employeeRequestDto.PassportNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same passport number already exists");

            if (await _employeeRepo.GetEmployeeByContactNumberAsync(employeeRequestDto.ContactNumber, employeeId) != null)
                throw new BusinessRuleException("Another employee with the same contact number already exists");
        }
        private async Task ValidateCareerManagerAsync(string employeeId, string? CareerMangerId)
        {
            if (string.IsNullOrWhiteSpace(CareerMangerId))
                return;

            if (CareerMangerId == employeeId)
                throw new BusinessRuleException("Employee cannot be their own Career Manager");

            var manager = await _employeeRepo.GetEmployeeByIdAsync(CareerMangerId);

            if (manager == null)
                throw new BusinessRuleException("Career Manager must be an existing Employee");
        }
    }
}