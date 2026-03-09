namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;

  /// <summary>
  /// Defines the contract for managing medical options and their categories within the HR Connect system.
  /// Provides comprehensive functionality for retrieving, validating, and updating medical option data,
  /// including support for salary-based contribution calculations and bulk operations.
  /// </summary>
  /// <remarks>
  /// This service interface is designed to handle medical insurance options that are categorized and include 
  /// various contribution amounts based on salary brackets and member types (principal, adult, child, child2). 
  /// The interface supports both individual and bulk operations, making it suitable for administrative 
  /// management of medical benefit options.
  /// 
  /// Key features include:
  /// - Hierarchical organization of medical options within categories
  /// - Salary bracket-based contribution calculations
  /// - Support for multiple member types with different contribution rates
  /// - Bulk update capabilities for efficient administration
  /// - Comprehensive validation and existence checking methods
  /// 
  /// The interface is typically used by:
  /// - API controllers for exposing medical option data to clients
  /// - Administrative interfaces for managing medical benefit configurations
  /// - Business logic components that need to validate or process medical option data
  /// - Background services that might perform bulk updates or validations
  /// </remarks>
  public interface IMedicalOptionService
  {
    /// <summary>
    /// Retrieves all medical options grouped by their categories in a hierarchical structure.
    /// </summary>
    /// <returns>A list of medical option categories with their associated medical options.</returns>
    /// <exception cref="System.Exception">Thrown when database access fails or data retrieval encounters an error.</exception>
    /// <example>
    /// <code>
    /// var groupedOptions = await _medicalOptionService.GetGroupedMedicalOptionsAsync();
    /// foreach (var category in groupedOptions)
    /// {
    ///     Console.WriteLine($"Category: {category.MedicalOptionCategoryName}");
    ///     foreach (var option in category.MedicalOptions)
    ///     {
    ///         Console.WriteLine($"  - {option.MedicalOptionName}");
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<List<MedicalOptionCategoryDto>> GetGroupedMedicalOptionsAsync();

    /// <summary>
    /// Retrieves a specific medical option by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the medical option to retrieve.</param>
    /// <returns>The medical option data transfer object if found; otherwise, null.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the provided id is invalid.</exception>
    /// <exception cref="System.Exception">Thrown when database access fails.</exception>
    /// <example>
    /// <code>
    /// var medicalOption = await _medicalOptionService.GetMedicalOptionByIdAsync(123);
    /// if (medicalOption != null)
    /// {
    ///     Console.WriteLine($"Option: {medicalOption.MedicalOptionName}");
    ///     Console.WriteLine($"Category ID: {medicalOption.MedicalOptionCategoryId}");
    /// }
    /// </code>
    /// </example>
    Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id);

    /// <summary>
    /// Retrieves a medical option category by its unique identifier, including all associated medical options.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category to retrieve.</param>
    /// <returns>The medical option category with its associated options if found; otherwise, null.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the provided categoryId is invalid.</exception>
    /// <exception cref="System.Exception">Thrown when database access fails.</exception>
    /// <example>
    /// <code>
    /// var category = await _medicalOptionService.GetMedicalOptionCategoryByIdAsync(5);
    /// if (category != null)
    /// {
    ///     Console.WriteLine($"Category: {category.MedicalOptionCategoryName}");
    ///     Console.WriteLine($"Total options: {category.MedicalOptions.Count}");
    /// }
    /// </code>
    /// </example>
    Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int categoryId);

    /// <summary>
    /// Checks if a medical option category exists in the system.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category to check.</param>
    /// <returns>True if the category exists; otherwise, false.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the provided categoryId is invalid.</exception>
    /// <exception cref="System.Exception">Thrown when database access fails.</exception>
    /// <example>
    /// <code>
    /// bool categoryExists = await _medicalOptionService.MedicalOptionCategoryExistsAsync(5);
    /// if (!categoryExists)
    /// {
    ///     Console.WriteLine("Category does not exist");
    /// }
    /// </code>
    /// </example>
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);

    /// <summary>
    /// Checks if a specific medical option exists in the system.
    /// </summary>
    /// <param name="optionId">The unique identifier of the medical option to check.</param>
    /// <returns>True if the medical option exists; otherwise, false.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the provided optionId is invalid.</exception>
    /// <exception cref="System.Exception">Thrown when database access fails.</exception>
    /// <example>
    /// <code>
    /// bool optionExists = await _medicalOptionService.MedicalOptionExistsAsync(123);
    /// if (optionExists)
    /// {
    ///     Console.WriteLine("Medical option is available");
    /// }
    /// </code>
    /// </example>
    Task<Boolean> MedicalOptionExistsAsync(int optionId);

    /// <summary>
    /// Retrieves all medical options that belong to a specific category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <returns>A list of medical options within the specified category; empty list if category has no options.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the provided categoryId is invalid.</exception>
    /// <exception cref="System.Exception">Thrown when database access fails.</exception>
    /// <example>
    /// <code>
    /// var options = await _medicalOptionService.GetAllOptionsUnderCategoryAsync(5);
    /// Console.WriteLine($"Found {options.Count} options in category");
    /// foreach (var option in options.Where(o => o != null))
    /// {
    ///     Console.WriteLine($"- {option.MedicalOptionName}");
    /// }
    /// </code>
    /// </example>
    Task<List<MedicalOptionDto?>> GetAllOptionsUnderCategoryAsync(int categoryId);

    /// <summary>
    /// Verifies that a medical option belongs to a specified category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <param name="optionId">The unique identifier of the medical option.</param>
    /// <returns>True if the medical option exists within the specified category; otherwise, false.</returns>
    /// <exception cref="System.ArgumentException">Thrown when either categoryId or optionId is invalid.</exception>
    /// <exception cref="System.Exception">Thrown when database access fails.</exception>
    /// <example>
    /// <code>
    /// bool isValidHierarchy = await _medicalOptionService.MedicalOptionExistsWithinCategoryAsync(5, 123);
    /// if (isValidHierarchy)
    /// {
    ///     Console.WriteLine("Option belongs to the specified category");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Invalid category-option relationship");
    /// }
    /// </code>
    /// </example>
    Task<Boolean> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId);

    /// <summary>
    /// Performs bulk updates of medical options within a specific category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category containing the options to update.</param>
    /// <param name="bulkUpdateDto">Collection of medical option variant updates to apply.</param>
    /// <param name="testDate">Optional test date for temporal validation; if null, current date is used.</param>
    /// <returns>A read-only list of updated medical option data transfer objects.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when bulkUpdateDto is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when categoryId is invalid or bulkUpdateDto is empty.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when validation fails for any of the updates.</exception>
    /// <exception cref="System.Exception">Thrown when database access fails or transaction encounters an error.</exception>
    /// <remarks>
    /// This method performs comprehensive validation including:
    /// - Category existence verification
    /// - Option existence and category membership validation
    /// - Salary bracket consistency checks
    /// - Contribution amount validations
    /// - Business rule enforcement
    /// 
    /// The operation is performed within a transaction to ensure data consistency.
    /// If any validation fails, the entire operation is rolled back.
    /// </remarks>
    /// <example>
    /// <code>
    /// var updates = new List&lt;UpdateMedicalOptionVariantsDto&gt;
    /// {
    ///     new UpdateMedicalOptionVariantsDto
    ///     {
    ///         MedicalOptionId = 123,
    ///         SalaryBracketMin = 5000,
    ///         SalaryBracketMax = 10000,
    ///         MonthlyRiskContributionPrincipal = 150.50m,
    ///         TotalMonthlyContributionsAdult = 200.00m
    ///     },
    ///     new UpdateMedicalOptionVariantsDto
    ///     {
    ///         MedicalOptionId = 124,
    ///         SalaryBracketMin = 10001,
    ///         SalaryBracketMax = 20000,
    ///         MonthlyRiskContributionPrincipal = 250.75m,
    ///         TotalMonthlyContributionsAdult = 300.00m
    ///     }
    /// };
    /// 
    /// var updatedOptions = await _medicalOptionService.BulkUpdateMedicalOptionsByCategoryAsync(
    ///     categoryId: 5, 
    ///     bulkUpdateDto: updates, 
    ///     testDate: DateTime.Now);
    ///     
    /// Console.WriteLine($"Successfully updated {updatedOptions.Count} medical options");
    /// </code>
    /// </example>
    Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateMedicalOptionsByCategoryAsync(
      int categoryId, IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto, DateTime? testDate = null);
    
    // New Methods
    // Get
      // Get all Possible options for employee based on salary
    Task<IReadOnlyList<MedicalOptionDto>> GetAllOptionsWithinEmployeeSalary(decimal salaryAmount);
      // Get eligible options for employee
    Task<IReadOnlyCollection<MedicalOptionDto>> GetEmployeeEligibleOptions(string employeeId);
      // Get All options under category via category ID
    Task<IReadOnlyList<MedicalOptionDto>> GetAllCategoryOptionsById(int id);
      
      // Medical Options Category
    Task<List<MedicalOptionCategoryDto>> GetAllMedicalOptionCategories();

    Task<MedicalOptionCategoryDto> GetCategoryById(int id);
     
      // Create
    Task<MedicalOptionCategoryDto> CreateMedicalOptionCategory(CreateMedicalOptionCategoryDto createCategoryPayload);
    Task<List<CreateMedicalOptionVariantsDto>> CreateBulkOptionsByExistingCategoryId(int id, CreateMedicalOptionVariantsDto createOptionsPayload);
      // Update
    Task<MedicalOptionCategoryDto> UpdateExistingCategoryById(int id,
      UpdateMedicalOptionCategoryDto updateCategoryPayload);
    
  }  
}