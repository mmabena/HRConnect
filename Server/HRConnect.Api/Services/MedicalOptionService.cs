namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Serialization;
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Utils.Enums;
  using HRConnect.Api.Utils.Factories;
  using Middleware;
  using Models.MedicalOptions.Records;
  using Utils.Enums.Mappers;
  using Utils.MedicalOptions;


  public class MedicalOptionService:IMedicalOptionService
  {
    // TODO: Implement methods
    // TODO: Document methods
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
    
    public MedicalOptionService(IMedicalOptionRepository medicalOptionRepository)
    {
      _medicalOptionRepository = medicalOptionRepository ??
                                 throw new ArgumentNullException(nameof(medicalOptionRepository));
    }

    /// <summary>
    /// Retrieves medical options grouped by their categories in a client-friendly DTO format.
    /// This method uses the repository to get grouped medical options and transforms them
    /// using the MedicalOptionMapper extension methods for clean seperation of concerns.
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
    ///         Console.WriteLine($"  -
    ///           {option.MedicalOptionName}: R{option.TotalMonthlyContributionsAdult}/month");
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
      var groupedOptions = await _medicalOptionRepository.GetGroupedMedicalOptionsAsync();

      return groupedOptions.Select(group => group.ToMedicalOptionCategoryDto()).ToList();
    }

    /// <summary>
    /// Retrieves a specific medical option by its ID.
    /// </summary>
    /// <param name="id">The medical option ID to retrieve.</param>
    /// <returns>The Medical option if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when the ID is invalid.</exception>
    public async Task<MedicalOption?> GetMedicalOptionByIdAsync(int id)
    {
      return await _medicalOptionRepository.GetMedicalOptionByIdAsync(id);
    }

    /// <summary>
    /// Retrieves a specific medical option category by its ID.
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>List of medical options</returns>
    /// <exception cref="ArgumentException">Thrown when category ID is invalid</exception>
    public async Task<MedicalOption?> GetMedicalOptionCategoryByIdAsync(int categoryId)
    {
      var options = await _medicalOptionRepository.GetAllOptionsUnderCategoryAsync(categoryId);

      return options.FirstOrDefault() ?? throw new KeyNotFoundException($"no medical option found for category {categoryId}");
    }

    /// <summary>
    /// Checks if a medical option category exists.
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>True if category exists</returns>
    /// <exception cref="ArgumentException">Thrown when category ID is invalid</exception>
    public async Task<bool> MedicalOptionCategoryExistsAsync(int categoryId)
    {
      return await _medicalOptionRepository.MedicalOptionCategoryExistsAsync(categoryId);
    }

    /// <summary>
    /// Checks if a medical option exists.
    /// </summary>
    /// <param name="optionId">The option ID</param>
    /// <returns>True if option exists</returns>
    /// <exception cref="ArgumentException">Thrown when option ID is invalid</exception>
    public async Task<bool> MedicalOptionExistsAsync(int optionId)
    {
      return await _medicalOptionRepository.MedicalOptionExistsAsync(optionId);
    }

    /// <summary>
    /// Gets all options under a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>List of medical options</returns>
    /// <exception cref="ArgumentException">Thrown when category ID is invalid</exception>
    public async Task<List<MedicalOption>> GetAllOptionsUnderCategoryAsync(int categoryId)
    {
      return await _medicalOptionRepository.GetAllOptionsUnderCategoryAsync(categoryId);
    }

    /// <summary>
    /// Checks if a medical option exists within a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <param name="optionId">The option ID</param>
    /// <returns>True if option exists in category</returns>
    /// <exception cref="ArgumentException">Thrown when IDs are invalid</exception>
    public async Task<bool> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId)
    {
      return await _medicalOptionRepository.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId);
    }

    /// <summary>
    /// Bulk update medical options by category with comprehensive validation.
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <param name="bulkUpdateDto">The bulk update data</param>
    /// <returns>List of updated medical options</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
    public async Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateMedicalOptionsByCategoryAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
    {
      // Validate input parameters
      if (categoryId <= 0)
      {
        throw new ArgumentException("Category ID must be greater than 0", nameof(categoryId));
      }

      if (bulkUpdateDto == null || bulkUpdateDto.Count == 0)
      {
        throw new ArgumentException("Bulk update data cannot be null or empty", nameof(bulkUpdateDto));
      }

      // Get existing data for validation
      var dbData = await _medicalOptionRepository.GetAllOptionsUnderCategoryAsync(categoryId);

      // Perform comprehensive validation using the existing validator
      var validationResult = await MedicalOptionValidator.ValidateAllCategoryVariantsComprehensiveAsync(
        categoryId, bulkUpdateDto, _medicalOptionRepository, dbData);

      if (!validationResult.IsValid)
      {
        // Create validation errors dictionary with the error message
        var validationErrors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
        {
          validationErrors["Validation"] = new[] { validationResult.ErrorMessage };
        }

        throw new ValidationException(
          validationResult.ErrorMessage ?? "Validation failed",
          validationErrors);
      }

      // If validation passes, proceed with bulk update
      return await _medicalOptionRepository.BulkUpdateByCategoryIdAsync(categoryId, bulkUpdateDto);
    }
  }
}