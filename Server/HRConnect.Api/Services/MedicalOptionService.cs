namespace HRConnect.Api.Services
{
  using DTOs.MedicalOption;
  using Interfaces;
  using Mappers;
  using Models;
  using Utils.Enums;
  using Utils.MedicalOption;
  /// <summary>
  /// Service layer for managing medical options with business logic, validation orchestration, and data transformation.
  /// </summary>
  /// <remarks>
  /// Orchestrates complex operations between API controllers and repository, enforces business rules,
  /// and performs comprehensive validation. Uses cached HashSet for O(1) category restriction lookups.
  /// Salary bracket updates are blocked for Alliance and Double categories.
  /// </remarks>
  public class MedicalOptionService : IMedicalOptionService
  {
    private readonly IMedicalOptionRepository _medicalOptionRepository;
    /// <summary>
    /// Cached categories restricted from salary bracket updates (Alliance, Double).
    /// Provides O(1) lookup performance for validation checks.
    /// </summary>
    private static readonly HashSet<string> _restrictedPolicyCategoryUpdates = Enum
      .GetValues<NoUpdateOnMedicalOptionCategory>()
      .Select(e => e.ToString())
      .ToHashSet();
    /// <summary>
    /// Initializes a new instance of the service with repository dependency injection.
    /// </summary>
    /// <param name="medicalOptionRepository">The repository instance for data access.</param>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public MedicalOptionService(IMedicalOptionRepository medicalOptionRepository)
    {
      _medicalOptionRepository = medicalOptionRepository ??
                                 throw new ArgumentNullException(nameof(medicalOptionRepository));
    }
    /// <summary>
    /// Retrieves medical options grouped by category with eager-loaded category data.
    /// </summary>
    /// <returns>List of MedicalOptionCategoryDto with options grouped by category.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no options are found.</exception>
    public async Task<List<MedicalOptionCategoryDto>> GetGroupedMedicalOptionsAsync()
    {
      var groupedOptions = await _medicalOptionRepository
        .GetGroupedMedicalOptionsAsync();
      return groupedOptions
        .Select(group => group.ToMedicalOptionCategoryDto()).ToList();
    }
    /// <summary>
    /// Retrieves a medical option by ID.
    /// </summary>
    /// <param name="id">The medical option ID.</param>
    /// <returns>MedicalOptionDto if found.</returns>
    /// <exception cref="ArgumentException">Thrown when id is invalid (= 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the option is not found.</exception>
    public async Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id)
    {
      return await _medicalOptionRepository.GetMedicalOptionByIdAsync(id);
    }
    /// <summary>
    /// Retrieves the first medical option in a category.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>First MedicalOptionDto in the category.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (= 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when category has no options.</exception>
    public async Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int categoryId)
    {
      var options = await _medicalOptionRepository
        .GetAllOptionsUnderCategoryAsync(categoryId);
      return options.FirstOrDefault() ?? throw
        new KeyNotFoundException($"no medical option found for category {categoryId}");
    }
    /// <summary>
    /// Checks if a category exists.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>True if the category exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (= 0).</exception>
    public async Task<bool> MedicalOptionCategoryExistsAsync(int categoryId)
    {
      return await _medicalOptionRepository.MedicalOptionCategoryExistsAsync(categoryId);
    }
    /// <summary>
    /// Checks if an option exists.
    /// </summary>
    /// <param name="optionId">The option ID.</param>
    /// <returns>True if the option exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when optionId is invalid (= 0).</exception>
    public async Task<bool> MedicalOptionExistsAsync(int optionId)
    {
      return await _medicalOptionRepository.MedicalOptionExistsAsync(optionId);
    }
    /// <summary>
    /// Retrieves all options in a category. Returns empty list if category exists but has no options.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>List of MedicalOptionDto objects in the category.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (= 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the category does not exist.</exception>
    public async Task<List<MedicalOptionDto?>> GetAllOptionsUnderCategoryAsync(int categoryId)
    {
      return await _medicalOptionRepository.GetAllOptionsUnderCategoryAsync(categoryId);
    }
    /// <summary>
    /// Validates that an option exists within a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="optionId">The option ID.</param>
    /// <returns>True if the option belongs to the category; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId or optionId is invalid (= 0).</exception>
    public async Task<bool> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId)
    {
      return await _medicalOptionRepository
        .MedicalOptionExistsWithinCategoryAsync(categoryId, optionId);
    }
    /// <summary>
    /// Bulk updates medical options with comprehensive validation and business rule enforcement.
    /// </summary>
    /// <param name="categoryId">The category ID containing options to update.</param>
    /// <param name="bulkUpdateDto">Collection of updates with new values.</param>
    /// <param name="testDate">Optional date for testing; uses current date if null.</param>
    /// <returns>Read-only list of updated MedicalOptionDto objects.</returns>
    /// <remarks>Salary bracket updates blocked for Alliance and Double categories. All validation must pass before updates execute.</remarks>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (= 0) or bulkUpdateDto is null/empty.</exception>
    /// <exception cref="Middleware.ValidationException">Thrown when validation fails with detailed error information.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no options found for the category.</exception>
    public async Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateMedicalOptionsByCategoryAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto, DateTime? testDate = null)
    {
      if (categoryId <= 0)
      {
        throw new ArgumentException("Category ID must be greater than 0", nameof(categoryId));
      }
      if (bulkUpdateDto == null || bulkUpdateDto.Count == 0)
      {
        throw new ArgumentException("Bulk update data cannot be null or empty",
          nameof(bulkUpdateDto));
      }
      var dbData = await _medicalOptionRepository
        .GetAllOptionsUnderCategoryAsync(categoryId);
      var validationResult = await MedicalOptionValidator
        .ValidateAllCategoryVariantsComprehensiveAsync(
        categoryId, bulkUpdateDto, _medicalOptionRepository, dbData.Select(dto => new MedicalOption
        {
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
        var validationErrors = new Dictionary<string, string[]>();
        if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
        {
          validationErrors["Validation"] = new[] { validationResult.ErrorMessage };
        }
        throw new Middleware.ValidationException(
          validationResult.ErrorMessage ?? "Validation failed",
        validationErrors);
      }
      return await _medicalOptionRepository.BulkUpdateByCategoryIdAsync(categoryId, bulkUpdateDto);
    }
    /// <summary>
    /// Retrieves options eligible for an employee based on salary amount, grouped by category.
    /// </summary>
    /// <param name="salaryAmount">The employee's salary to match against salary brackets.</param>
    /// <returns>Options grouped by category where salary falls within the bracket.</returns>
    public async Task<IReadOnlyList<MedicalOptionCategoryDto>> GetAllOptionsWithinEmployeeSalary(
      decimal salaryAmount)
    {
      var employeeOptions =
        await _medicalOptionRepository.GetAllOptionsWithinEmployeeSalary(salaryAmount);
      return employeeOptions
        .Select(group => group.ToMedicalOptionCategoryDto()).ToList();
    }
    /// <summary>
    /// Retrieves options eligible for an employee based on enrolment period and salary. Not yet implemented.
    /// </summary>
    /// <param name="employeeId">The employee ID.</param>
    /// <returns>Eligible options grouped by category.</returns>
    public async Task<List<IGrouping<int, MedicalOptionDto>>> GetEmployeeEligibleOptions(string employeeId)
    {
      if (string.IsNullOrWhiteSpace(employeeId))
      {
        throw new ArgumentException("Employee ID cannot be null or empty", nameof(employeeId));
      }
      var eligibleOptions = await _medicalOptionRepository.GetEmployeeEligibleOptions(employeeId);
      if (eligibleOptions == null || eligibleOptions.Count == 0)
      {
        return new List<IGrouping<int, MedicalOptionDto>>();
      }
      return eligibleOptions;
    }
    /// <summary>
    /// Retrieves all options in a category, grouped by category ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>Options grouped by category.</returns>
    public async Task<List<MedicalOptionCategoryDto>> GetAllCategoryOptionsById(int id)
    {
      var groupedCategorySpecificOptions = await _medicalOptionRepository.GetAllCategoryOptionsById(id);
      return groupedCategorySpecificOptions
        .Select(group => group.ToMedicalOptionCategoryDto()).ToList();
    }
    /// <summary>
    /// Retrieves all medical option categories grouped by ID.
    /// </summary>
    /// <returns>Categories grouped by ID.</returns>
    public async Task<List<IGrouping<int, MedicalOptionCategory>>> GetAllMedicalOptionCategories()
    {
      return await _medicalOptionRepository.GetAllMedicalOptionCategories();
    }
    /// <summary>
    /// Retrieves a category by ID as a list (typically single result).
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>List of matching categories.</returns>
    public async Task<List<MedicalOptionCategoryOnlyDto>> GetCategoryById(int id)
    {
      var response = await _medicalOptionRepository.GetCategoryById(id);
      return response
        .Select(cat => cat.ToMedicalOptionCategoryOnlyDto()).ToList();
    }
    /// <summary>
    /// Retrieves a snapshot of all medical options from the database.
    /// </summary>
    /// <returns>Read-only list of all MedicalOptionDto objects.</returns>
    public async Task<IReadOnlyList<MedicalOptionDto>> GetCurrentDbCopy()
    {
      return await _medicalOptionRepository.GetCurrentDbCopy();
    }
    /// <summary>
    /// Creates a new medical option category. Not yet implemented.
    /// </summary>
    /// <param name="createCategoryPayload">Category details to create.</param>
    /// <returns>Created MedicalOptionCategoryDto.</returns>
    public async Task<MedicalOptionCategoryDto> CreateMedicalOptionCategory(CreateMedicalOptionCategoryDto createCategoryPayload)
    {
      ArgumentNullException.ThrowIfNull(createCategoryPayload);
      if (string.IsNullOrWhiteSpace(createCategoryPayload.MedicalOptionCategoryName))
      {
        throw new ArgumentException("Category name cannot be null or empty");
      }
      var createdCategory = await _medicalOptionRepository.CreateMedicalOptionCategory(createCategoryPayload);
      return createdCategory;
    }
        /// <summary>
        /// Bulk creates medical options in an existing category. Only allowed during update period (Nov-Dec).
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <param name="createOptionsPayload">Collection of options to create.</param>
        /// <param name="testDate">Optional date for testing; uses current date if null.</param>
        /// <returns>List of created CreateMedicalOptionVariantsDto objects.</returns>
        /// <exception cref="ArgumentException">Thrown when id is invalid (= 0), payload is null/empty, or category mismatch detected.</exception>
        /// <exception cref="InvalidOperationException">Thrown when category not found or outside update period.</exception>
        /// <exception cref="Middleware.ValidationException">Thrown when validation fails with detailed error information.</exception>
        public async Task<IReadOnlyList<CreateMedicalOptionVariantsDto>> CreateBulkOptionsByExistingCategoryId(
          int id,
          IReadOnlyCollection<CreateMedicalOptionVariantsDto> createOptionsPayload,
          DateTime? testDate = null)
        {
      if (id <= 0)
      {
        throw new ArgumentException("Category ID must be greater than 0", nameof(id));
      }
      if (createOptionsPayload == null || createOptionsPayload.Count == 0)
      {
        throw new ArgumentException("Bulk option insert payload cannot be null or empty", nameof(createOptionsPayload));
      }
      foreach (var dto in createOptionsPayload)
      {
        if (dto.MedicalOptionCategoryId != id)
        {
          throw new ArgumentException(
            $"Option '{dto.MedicalOptionName}' has incorrect category ID. Expected: {id}, Got: {dto.MedicalOptionCategoryId}");
        }
      }
      var categoryData = await _medicalOptionRepository.GetCategoryById(id);
      var categoryInfo = categoryData.FirstOrDefault();
      if (categoryInfo == null)
      {
        throw new InvalidOperationException($"Insert failed: Category with ID {id} does not exist");
      }
      var existingOptions = await _medicalOptionRepository.GetAllOptionsUnderCategoryAsync(id);
      var validationResult = await MedicalOptionValidator.ValidateBulkInsertAsync(
        id,
        createOptionsPayload,
        _medicalOptionRepository,
        categoryInfo,
        existingOptions,
        testDate ?? DateTime.Now);
      if (!validationResult.IsValid)
      {
        var validationErrors = new Dictionary<string, string[]>();
        if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
        {
          validationErrors["Validation"] = new[] { validationResult.ErrorMessage };
        }
        throw new Middleware.ValidationException(
          validationResult.ErrorMessage ?? "Validation failed",
          validationErrors);
      }
      return await _medicalOptionRepository.CreateBulkOptionsByExistingCategoryId(id, createOptionsPayload);
    }
    /// <summary>
    /// Updates an existing medical option category. Not yet implemented.
    /// </summary>
    /// <param name="id">The category ID to update.</param>
    /// <param name="updateCategoryPayload">Updated category details.</param>
    /// <returns>Updated MedicalOptionCategoryDto.</returns>
    public async Task<MedicalOptionCategoryDto> UpdateExistingCategoryById(int id, UpdateMedicalOptionCategoryDto updateCategoryPayload)
    {
      if (id <= 0)
      {
        throw new ArgumentException("Category ID must be greater than 0");
      }
      ArgumentNullException.ThrowIfNull(updateCategoryPayload);
      if (string.IsNullOrWhiteSpace(updateCategoryPayload.MedicalOptionCategoryName))
      {
        throw new ArgumentException("Category name cannot be null or empty");
      }
      var updatedCategory = await _medicalOptionRepository.UpdateExistingCategoryById(id, updateCategoryPayload);
      return updatedCategory;
    }
  }
}
