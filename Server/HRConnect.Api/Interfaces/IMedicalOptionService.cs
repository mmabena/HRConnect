namespace HRConnect.Api.Interfaces
{
  using DTOs.MedicalOption;
  using Models;

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
    /// Retrieves a specific group of medical options by its unique identifier (CategoryId).
    /// </summary>
    /// <param name="id">The unique identifier of the medical option.</param>
    /// <returns>The medical option if found; otherwise, null.</returns>
    Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id);

    /// <summary>Retrieves a medical option category by its identifier, including all associated options.</summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <returns>The medical option category with its options if found; otherwise, null.</returns>
    Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int categoryId);

    /// <summary>Checks if a medical option category exists in the system.</summary>
    /// <param name="categoryId">The unique identifier of the medical option category to check.</param>
    /// <returns>True if the category exists; otherwise, false.</returns>
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);

    /// <summary>Checks if a specific medical option exists in the system.</summary>
    /// <param name="optionId">The unique identifier of the medical option to check.</param>
    /// <returns>True if the medical option exists; otherwise, false.</returns>
    Task<Boolean> MedicalOptionExistsAsync(int optionId);

    /// <summary>Retrieves all medical options that belong to a specific category.</summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <returns>A list of medical options within the category; empty list if none found.</returns>
    Task<List<MedicalOptionDto?>> GetAllOptionsUnderCategoryAsync(int categoryId);

    /// <summary>Verifies that a medical option belongs to a specified category.</summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <param name="optionId">The unique identifier of the medical option.</param>
    /// <returns>True if the option exists within the category; otherwise, false.</returns>
    Task<Boolean> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId);

    /// <summary>Performs bulk updates of medical options within a specific category.</summary>
    /// <param name="categoryId">The unique identifier of the medical option category containing the options to update.</param>
    /// <param name="bulkUpdateDto">Collection of medical option updates to apply.</param>
    /// <param name="testDate">Optional test date for temporal validation; if null, current date is used.</param>
    /// <returns>A read-only list of updated medical option data transfer objects.</returns>
    /// <remarks>Validates category existence, option membership, salary brackets, and contribution amounts. Transaction-backed for data consistency.</remarks>
    Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateMedicalOptionsByCategoryAsync(
      int categoryId, IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      DateTime? testDate = null);

    /// <summary>Retrieves all medical option categories and options whose salary brackets match the specified salary amount.</summary>
    /// <param name="salaryAmount">The employee's salary to match against salary brackets.</param>
    /// <returns>A read-only list of medical option categories with matching options.</returns>
    Task<IReadOnlyList<MedicalOptionCategoryDto>> GetAllOptionsWithinEmployeeSalary(
      decimal salaryAmount);

    /// <summary>Retrieves all eligible medical options available to a specific employee.</summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <returns>A list of medical options grouped by category ID for the employee.</returns>
    Task<List<IGrouping<int, MedicalOptionDto>>> GetEmployeeEligibleOptions(string employeeId);

    /// <summary>Retrieves all category options for a specified category by ID.</summary>
    /// <param name="id">The unique identifier of the medical option category.</param>
    /// <returns>A list of medical option categories with their options.</returns>
    Task<List<MedicalOptionCategoryDto>> GetAllCategoryOptionsById(int id);

    /// <summary>Retrieves all medical option categories, grouped by ID.</summary>
    /// <returns>A list of medical option categories grouped by their identifiers.</returns>
    Task<List<IGrouping<int, MedicalOptionCategory>>> GetAllMedicalOptionCategories();

    /// <summary>Retrieves a specific medical option category by its ID.</summary>
    /// <param name="id">The unique identifier of the medical option category.</param>
    /// <returns>The medical option category details if found; otherwise, null.</returns>
    Task<List<MedicalOptionCategoryOnlyDto>> GetCategoryById(int id);
    
    /// <summary>Retrieves the current database copy of all medical options.</summary>
    /// <returns>A read-only list of all medical options from the database.</returns>
    Task<IReadOnlyList<MedicalOptionDto>> GetCurrentDbCopy();

    /// <summary>Creates a new medical option category.</summary>
    /// <param name="createCategoryPayload">The medical option category data to create.</param>
    /// <returns>The newly created medical option category.</returns>
    Task<MedicalOptionCategoryDto> CreateMedicalOptionCategory(
      CreateMedicalOptionCategoryDto createCategoryPayload);

    /// <summary>Creates multiple medical options within an existing category.</summary>
    /// <param name="id">The unique identifier of the existing medical option category.</param>
    /// <param name="createOptionsPayload">Collection of medical options to create.</param>
    /// <param name="testDate">Optional test date for temporal validation; if null, current date is used.</param>
    /// <returns>A read-only list of created medical option variants.</returns>
    Task<IReadOnlyList<CreateMedicalOptionVariantsDto>> CreateBulkOptionsByExistingCategoryId(
      int id,
      IReadOnlyCollection<CreateMedicalOptionVariantsDto> createOptionsPayload,
      DateTime? testDate = null);

    /// <summary>Updates an existing medical option category.</summary>
    /// <param name="id">The unique identifier of the medical option category to update.</param>
    /// <param name="updateCategoryPayload">The updated medical option category data.</param>
    /// <returns>The updated medical option category.</returns>
    Task<MedicalOptionCategoryDto> UpdateExistingCategoryById(int id,
      UpdateMedicalOptionCategoryDto updateCategoryPayload);
  }
}