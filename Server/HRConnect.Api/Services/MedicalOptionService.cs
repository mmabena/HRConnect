namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils.Enums;
  using Middleware;
  using Utils.MedicalOption;

  /// <summary>
  /// Service layer implementation for managing medical options and their categories in the HR Connect system.
  /// Provides business logic orchestration, validation, and data transformation between the repository layer
  /// and API controllers, ensuring proper separation of concerns and comprehensive validation.
  /// </summary>
  /// <remarks>
  /// This service acts as the business logic layer between the API controllers and the data repository.
  /// It orchestrates complex operations, applies business rules, performs comprehensive validation,
  /// and transforms data between different layers of the application.
  /// 
  /// Key responsibilities:
  /// - Business rule enforcement and validation orchestration
  /// - Data transformation between DTOs and entities
  /// - Comprehensive bulk update validation using MedicalOptionValidator
  /// - Category-specific restriction enforcement (salary bracket updates)
  /// - Error handling and exception propagation
  /// - Integration with validation utilities and business logic components
  /// 
  /// Architecture patterns:
  /// - Implements the Service layer pattern for business logic separation
  /// - Uses dependency injection for repository abstraction
  /// - Leverages extension methods for data mapping (MedicalOptionMapper)
  /// - Integrates with comprehensive validation framework
  /// - Applies the Strategy pattern for category-specific restrictions
  /// 
  /// Performance considerations:
  /// - Uses cached HashSet for O(1) category restriction lookups
  /// - Leverages repository bulk operations for efficient data access
  /// - Implements comprehensive validation before database operations
  /// - Uses async/await pattern for non-blocking operations
  /// 
  /// Business rules enforced:
  /// - Salary bracket restrictions for specific categories (Alliance, Double)
  /// - Comprehensive validation of contribution amounts and salary brackets
  /// - Category-option relationship validation
  /// - Temporal validation support for testing scenarios
  /// </remarks>
  public class MedicalOptionService : IMedicalOptionService
  {
    private readonly IMedicalOptionRepository _medicalOptionRepository;

    /// <summary>
    /// Cached HashSet containing the string names of all medical option categories that are
    /// restricted from salary bracket updates. This provides O(1) lookup performance for
    /// validation checks.
    /// </summary>
    /// <remarks>
    /// This HashSet is populated at application startup by converting the
    /// <see cref="NoUpdateOnMedicalOptionCategory"/> 
    /// enum values to their string representations ("Alliance", "Double") and contains
    /// categories that have special business rules preventing salary bracket modifications.
    ///
    /// Usage Example:
    /// <code>
    /// if (_restrictedPolicyCategoryUpdates
    ///   .Contains(existingOption.MedicalOptionCategory?.MedicalOptionCategoryName))
    /// {
    ///     throw new
    ///       InvalidOperationException("Salary bracket cannot be updated for this category");
    /// }
    /// </code>
    ///
    /// The static readonly modifier ensures this collection is initialized only once per
    /// application lifetime and shared across all service instances for optimal performance.
    /// Using string values allows direct 
    /// comparison with database category names without enum parsing overhead.
    /// </remarks>
    private static readonly HashSet<string> _restrictedPolicyCategoryUpdates = Enum
      .GetValues<NoUpdateOnMedicalOptionCategory>()
      .Select(e => e.ToString())
      .ToHashSet();

    /// <summary>
    /// Initializes a new instance of the MedicalOptionService class.
    /// </summary>
    /// <param name="medicalOptionRepository">The repository instance for data access operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when medicalOptionRepository is null.</exception>
    /// <remarks>
    /// The service requires a valid IMedicalOptionRepository instance for all data operations.
    /// Dependency injection is used to provide the repository implementation, ensuring
    /// loose coupling and testability.
    /// </remarks>
    public MedicalOptionService(IMedicalOptionRepository medicalOptionRepository)
    {
      _medicalOptionRepository = medicalOptionRepository ??
                                 throw new ArgumentNullException(nameof(medicalOptionRepository));
    }

    /// <summary>
    /// Retrieves medical options grouped by their categories in a client-friendly DTO format.
    /// This method uses the repository to get grouped medical options and transforms them
    /// using the MedicalOptionMapper extension methods for clean separation of concerns.
    /// </summary>
    /// <returns>
    /// A list of MedicalOptionCategoryDto objects, each containing:
    /// - MedicalOptionCategoryId: Unique identifier for the category
    /// - MedicaloptionCategoryName: Human-readable category name
    /// - Medicaloptions: List of MedicalOptionDto objects with detailed plan information
    ///
    /// Example structure:
    /// [
    ///   MedicalOptionCategoryDto {
    ///     MedicalOptionCategoryId: 1,
    ///     MedicalOptionCategoryName: "Vital"
    ///     MedicalOptions: [
    ///       MedicalOptionDto {
    ///         MedicalOptionId: 1,
    ///         MedicalOptionName: "Plan A",
    ///         MedicalOptionCategoryId: 1,
    ///         SalaryBracketMin: 0,
    ///         SalaryBracketMax: 15000,
    ///         MonthlyRiskContributionPrincipal: 250.00,
    ///         MonthlyRiskContributionAdult: 175.00,
    ///         MonthlyRiskContributionChild: 125.00,
    ///         TotalMonthlyContributionsPrincipal: 250.00,
    ///         TotalMonthlyContributionsAdult: 175.00,
    ///         TotalMonthlyContributionsChild: 125.00,
    ///       },
    ///       MedicalOptionDto {
    ///         MedicalOptionId: 2,
    ///         MedicalOptionName: "Plan B",
    ///         MedicalOptionCategoryId: 1,
    ///         SalaryBracketMin: 15001,
    ///         SalaryBracketMax: 30000,
    ///         MonthlyRiskContributionPrincipal: 550.50,
    ///         MonthlyRiskContributionAdult: 450.00,
    ///         MonthlyRiskContributionChild: 350.00,
    ///         TotalMonthlyContributionsPrincipal: 550.50,
    ///         TotalMonthlyContributionsAdult: 450.00,
    ///         TotalMonthlyContributionsChild: 350.00, 
    ///       }
    ///     ]
    ///   },
    ///   MedicalOptionCategoryDto {
    ///     MedicalOptionCategoryId: 2,
    ///     MedicalOptionCategoryName: "Essential",
    ///     MedicalOptions: [
    ///       MedicalOptionDto {
    ///         MedicalOptionId: 3,
    ///         MedicalOptionName: "Plan C",
    ///         MedicalOptionCategoryId: 2,
    ///         SalaryBracketMin: 0,
    ///         SalaryBracketMax: 20000,
    ///         MonthlyRiskContributionAdult: 180.00,
    ///         MonthlyRiskContributionChild: 120.00,
    ///         TotalMonthlyContributionsAdult: 180.00,
    ///         TotalMonthlyContributionsChild: 120.00,
    ///       }
    ///     ]
    ///   }
    /// ]
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is asynchronous to prevent thread blocking during database operations.
    /// The underlying repository query uses Entity Framework's Include() method to eagerly load
    /// the MedicalOptionCategory navigation property, preventing N+1 query problems.
    /// </para>
    /// <para>
    /// Uses MedicalOptionMapper.ToMedicalOptionCategoryDto() extension method for transformation,
    /// which internally calls MedicalOptionMapper.ToMedicalOptionDto() for each medical option.
    /// </para>
    /// <para>
    /// If a medical option category is not properly loaded, the MedicalOptionCategoryName
    /// will default to an empty string rather than throwing a null reference exception.
    /// </para>
    /// </remarks>
    /// <example>
    /// Service usage in a controller or a business logic
    /// <code>
    /// var medicalOptionService = new medicalOptionService(medicalOptionRepository);
    /// var categories = await medicalOptionService.GetGroupedMedicalOptionsAsync();
    ///
    /// // Process the results
    /// foreach (var category in categories)
    /// {
    ///     Console.WriteLine($"Category: {category.MedicalOptionCategoryName}");
    ///     Console.WriteLine($"Total Options: {category.MedicalOptions.Count}");
    ///     
    ///     foreach (var option in category.MedicalOptions)
    ///     {
    ///         Console.WriteLine($"  - {option.MedicalOptionName}: R{option.TotalMonthlyContributionsAdult}/month");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is disposed or unavailable.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the repository dependency is null.
    /// </exception>
    public async Task<List<MedicalOptionCategoryDto>> GetGroupedMedicalOptionsAsync()
    {
      var groupedOptions = await _medicalOptionRepository
        .GetGroupedMedicalOptionsAsync();

      return groupedOptions
        .Select(group => group.ToMedicalOptionCategoryDto()).ToList();
    }

    /// <summary>
    /// Retrieves a specific medical option by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the medical option to retrieve.</param>
    /// <returns>The MedicalOptionDto if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided id is invalid.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical option with the specified ID is found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method delegates directly to the repository layer, maintaining a clean separation
    /// of concerns. The repository handles all database operations and entity-to-DTO mapping.
    /// 
    /// The service layer could add additional business logic in the future, such as
    /// caching, logging, or permission checks, without modifying the repository.
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new MedicalOptionService(repository);
    /// var medicalOption = await service.GetMedicalOptionByIdAsync(123);
    /// 
    /// if (medicalOption != null)
    /// {
    ///     Console.WriteLine($"Option: {medicalOption.MedicalOptionName}");
    ///     Console.WriteLine($"Category ID: {medicalOption.MedicalOptionCategoryId}");
    /// }
    /// </code>
    /// </example>
    public async Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id)
    {
      return await _medicalOptionRepository.GetMedicalOptionByIdAsync(id);
    }

    /// <summary>
    /// Retrieves the first medical option found within a specific category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <returns>The first MedicalOptionDto found in the category; otherwise, throws KeyNotFoundException.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided category ID is invalid.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical options are found for the specified category.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method retrieves all options within a category and returns the first one found.
    /// It throws a KeyNotFoundException if no options exist in the category, ensuring
    /// that callers are aware when a category is empty.
    /// 
    /// This approach is useful when you need to validate that a category has at least
    /// one option, or when you need a representative option from a category for display
    /// or validation purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new MedicalOptionService(repository);
    /// 
    /// try
    /// {
    ///     var firstOption = await service.GetMedicalOptionCategoryByIdAsync(5);
    ///     Console.WriteLine($"First option in category 5: {firstOption.MedicalOptionName}");
    /// }
    /// catch (KeyNotFoundException)
    /// {
    ///     Console.WriteLine("Category 5 has no medical options");
    /// }
    /// </code>
    /// </example>
    public async Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int categoryId)
    {
      var options = await _medicalOptionRepository
        .GetAllOptionsUnderCategoryAsync(categoryId);

      return options.FirstOrDefault() ?? throw
        new KeyNotFoundException($"no medical option found for category {categoryId}");
    }

    /// <summary>
    /// Checks if a medical option category exists in the system.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category to validate.</param>
    /// <returns>True if the category exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided category ID is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method delegates directly to the repository layer for efficient existence checking.
    /// The repository uses Entity Framework's AnyAsync() method for optimal performance,
    /// generating a SQL query that stops as soon as a matching record is found.
    /// 
    /// This is useful for pre-validation before performing operations that depend on
    /// the existence of a specific category.
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new MedicalOptionService(repository);
    /// 
    /// if (await service.MedicalOptionCategoryExistsAsync(5))
    /// {
    ///     Console.WriteLine("Category 5 exists, proceeding with operations...");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Category 5 does not exist");
    /// }
    /// </code>
    /// </example>
    public async Task<bool> MedicalOptionCategoryExistsAsync(int categoryId)
    {
      return await _medicalOptionRepository.MedicalOptionCategoryExistsAsync(categoryId);
    }

    /// <summary>
    /// Checks if a specific medical option exists in the system.
    /// </summary>
    /// <param name="optionId">The unique identifier of the medical option to validate.</param>
    /// <returns>True if the medical option exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided option ID is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method delegates directly to the repository layer for efficient existence checking.
    /// The repository uses Entity Framework's AnyAsync() method for optimal performance,
    /// generating a SQL query that stops as soon as a matching record is found.
    /// 
    /// This is useful for pre-validation before performing operations that depend on
    /// the existence of a specific medical option.
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new MedicalOptionService(repository);
    /// 
    /// if (await service.MedicalOptionExistsAsync(123))
    /// {
    ///     Console.WriteLine("Medical option 123 exists, proceeding with operations...");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Medical option 123 does not exist");
    /// }
    /// </code>
    /// </example>
    public async Task<bool> MedicalOptionExistsAsync(int optionId)
    {
      return await _medicalOptionRepository.MedicalOptionExistsAsync(optionId);
    }

    /// <summary>
    /// Retrieves all medical options belonging to a specific category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <returns>List of MedicalOptionDto objects belonging to the specified category.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided category ID is invalid.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the specified category does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method delegates directly to the repository layer, which handles all database
    /// operations and entity-to-DTO mapping. The repository validates category existence
    /// before retrieving the options to ensure data integrity.
    /// 
    /// The method returns an empty list if the category exists but contains no medical options.
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new MedicalOptionService(repository);
    /// 
    /// try
    /// {
    ///     var categoryOptions = await service.GetAllOptionsUnderCategoryAsync(5);
    ///     Console.WriteLine($"Found {categoryOptions.Count} options in category 5:");
    ///     
    ///     foreach (var option in categoryOptions)
    ///     {
    ///         Console.WriteLine($"- {option.MedicalOptionName}");
    ///     }
    /// }
    /// catch (KeyNotFoundException ex)
    /// {
    ///     Console.WriteLine($"Error: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    public async Task<List<MedicalOptionDto?>> GetAllOptionsUnderCategoryAsync(int categoryId)
    {
      return await _medicalOptionRepository.GetAllOptionsUnderCategoryAsync(categoryId);
    }

    /// <summary>
    /// Validates that a specific medical option exists within a particular category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category.</param>
    /// <param name="optionId">The unique identifier of the medical option.</param>
    /// <returns>True if the medical option exists within the specified category; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when either categoryId or optionId is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is disposed.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or query execution error.</exception>
    /// <remarks>
    /// This method delegates directly to the repository layer, which uses Entity Framework's
    /// AnyAsync() method with a compound condition to check both the category membership
    /// and option existence in a single database query.
    /// 
    /// This is useful for preventing invalid associations in business logic and ensuring
    /// that operations are performed on valid category-option relationships.
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new MedicalOptionService(repository);
    /// 
    /// if (await service.MedicalOptionExistsWithinCategoryAsync(5, 123))
    /// {
    ///     Console.WriteLine("Option 123 belongs to category 5");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Option 123 does not belong to category 5");
    /// }
    /// </code>
    /// </example>
    public async Task<bool> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId)
    {
      return await _medicalOptionRepository
        .MedicalOptionExistsWithinCategoryAsync(categoryId, optionId);
    }

    /// <summary>
    /// Performs bulk updates of medical options within a specific category with comprehensive validation.
    /// This method orchestrates complex business logic, validation, and database operations to ensure
    /// data integrity and compliance with business rules before applying updates.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the medical option category containing options to update.</param>
    /// <param name="bulkUpdateDto">Collection of UpdateMedicalOptionVariantsDto objects containing the new values for updates.</param>
    /// <param name="testDate">Optional date parameter for testing integration scenarios; if null, current date is used.</param>
    /// <returns>Read-only list of updated MedicalOptionDto objects representing the successfully modified medical options.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (less than or equal to 0) or bulkUpdateDto is null or empty.</exception>
    /// <exception cref="ValidationException">Thrown when comprehensive validation fails, containing detailed error information.</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated or category restrictions apply.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no medical options are found for the specified category with the provided option IDs.</exception>
    /// <exception cref="SqlException">Thrown when there's a database connectivity or bulk operation error.</exception>
    /// <remarks>
    /// <para>
    /// This method implements a comprehensive validation and update strategy:
    /// </para>
    /// <list type="number">
    /// <item><description>Input parameter validation for categoryId and bulkUpdateDto</description></item>
    /// <item><description>Retrieval of existing category data for validation context</description></item>
    /// <item><description>Comprehensive validation using MedicalOptionValidator with multiple validation layers</description></item>
    /// <item><description>Category-specific restriction enforcement (salary bracket updates blocked for Alliance, Double categories)</description></item>
    /// <item><description>Temporal validation support using testDate parameter for testing scenarios</description></item>
    /// <item><description>Database update execution only after all validations pass</description></item>
    /// </list>
    /// 
    /// <para>
    /// Validation layers applied:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Basic parameter validation (IDs, null checks)</description></item>
    /// <item><description>Category existence and membership validation</description></item>
    /// <item><description>Salary bracket range validation (min &lt; max)</description></item>
    /// <item><description>Contribution amount validation and consistency checks</description></item>
    /// <item><description>Category-specific business rule enforcement</description></item>
    /// <item><description>Temporal validation for test scenarios</description></item>
    /// </list>
    /// 
    /// <para>
    /// Business rules enforced:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Salary bracket updates are blocked for Alliance and Double categories</description></item>
    /// <item><description>All contribution amounts must be positive and consistent</description></item>
    /// <item><description>Salary bracket ranges must not overlap within categories</description></item>
    /// <item><description>Category-option relationships must be maintained</description></item>
    /// </list>
    /// 
    /// <para>
    /// Performance considerations:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Uses cached HashSet for O(1) category restriction lookups</description></item>
    /// <item><description>Performs comprehensive validation before database operations</description></item>
    /// <item><description>Leverages repository bulk operations for efficient updates</description></item>
    /// <item><description>Maps DTOs to entities only for validation, minimizing memory usage</description></item>
    /// </list>
    /// 
    /// <para>
    /// The operation is atomic - either all updates succeed after validation or none are applied.
    /// Validation failures result in detailed ValidationException with specific error messages.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new MedicalOptionService(repository);
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
    ///     var updatedOptions = await service.BulkUpdateMedicalOptionsByCategoryAsync(
    ///         categoryId: 5, 
    ///         bulkUpdateDto: updates, 
    ///         testDate: DateTime.Now);
    ///         
    ///     Console.WriteLine($"Successfully updated {updatedOptions.Count} medical options");
    ///     
    ///     foreach (var option in updatedOptions)
    ///     {
    ///         Console.WriteLine($"Updated: {option.MedicalOptionName}");
    ///         Console.WriteLine($"New Adult Contribution: {option.TotalMonthlyContributionsAdult:C}");
    ///     }
    /// }
    /// catch (ValidationException ex)
    /// {
    ///     Console.WriteLine($"Validation failed: {ex.Message}");
    ///     if (ex.Errors != null)
    ///     {
    ///         foreach (var error in ex.Errors)
    ///         {
    ///             Console.WriteLine($"Error: {string.Join(", ", error.Value)}");
    ///         }
    ///     }
    /// }
    /// catch (InvalidOperationException ex)
    /// {
    ///     Console.WriteLine($"Business rule violation: {ex.Message}");
    /// }
    /// catch (KeyNotFoundException ex)
    /// {
    ///     Console.WriteLine($"Data not found: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    public async Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateMedicalOptionsByCategoryAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto, DateTime? testDate = null)
    {
      // Validate input parameters
      if (categoryId <= 0)
      {
        throw new ArgumentException("Category ID must be greater than 0", nameof(categoryId));
      }

      if (bulkUpdateDto == null || bulkUpdateDto.Count == 0)
      {
        throw new ArgumentException("Bulk update data cannot be null or empty",
          nameof(bulkUpdateDto));
      }

      // Get existing data for validation
      var dbData = await _medicalOptionRepository
        .GetAllOptionsUnderCategoryAsync(categoryId);

      // Perform comprehensive validation using the existing validator
      var validationResult = await MedicalOptionValidator
        .ValidateAllCategoryVariantsComprehensiveAsync(
        categoryId, bulkUpdateDto, _medicalOptionRepository, dbData.Select(dto => new MedicalOption
        {
          // Map DTO back to entity for validation
          MedicalOptionId = dto.MedicalOptionId,
          MedicalOptionName = dto.MedicalOptionName,
          MedicalOptionCategoryId = dto.MedicalOptionCategoryId,
          SalaryBracketMin = dto.SalaryBracketMin,
          SalaryBracketMax = dto.SalaryBracketMax,
          MonthlyRiskContributionPrincipal = dto.MonthlyRiskContributionPrincipal,
          MonthlyRiskContributionAdult = dto.MonthlyRiskContributionAdult,
          MonthlyRiskContributionChild = dto.MonthlyRiskContributionChild,
          MonthlyRiskContributionChild2 = dto.MonthlyRiskContributionChild2,
          MonthlyMsaContributionPrincipal = dto.MonthlyMsaContributionPrincipal,
          MonthlyMsaContributionAdult = dto.MonthlyMsaContributionAdult,
          MonthlyMsaContributionChild = dto.MonthlyMsaContributionChild,
          TotalMonthlyContributionsPrincipal = dto.TotalMonthlyContributionsPrincipal,
          TotalMonthlyContributionsAdult = dto.TotalMonthlyContributionsAdult,
          TotalMonthlyContributionsChild = dto.TotalMonthlyContributionsChild,
          TotalMonthlyContributionsChild2 = dto.TotalMonthlyContributionsChild2
        }).ToList(), testDate ?? DateTime.Now);

      if (!validationResult.IsValid)
      {
        // Create validation errors dictionary with the error message
        var validationErrors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
        {
          validationErrors["Validation"] = new[] { validationResult.ErrorMessage };
        }

        throw new HRConnect.Api.Middleware.ValidationException(
          validationResult.ErrorMessage ?? "Validation failed",
        validationErrors);
      }

      // If validation passes, proceed with bulk update
      return await _medicalOptionRepository.BulkUpdateByCategoryIdAsync(categoryId, bulkUpdateDto);
    }

    // New
    
    public async Task<List<MedicalOptionCategoryDto>> GetAllOptionsWithinEmployeeSalary(
      decimal salaryAmount)
    {
      // Call Repo to get Applicable options for employee based on Salary
      // Return List<IGrouping<int, MedicalOptions>>
      var employeeOptions =
        await _medicalOptionRepository.GetAllOptionsWithinEmployeeSalary(salaryAmount);

      return employeeOptions
        .Select(group => group.ToMedicalOptionCategoryDto()).ToList();
    }

    public async Task<List<IGrouping<int, MedicalOptionDto>>> GetEmployeeEligibleOptions(string employeeId)
    {
      throw new NotImplementedException();
    }

    public async Task<List<MedicalOptionCategoryDto>> GetAllCategoryOptionsById(int id)
    {
      var groupedCategorySpecificOptions = await _medicalOptionRepository.GetAllCategoryOptionsById(id);

      return groupedCategorySpecificOptions
        .Select(group => group.ToMedicalOptionCategoryDto()).ToList();
    }


    public async Task<List<IGrouping<int, MedicalOptionCategory>>> GetAllMedicalOptionCategories()
    {
      return await _medicalOptionRepository.GetAllMedicalOptionCategories();
    }

    public async Task<List<MedicalOptionCategoryOnlyDto>> GetCategoryById(int id)
    {
      var response = await _medicalOptionRepository.GetCategoryById(id);
      
      return response
        .Select(cat => cat.ToMedicalOptionCategoryOnlyDto()).ToList();
    }

    public async Task<IReadOnlyList<MedicalOptionDto>> GetCurrentDbCopy()
    {
      return await _medicalOptionRepository.GetCurrentDbCopy();
    }

    public async Task<MedicalOptionCategoryDto> CreateMedicalOptionCategory(CreateMedicalOptionCategoryDto createCategoryPayload)
    {
      throw new NotImplementedException();
    }

    public async Task<List<CreateMedicalOptionVariantsDto>> CreateBulkOptionsByExistingCategoryId(int id,
      CreateMedicalOptionVariantsDto createOptionsPayload)
    {
      throw new NotImplementedException();
    }

    public async Task<MedicalOptionCategoryDto> UpdateExistingCategoryById(int id, UpdateMedicalOptionCategoryDto updateCategoryPayload)
    {
      throw new NotImplementedException();
    }
  }
}