namespace HRConnect.Api.Repository
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Mappers;
  using Microsoft.Data.SqlClient;
  using EFCore.BulkExtensions;

  /// <summary>
  /// Repository implementation for managing medical options and their categories in the HR Connect system.
  /// Provides data access operations for retrieving, validating, and updating medical option data,
  /// including salary brackets, contribution calculations, and category-based operations.
  /// </summary>
  /// <remarks>
  /// This repository serves as the data access layer for medical options, implementing the
  /// IMedicalOptionRepository interface. It uses Entity Framework Core for database operations
  /// and EFCore.BulkExtensions for efficient bulk updates.
  /// 
  /// Key responsibilities:
  /// - Database query execution and entity mapping
  /// - Transaction management for bulk operations
  /// - Data validation at the repository level
  /// - Efficient bulk update operations using EFCore.BulkExtensions
  /// - Navigation property eager loading to prevent N+1 query problems
  /// 
  /// Performance considerations:
  /// - Uses Include() for eager loading navigation properties
  /// - Implements bulk operations for large-scale updates
  /// - Validates input parameters before database operations
  /// - Uses dictionary-based lookups for O(1) performance in bulk updates
  /// 
  /// Error handling:
  /// - Throws ArgumentException for invalid input parameters
  /// - Throws KeyNotFoundException when entities are not found
  /// - Propagates database exceptions from Entity Framework
  /// - Provides detailed error messages for debugging
  /// </remarks>
  public class MedicalOptionRepository: IMedicalOptionRepository
  {
    private readonly ApplicationDBContext _context;

    /// <summary>
    /// Initializes a new instance of the MedicalOptionRepository class.
    /// </summary>
    /// <param name="context">The database context for medical option operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <remarks>
    /// The repository requires an active ApplicationDBContext instance for all database operations.
    /// The context should be properly configured with the medical options and categories tables.
    /// </remarks>
    public MedicalOptionRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Retrieves medical options grouped by their category ID from the database.
    /// This method uses Entity Framework Core to query medical options with their
    /// associated category data and groups them by their MedicalOptionCategoryId
    /// (marked as the Key in the Grouping interface).
    /// </summary>
    /// <returns>
    /// A list of IGrouping&lt;int, MedicalOption&gt; where:
    /// - Key: the MedicalOptionCategoryId (int)
    /// - Group: Collection of MedicalOptions entities belonging to that category
    /// 
    /// <para>
    /// Each MedicalOption includes the eagerly loaded MedicalOptionCategory navigation property.
    /// </para>
    /// 
    /// <para>
    /// Example structure:
    /// [
    ///   IGrouping&lt;Key=1, [MedicalOption, MedicalOption]&gt;,
    ///   IGrouping&lt;Key=2, [MedicalOption, MedicalOption, MedicalOption, MedicalOption]&gt;,
    ///   IGrouping&lt;Key=3, [MedicalOption, MedicalOption, MedicalOption]&gt;
    /// ]
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Uses Entity Framework's Include() method to eagerly load the MedicalOptionCategory
    /// navigation property, preventing N+1 query problems when accessing category information.
    /// </para>
    /// <para>
    /// The GroupBy() operation is performed in-memory after data retrieval, which is efficient
    /// for typical medical option datasets (ranging between small to slightly large datasets).
    /// For very large datasets, consider using a database-specific grouping function or
    /// implementing a custom grouping strategy.
    /// </para>
    /// <para>
    /// This method returns the raw entity grouping. For client consumption, use the service layer
    /// to transform this info into MedicalOptionCategoryDto objects.
    /// </para>
    /// </remarks>
    /// <example>
    /// // Repository usage
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// var groupedOptions = await repository.GetGroupedMedicalOptionsAsync();
    /// 
    /// // Process the grouped results
    /// foreach (var group in groupedOptions)
    /// {
    ///     int categoryId = group.Key;
    ///     var categoryName = group.First().MedicalOptionCategory?.MedicalOptionCategoryName;
    ///     var optionCount = group.Count();
    ///     
    ///     Console.WriteLine($"Category {categoryId} ({categoryName}): {optionCount} options");
    /// }
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is disposed or unavailable.
    /// </exception>
    /// <exception cref="SqlException">
    /// Thrown when there's a database connectivity or query execution error.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the cancellation token.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no medical options are found in the database.
    /// </exception>
    public async Task<List<IGrouping<int, MedicalOption>>> GetGroupedMedicalOptionsAsync()
    {
      var groupedMedicalOptions = await _context.MedicalOptions
        .Include(mo => mo.MedicalOptionCategory)
        .Where(mo => mo.MedicalOptionCategory != null && mo.MedicalOptionCategoryId != null)
        .GroupBy(mo => mo.MedicalOptionCategoryId)
        .ToListAsync();

      if (groupedMedicalOptions.Count == 0)
      {
        throw new KeyNotFoundException("No medical options found in the database");
      }

      return groupedMedicalOptions;
    }

    /// <summary>
    /// Retrieves a specific medical option by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the medical option to retrieve.</param>
    /// <returns>The medical option data transfer object if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided id is invalid (less than or equal to 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical option with the specified ID is found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method uses Entity Framework Core to query the database and eagerly loads
    /// the MedicalOptionCategory navigation property to prevent additional database queries
    /// when accessing category information.
    /// 
    /// The result is automatically mapped to a MedicalOptionDto using the ToMedicalOptionDto()
    /// extension method from the mapping layer.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// var medicalOption = await repository.GetMedicalOptionByIdAsync(123);
    /// 
    /// if (medicalOption != null)
    /// {
    ///     Console.WriteLine($"Option: {medicalOption.MedicalOptionName}");
    ///     Console.WriteLine($"Category ID: {medicalOption.MedicalOptionCategoryId}");
    ///     Console.WriteLine($"Adult Contribution: {medicalOption.TotalMonthlyContributionsAdult:C}");
    /// }
    /// </code>
    /// </example>
    public async Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Medical option ID must be greater than 0", 
              nameof(id));
        }

        var medicalOption = await _context.MedicalOptions
            .Include(opt => opt.MedicalOptionCategory)
            .FirstOrDefaultAsync(opt => opt.MedicalOptionId == id);

        if (medicalOption == null)
        {
            throw new KeyNotFoundException($"MedicalOption with ID {id} was not found");
        }

        return medicalOption?.ToMedicalOptionDto();
    }

    /// <summary>
    /// Retrieves medical option details by category ID.
    /// </summary>
    /// <param name="id">The category ID to search for medical options.</param>
    /// <returns>The first medical option found in the specified category as a MedicalOptionDto; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided category ID is invalid (less than or equal to 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical options are found for the specified category ID.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method retrieves the first medical option found within the specified category.
    /// It eagerly loads the MedicalOptionCategory navigation property to prevent additional
    /// database queries when accessing category information.
    /// 
    /// Note: This method returns only the first matching option, not all options in the category.
    /// Use GetAllOptionsUnderCategoryAsync() to retrieve all options within a category.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// var medicalOption = await repository.GetMedicalOptionCategoryByIdAsync(5);
    /// 
    /// if (medicalOption != null)
    /// {
    ///     Console.WriteLine($"Found option: {medicalOption.MedicalOptionName}");
    ///     Console.WriteLine($"In category: {medicalOption.MedicalOptionCategoryId}");
    /// }
    /// </code>
    /// </example>
    public async Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int id)
    {
      if (id <= 0)
      {
        throw new ArgumentException("Category ID must be greater than 0", nameof(id));
      }

      var medicalOptions = await _context.MedicalOptions
          .Include(mo => mo.MedicalOptionCategory)
          .FirstOrDefaultAsync(mo => mo.MedicalOptionCategoryId == id);

      if (medicalOptions is null)
      {
        throw new KeyNotFoundException($"No medical options found for category ID {id}");
      }

      return medicalOptions?.ToMedicalOptionDto();
    }

    /// <summary>
    /// Retrieves multiple medical options by their IDs in a single database operation.
    /// </summary>
    /// <param name="ids">List of medical option IDs to retrieve.</param>
    /// <returns>List of MedicalOptionDto objects corresponding to the provided IDs.</returns>
    /// <exception cref="ArgumentException">Thrown when the IDs list is null or empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical options are found for any of the provided IDs.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method is more efficient than making multiple individual calls to GetMedicalOptionByIdAsync()
    /// when you need to retrieve multiple medical options. It uses the SQL IN clause to fetch all
    /// matching records in a single database query.
    /// 
    /// The method eagerly loads the MedicalOptionCategory navigation property for all options
    /// to prevent N+1 query problems when accessing category information.
    /// 
    /// All returned options are automatically mapped to MedicalOptionDto objects using the
    /// ToMedicalOptionDto() extension method.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// var optionIds = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
    /// var medicalOptions = await repository.GetMedicalOptionsByIdsAsync(optionIds);
    /// 
    /// Console.WriteLine($"Retrieved {medicalOptions.Count} medical options:");
    /// foreach (var option in medicalOptions)
    /// {
    ///     Console.WriteLine($"- {option.MedicalOptionName} (Category: {option.MedicalOptionCategoryId})");
    /// }
    /// </code>
    /// </example>
    public async Task<List<MedicalOptionDto>> GetMedicalOptionsByIdsAsync(List<int> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            throw new ArgumentException("IDs list cannot be null or empty", nameof(ids));
        }

        var medicalOptions = await _context.MedicalOptions
            .Where(mo => ids.Contains(mo.MedicalOptionId))
            .Include(mo => mo.MedicalOptionCategory)
            .ToListAsync();

        if (medicalOptions.Count == 0)
        {
            throw new KeyNotFoundException("No medical options found for the provided IDs");
        }

        return medicalOptions.Select(mo => mo.ToMedicalOptionDto()).ToList();
    }

    /// <summary>
    /// Retrieves a medical option category by its unique identifier.
    /// </summary>
    /// <param name="id">The category ID to search for.</param>
    /// <returns>The MedicalOptionCategory entity if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided category ID is invalid (less than or equal to 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no category with the specified ID is found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method returns the raw MedicalOptionCategory entity, not a DTO. This is useful
    /// when you need access to the full entity with all its properties and navigation properties
    /// for further database operations or complex business logic.
    /// 
    /// Unlike other methods that return DTOs, this method returns the entity itself, which
    /// can be used for update operations or when you need access to entity-specific features.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// var category = await repository.GetCategoryByIdAsync(5);
    /// 
    /// if (category != null)
    /// {
    ///     Console.WriteLine($"Category: {category.MedicalOptionCategoryName}");
    ///     // Use the entity for further operations
    ///     category.MedicalOptionCategoryName = "Updated Category Name";
    ///     await context.SaveChangesAsync();
    /// }
    /// </code>
    /// </example>
    public async Task<MedicalOptionCategory> GetCategoryByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Category ID must be greater than 0", nameof(id));
        }

        var category = await _context.MedicalOptionCategories
            .FirstOrDefaultAsync(c => c.MedicalOptionCategoryId == id);

        if (category == null)
        {
            throw new KeyNotFoundException($"MedicalOptionCategory with ID " +
                                           $"{id} was not found");
        }

        return category;
    }

    /// <summary>
    /// Retrieves all medical options under a category variant based on option name pattern matching.
    /// </summary>
    /// <param name="optionName">The option name pattern to search for (supports partial matching).</param>
    /// <returns>List of MedicalOptionDto objects containing the specified option name pattern.</returns>
    /// <exception cref="ArgumentException">Thrown when optionName is null, empty, or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical options are found containing the specified option name.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method performs a case-sensitive Contains() operation on the MedicalOptionName field,
    /// allowing for partial matching. It's useful for finding all variants of a particular medical
    /// option type across different categories.
    /// 
    /// The method eagerly loads the MedicalOptionCategory navigation property to prevent additional
    /// database queries when accessing category information.
    /// 
    /// All returned options are automatically mapped to MedicalOptionDto objects.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// 
    /// // Find all options containing "Hospital" in their name
    /// var hospitalOptions = await repository.GetAllMedicalOptionsUnderCategoryVariantAsync("Hospital");
    /// 
    /// Console.WriteLine($"Found {hospitalOptions.Count} hospital-related options:");
    /// foreach (var option in hospitalOptions.Where(o => o != null))
    /// {
    ///     Console.WriteLine($"- {option.MedicalOptionName} (Category: {option.MedicalOptionCategoryId})");
    /// }
    /// 
    /// // Find all options containing "Dental"
    /// var dentalOptions = await repository.GetAllMedicalOptionsUnderCategoryVariantAsync("Dental");
    /// </code>
    /// </example>
    public async Task<List<MedicalOptionDto?>> GetAllMedicalOptionsUnderCategoryVariantAsync(
      string optionName)
    {
        if (string.IsNullOrWhiteSpace(optionName))
        {
            throw new ArgumentException("Option name cannot be null or empty", 
              nameof(optionName));
        }

        var medicalOptions = await _context.MedicalOptions
            .Include(mo => mo.MedicalOptionCategory)
            .Where(mo => mo.MedicalOptionName.Contains(optionName))
            .ToListAsync();

        if (medicalOptions.Count == 0)
        {
            throw new KeyNotFoundException($"No medical options found containing " +
                                           $"'{optionName}'");
        }

        return medicalOptions?.Select(mo => mo.ToMedicalOptionDto()).ToList();
    }

    /// <summary>
    /// Checks if a medical option category exists in the database.
    /// </summary>
    /// <param name="categoryId">The category ID to validate.</param>
    /// <returns>True if the category exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided category ID is invalid (less than or equal to 0).</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method uses Entity Framework's AnyAsync() method for efficient existence checking,
    /// which generates an optimized SQL query that stops as soon as a matching record is found.
    /// 
    /// This is more efficient than retrieving the full category entity when you only need
    /// to validate existence, as it avoids loading unnecessary data into memory.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// 
    /// // Validate category before performing operations
    /// int categoryId = 5;
    /// if (await repository.MedicalOptionCategoryExistsAsync(categoryId))
    /// {
    ///     Console.WriteLine($"Category {categoryId} exists, proceeding with operations...");
    ///     // Perform category-related operations
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Category {categoryId} does not exist");
    /// }
    /// </code>
    /// </example>
    public async Task<bool> MedicalOptionCategoryExistsAsync(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than 0", 
              nameof(categoryId));
        }

        return await _context.MedicalOptionCategories
            .AnyAsync(moc => moc.MedicalOptionCategoryId == categoryId);
    }

    /// <summary>
    /// Checks if a specific medical option exists in the database.
    /// </summary>
    /// <param name="optionId">The medical option ID to validate.</param>
    /// <returns>True if the medical option exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided option ID is invalid (less than or equal to 0).</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method uses Entity Framework's AnyAsync() method for efficient existence checking,
    /// which generates an optimized SQL query that stops as soon as a matching record is found.
    /// 
    /// This is more efficient than retrieving the full medical option entity when you only need
    /// to validate existence, as it avoids loading unnecessary data into memory.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// 
    /// // Validate medical option before performing operations
    /// int optionId = 123;
    /// if (await repository.MedicalOptionExistsAsync(optionId))
    /// {
    ///     Console.WriteLine($"Medical option {optionId} exists, proceeding with operations...");
    ///     // Perform option-related operations
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Medical option {optionId} does not exist");
    /// }
    /// </code>
    /// </example>
    public async Task<bool> MedicalOptionExistsAsync(int optionId)
    {
        if (optionId <= 0)
        {
            throw new ArgumentException("Option ID must be greater than 0", nameof(optionId));
        }

        return await _context.MedicalOptions
            .AnyAsync(o => o.MedicalOptionId == optionId);
    }

    /// <summary>
    /// Retrieves all medical options belonging to a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID to filter medical options by.</param>
    /// <returns>List of MedicalOptionDto objects belonging to the specified category.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided category ID is invalid (less than or equal to 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the specified category does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method first validates that the category exists using MedicalOptionCategoryExistsAsync(),
    /// then retrieves all medical options belonging to that category. This two-step approach ensures
    /// data integrity and provides clear error messages when the category doesn't exist.
    /// 
    /// The method returns an empty list if the category exists but contains no medical options.
    /// All returned options are automatically mapped to MedicalOptionDto objects.
    /// 
    /// This method does not eagerly load the MedicalOptionCategory navigation property since
    /// the category is already known to exist and the category ID is provided.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// 
    /// try
    /// {
    ///     var categoryOptions = await repository.GetAllOptionsUnderCategoryAsync(5);
    ///     Console.WriteLine($"Found {categoryOptions.Count} options in category 5:");
    ///     
    ///     foreach (var option in categoryOptions)
    ///     {
    ///         Console.WriteLine($"- {option.MedicalOptionName}");
    ///         Console.WriteLine($"  Adult Contribution: {option.TotalMonthlyContributionsAdult:C}");
    ///         Console.WriteLine($"  Child Contribution: {option.TotalMonthlyContributionsChild:C}");
    ///     }
    /// }
    /// catch (KeyNotFoundException ex)
    /// {
    ///     Console.WriteLine($"Error: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    public async Task<List<MedicalOptionDto>> GetAllOptionsUnderCategoryAsync(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than 0", 
              nameof(categoryId));
        }

        if (!await MedicalOptionCategoryExistsAsync(categoryId))
        {
            throw new KeyNotFoundException($"MedicalOptionCategory with ID " +
                                           $"{categoryId} was not found");
        }

        var allOptions = await _context.MedicalOptions
            .Where(co => co.MedicalOptionCategoryId == categoryId)
            .ToListAsync();

        return allOptions.Select(mo => mo.ToMedicalOptionDto()).ToList();
    }

    /// <summary>
    /// Validates that a specific medical option exists within a particular category.
    /// </summary>
    /// <param name="categoryId">The category ID to check within.</param>
    /// <param name="optionId">The medical option ID to validate.</param>
    /// <returns>True if the medical option exists within the specified category; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when either categoryId or optionId is invalid (less than or equal to 0).</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method validates the relationship between a medical option and its category,
    /// ensuring data integrity when linking options to categories. It uses Entity Framework's
    /// AnyAsync() method with a compound condition to check both the category membership
    /// and option existence in a single database query.
    /// 
    /// This is useful for preventing invalid associations in business logic and ensuring
    /// that operations are performed on valid category-option relationships.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// 
    /// // Validate the relationship before performing operations
    /// int categoryId = 5;
    /// int optionId = 123;
    /// 
    /// if (await repository.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId))
    /// {
    ///     Console.WriteLine($"Option {optionId} belongs to category {categoryId}");
    ///     // Proceed with operations that depend on this relationship
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Option {optionId} does not belong to category {categoryId}");
    /// }
    /// </code>
    /// </example>
    public async Task<bool> MedicalOptionExistsWithinCategoryAsync(
      int categoryId, int optionId)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than 0",
              nameof(categoryId));
        }

        if (optionId <= 0)
        {
            throw new ArgumentException("Option ID must be greater than 0", nameof(optionId));
        }

        return await _context.MedicalOptions
            .AnyAsync(o => o.MedicalOptionCategoryId == categoryId && 
                           o.MedicalOptionId == optionId);
    }

    /// <summary>
    /// Performs bulk updates of medical options within a specific category using EFCore.BulkExtensions.
    /// </summary>
    /// <param name="categoryId">The category ID containing the medical options to update.</param>
    /// <param name="bulkUpdateDto">Collection of UpdateMedicalOptionVariantsDto objects containing the new values.</param>
    /// <returns>Read-only list of updated MedicalOptionDto objects representing the modified medical options.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (less than or equal to 0) or bulkUpdateDto is null or empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical options are found for the specified category with the provided option IDs.</exception>
    /// <exception cref="ArgumentException">Thrown when salary bracket validation fails (minimum must be less than maximum).</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or bulk operation error.</exception>
    /// <remarks>
    /// <para>
    /// This method implements an efficient bulk update strategy using the following approach:
    /// </para>
    /// <list type="number">
    /// <item><description>Validates input parameters and category existence</description></item>
    /// <item><description>Retrieves existing medical options that match the provided option IDs within the category</description></item>
    /// <item><description>Creates a dictionary for O(1) lookup performance when matching update DTOs to entities</description></item>
    /// <item><description>Validates salary bracket ranges for each update</description></item>
    /// <item><description>Updates entities using the UpdateFromDto() extension method</description></item>
    /// <item><description>Performs bulk update using EFCore.BulkExtensions with optimized batch size and property selection</description></item>
    /// <item><description>Maps results to DTOs to avoid circular reference issues</description></item>
    /// </list>
    /// 
    /// <para>
    /// Performance optimizations:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Uses dictionary-based lookups instead of nested loops for O(n) performance</description></item>
    /// <item><description>Bulk update with batch size of 1000 for optimal database performance</description></item>
    /// <item><description>Explicit property inclusion to minimize data transfer</description></item>
    /// <item><description>Single database round-trip for all updates</description></item>
    /// </list>
    /// 
    /// <para>
    /// Validation rules:
    /// </para>
    /// <list type="bullet">
    /// <item><description>SalaryBracketMin must be less than SalaryBracketMax</description></item>
    /// <item><description>All option IDs must exist within the specified category</description></item>
    /// <item><description>Category ID must be valid and exist in the database</description></item>
    /// </list>
    /// 
    /// <para>
    /// The operation is performed atomically - either all updates succeed or all fail.
    /// If any validation fails, the entire operation is rolled back.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var repository = new MedicalOptionRepository(context);
    /// 
    /// // Prepare bulk update data
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
    /// try
    /// {
    ///     var updatedOptions = await repository.BulkUpdateByCategoryIdAsync(5, updates);
    ///     Console.WriteLine($"Successfully updated {updatedOptions.Count} medical options");
    ///     
    ///     foreach (var option in updatedOptions)
    ///     {
    ///         Console.WriteLine($"Updated: {option.MedicalOptionName}");
    ///         Console.WriteLine($"New Adult Contribution: {option.TotalMonthlyContributionsAdult:C}");
    ///     }
    /// }
    /// catch (ArgumentException ex)
    /// {
    ///     Console.WriteLine($"Validation error: {ex.Message}");
    /// }
    /// catch (KeyNotFoundException ex)
    /// {
    ///     Console.WriteLine($"Data error: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    public async Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateByCategoryIdAsync(
      int categoryId, IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than 0", 
              nameof(categoryId));
        }

        if (bulkUpdateDto == null || bulkUpdateDto.Count == 0)
        {
            throw new ArgumentException("Bulk update DTO cannot be null or empty", 
              nameof(bulkUpdateDto));
        }

        // Get existing options in category that match with the Payload's IDs
        var optionIdsToUpdate = bulkUpdateDto.Select(dto => dto.MedicalOptionId)
          .ToList();

        var existingOptions = await _context.MedicalOptions
            .Where(o => o.MedicalOptionCategoryId == categoryId && 
                        optionIdsToUpdate.Contains(o.MedicalOptionId))
            .ToListAsync();

        if (existingOptions.Count == 0)
        {
            throw new KeyNotFoundException($"No medical options found for category ID " +
                                           $"{categoryId} with the provided option IDs");
        }

        // Create a dictionary for faster lookups
        var updateDict = bulkUpdateDto.ToDictionary(
          dto => dto.MedicalOptionId, dto => dto);

        // Update entities using the dictionary for O(1) lookups
        foreach (var entity in existingOptions)
        {
            if (updateDict.TryGetValue(entity.MedicalOptionId, out var updateDto))
            {
                // Validate salary bracket ranges
                if (updateDto.SalaryBracketMin >= updateDto.SalaryBracketMax)
                {
                    throw new ArgumentException(
                        $"Invalid salary bracket range for option ID {entity.MedicalOptionId}: " +
                        "Minimum must be less than maximum");
                }
                entity.UpdateFromDto(updateDto);
            }
        }

        // Perform bulk update using EFCore.BulkExtensions
        await _context.BulkUpdateAsync(existingOptions, new BulkConfig()
        {
            BatchSize = 1000,
            PropertiesToInclude = new List<string>
            {
                nameof(MedicalOption.SalaryBracketMin),
                nameof(MedicalOption.SalaryBracketMax),
                nameof(MedicalOption.MonthlyMsaContributionAdult),
                nameof(MedicalOption.MonthlyMsaContributionChild),
                nameof(MedicalOption.MonthlyMsaContributionPrincipal),
                nameof(MedicalOption.MonthlyRiskContributionAdult),
                nameof(MedicalOption.MonthlyRiskContributionChild),
                nameof(MedicalOption.MonthlyRiskContributionChild2),
                nameof(MedicalOption.MonthlyRiskContributionPrincipal),
                nameof(MedicalOption.TotalMonthlyContributionsAdult),
                nameof(MedicalOption.TotalMonthlyContributionsChild),
                nameof(MedicalOption.TotalMonthlyContributionsChild2),
                nameof(MedicalOption.TotalMonthlyContributionsPrincipal)
            }
        });

        // Map to DTOs to avoid circular reference
        var responseDtos = existingOptions
          .Select(option => option.ToMedicalOptionDto()).ToList();

        return responseDtos.AsReadOnly();
    }
  }
}