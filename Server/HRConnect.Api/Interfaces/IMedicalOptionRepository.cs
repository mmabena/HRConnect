namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;
  
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
    /// <remarks>
    /// Ideal for displaying medical options organized by category in UI components.
    /// Each group key represents the MedicalOptionCategoryId.
    /// </remarks>
    Task<List<IGrouping<int, MedicalOption>>> GetGroupedMedicalOptionsAsync();

    /// <summary>
    /// Retrieves a specific medical option by its unique identifier.
    /// </summary>
    /// <param name="id">The medical option ID to search for.</param>
    /// <returns>MedicalOptionDto if found, null if not found.</returns>
    /// <example>
    /// <code>
    /// var option = await repository.GetMedicalOptionByIdAsync(123);
    /// if (option != null) {
    ///     Console.WriteLine($"Option: {option.MedicalOptionName}");
    /// }
    /// </code>
    /// </example>
    Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id);

    /// <summary>
    /// Retrieves medical option details by category ID.
    /// </summary>
    /// <param name="id">The category ID to search for.</param>
    /// <returns>MedicalOptionDto if found, null if not found.</returns>
    /// <remarks>
    /// This method returns medical options filtered by a specific category.
    /// Use when you need to get options within a particular category context.
    /// </remarks>
    Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int id);

    /// <summary>
    /// Retrieves multiple medical options by their IDs in a single operation.
    /// </summary>
    /// <param name="ids">List of medical option IDs to retrieve.</param>
    /// <returns>List of MedicalOptionDto objects. Returns empty list if no options found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when ids parameter is null.</exception>
    /// <remarks>
    /// More efficient than multiple individual calls when retrieving multiple options.
    /// Useful for bulk operations and comparison scenarios.
    /// </remarks>
    Task<List<MedicalOptionDto>> GetMedicalOptionsByIdsAsync(List<int> ids);

    /// <summary>
    /// Retrieves a medical option category by its ID.
    /// </summary>
    /// <param name="id">The category ID to search for.</param>
    /// <returns>MedicalOptionCategory if found, null if not found.</returns>
    /// <remarks>
    /// Returns the category entity with navigation properties including associated medical options.
    /// Use when you need category metadata and want to access related options.
    /// </remarks>
    Task<MedicalOptionCategory?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Retrieves all medical options that belong to categories containing the specified option name.
    /// </summary>
    /// <param name="optionName">The name pattern to search for within categories.</param>
    /// <returns>List of MedicalOptionDto objects. Returns empty list if no matches found.</returns>
    /// <exception cref="ArgumentException">Thrown when optionName is null or empty.</exception>
    /// <remarks>
    /// Performs a pattern match search across category names to find related medical options.
    /// Useful for finding all variants of a particular medical option type across different categories.
    /// The search is case-insensitive and supports partial matching.
    /// </remarks>
    Task<List<MedicalOptionDto?>> GetAllMedicalOptionsUnderCategoryVariantAsync(string optionName);

    /// <summary>
    /// Checks if a medical option category exists.
    /// </summary>
    /// <param name="categoryId">The category ID to validate.</param>
    /// <returns>True if category exists, false otherwise.</returns>
    /// <remarks>
    /// Helper method for pre-validation before category-related operations.
    /// More efficient than retrieving the full category entity when only checking existence.
    /// </remarks>
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);

    /// <summary>
    /// Checks if a medical option exists.
    /// </summary>
    /// <param name="optionId">The medical option ID to validate.</param>
    /// <returns>True if option exists, false otherwise.</returns>
    /// <remarks>
    /// Helper method for pre-validation before option-related operations.
    /// More efficient than retrieving the full option entity when only checking existence.
    /// </remarks>
    Task<Boolean> MedicalOptionExistsAsync(int optionId);
    
    /// <summary>
    /// Retrieves all medical options belonging to a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID to filter by.</param>
    /// <returns>List of MedicalOptionDto objects. Returns empty list if category has no options or doesn't exist.</returns>
    /// <remarks>
    /// Use for displaying all available options within a category for employee selection.
    /// Options are returned in their natural order from the data source.
    /// </remarks>
    Task<List<MedicalOptionDto>> GetAllOptionsUnderCategoryAsync(int categoryId);

    /// <summary>
    /// Validates that a specific medical option exists within a particular category.
    /// </summary>
    /// <param name="categoryId">The category ID to check within.</param>
    /// <param name="optionId">The medical option ID to validate.</param>
    /// <returns>True if the option exists within the specified category, false otherwise.</returns>
    /// <remarks>
    /// Ensures data integrity when linking options to categories.
    /// Validates the relationship between category and option entities.
    /// Useful for preventing invalid associations in business logic.
    /// </remarks>
    Task<Boolean> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId);
    
    /// <summary>
    /// Performs bulk updates of medical options within a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID containing options to update.</param>
    /// <param name="bulkUpdateDto">Collection of update DTOs containing the new values.</param>
    /// <returns>Read-only list of updated MedicalOptionDto objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bulkUpdateDto is null.</exception>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid or no matching options found.</exception>
    /// <remarks>
    /// Efficiently updates multiple medical options in a single transaction.
    /// Ideal for annual contribution rate changes or bulk price adjustments.
    /// All updates are performed atomically - either all succeed or all fail.
    /// The method updates only the properties specified in the DTO; null values preserve existing data.
    /// </remarks>
    /// <example>
    /// <code>
    /// var updates = new List&lt;UpdateMedicalOptionVariantsDto&gt;
    /// {
    ///     new UpdateMedicalOptionVariantsDto { MedicalOptionId = 1, MonthlyRiskContributionAdult = 500.00m },
    ///     new UpdateMedicalOptionVariantsDto { MedicalOptionId = 2, MonthlyRiskContributionAdult = 550.00m }
    /// };
    /// var results = await repository.BulkUpdateByCategoryIdAsync(1, updates);
    /// </code>
    /// </example>
    Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateByCategoryIdAsync(int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto);
    
  }
}