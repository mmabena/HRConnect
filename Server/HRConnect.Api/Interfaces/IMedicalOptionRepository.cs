namespace HRConnect.Api.Interfaces
{
  using DTOs.MedicalOption;
  using Models;

  /// <summary>
  /// Defines the contract for data access operations related to medical options and their categories.
  /// This repository provides methods for retrieving, validating, and updating medical option data,
  /// including salary brackets, contribution calculations, and category-based operations.
  /// </summary>
  public interface IMedicalOptionRepository
  {
    /// <summary>
    /// Retrieves all medical options grouped by their category ID.
    /// </summary>
    /// <returns>A list of groupings where each group contains medical options belonging to the same category.</returns>
    Task<List<IGrouping<int, MedicalOption>>> GetGroupedMedicalOptionsAsync();

    /// <summary>
    /// Retrieves a specific medical option by its unique identifier.
    /// </summary>
    /// <param name="id">The medical option ID to search for.</param>
    /// <returns>MedicalOptionDto if found, null if not found.</returns>
    Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id);

    /// <summary>
    /// Retrieves medical option details by category ID.
    /// </summary>
    /// <param name="id">The category ID to search for.</param>
    /// <returns>MedicalOptionDto if found, null if not found.</returns>
    Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int id);

    /// <summary>
    /// Retrieves multiple medical options by their IDs in a single operation.
    /// </summary>
    /// <param name="ids">List of medical option IDs to retrieve.</param>
    /// <returns>List of MedicalOptionDto objects. Returns empty list if no options found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when ids parameter is null.</exception>
    Task<List<MedicalOptionDto>> GetMedicalOptionsByIdsAsync(List<int> ids);

    /// <summary>
    /// Retrieves a medical option category by its ID.
    /// </summary>
    /// <param name="id">The category ID to search for.</param>
    /// <returns>MedicalOptionCategory if found, null if not found.
    /// </returns>
    Task<MedicalOptionCategory?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Retrieves all medical options that belongs to a category containing the specified option name.
    /// </summary>
    /// <param name="optionName">The name pattern to search for within categories.</param>
    /// <returns>List of MedicalOptionDto objects. Returns empty list if no matches found.</returns>
    /// <exception cref="ArgumentException">Thrown when optionName is null or empty.</exception>
    Task<List<MedicalOptionDto?>> GetAllMedicalOptionsUnderCategoryVariantAsync(string optionName);

    /// <summary>
    /// Checks if a medical option category exists.
    /// </summary>
    /// <param name="categoryId">The category ID to validate.</param>
    /// <returns>True if category exists, false otherwise.</returns>
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);

    /// <summary>
    /// Checks if a medical option exists.
    /// </summary>
    /// <param name="optionId">The medical option ID to validate.</param>
    /// <returns>True if option exists, false otherwise.</returns>
    Task<Boolean> MedicalOptionExistsAsync(int optionId);

    /// <summary>
    /// Retrieves all medical options belonging to a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID to filter by.</param>
    /// <returns>List of MedicalOptionDto objects. Returns empty list if category has no options or doesn't exist.
    /// </returns>
    Task<List<MedicalOptionDto?>> GetAllOptionsUnderCategoryAsync(int categoryId);

    /// <summary>
    /// Validates that a specific medical option exists within a particular category.
    /// </summary>
    /// <param name="categoryId">The category ID to check within.</param>
    /// <param name="optionId">The medical option ID to validate.</param>
    /// <returns>True if the option exists within the specified category, false otherwise.</returns>
    Task<Boolean> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId);

    /// <summary>
    /// Performs bulk updates of medical options within a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID containing options to update.</param>
    /// <param name="bulkUpdateDto">Collection of update DTOs containing the new values.</param>
    /// <returns>Read-only list of updated MedicalOptionDto objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bulkUpdateDto is null.</exception>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid or no matching options found.</exception>
    Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateByCategoryIdAsync(int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto);
    
    /// <summary>Retrieves all medical options matching an employee's salary bracket.</summary>
    /// <param name="salaryAmount">The employee's salary to match against salary brackets.</param>
    /// <returns>Medical options grouped by category ID that match the salary bracket.</returns>
    Task<List<IGrouping<int, MedicalOption>>> GetAllOptionsWithinEmployeeSalary(
      decimal salaryAmount);

    /// <summary>Retrieves all eligible medical options available to a specific employee.</summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <returns>Eligible medical options grouped by category ID for the employee.</returns>
    Task<List<IGrouping<int, MedicalOptionDto>>> GetEmployeeEligibleOptions(string employeeId);

    /// <summary>Retrieves all medical options under a specific category by its ID.</summary>
    /// <param name="id">The unique identifier of the medical option category.</param>
    /// <returns>Medical options grouped by category ID for the specified category.</returns>
    Task<List<IGrouping<int, MedicalOption>>> GetAllCategoryOptionsById(int id);

    /// <summary>Retrieves all medical option categories grouped by their unique identifiers.</summary>
    /// <returns>Medical option categories grouped by category ID.</returns>
    Task<List<IGrouping<int, MedicalOptionCategory>>> GetAllMedicalOptionCategories();

    /// <summary>Retrieves all medical option categories by category ID.</summary>
    /// <param name="id">The unique identifier of the medical option category.</param>
    /// <returns>List of medical option categories matching the ID.</returns>
    Task<List<MedicalOptionCategory>> GetCategoryById(int id);
    
    /// <summary>Retrieves the current database copy of all medical options.</summary>
    /// <returns>A read-only list of all medical options from the database.</returns>
    Task<IReadOnlyList<MedicalOptionDto>> GetCurrentDbCopy();

    /// <summary>Creates a new medical option category.</summary>
    /// <param name="createCategoryPayload">The medical option category data to create.</param>
    /// <returns>The newly created medical option category as a DTO.</returns>
    Task<MedicalOptionCategoryDto> CreateMedicalOptionCategory(
      CreateMedicalOptionCategoryDto createCategoryPayload);

    /// <summary>Creates multiple medical options within an existing category.</summary>
    /// <param name="id">The unique identifier of the medical option category.</param>
    /// <param name="createOptionsPayload">Collection of medical options to create.</param>
    /// <returns>List of created medical option variants.</returns>
    Task<List<CreateMedicalOptionVariantsDto>> CreateBulkOptionsByExistingCategoryId(int id,
      IReadOnlyCollection<CreateMedicalOptionVariantsDto> createOptionsPayload);

    /// <summary>Updates an existing medical option category.</summary>
    /// <param name="id">The unique identifier of the medical option category to update.</param>
    /// <param name="updateCategoryPayload">The updated medical option category data.</param>
    /// <returns>The updated medical option category as a DTO.</returns>
    Task<MedicalOptionCategoryDto> UpdateExistingCategoryById(int id,
      UpdateMedicalOptionCategoryDto updateCategoryPayload);
  }
}



