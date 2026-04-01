namespace HRConnect.Api.Repository
{
  using Data;
  using DTOs.MedicalOption;
  using EFCore.BulkExtensions;
  using Interfaces;
  using Mappers;
  using Microsoft.Data.SqlClient;
  using Microsoft.EntityFrameworkCore;
  using Models;
  using Utils.MedicalOption;

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
  public class MedicalOptionRepository : IMedicalOptionRepository
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
    /// Retrieves medical options grouped by category ID with eager-loaded category data.
    /// </summary>
    /// <returns>List of IGrouping&lt;int, MedicalOption&gt; grouped by MedicalOptionCategoryId.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no medical options are found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database context is unavailable.</exception>
    public async Task<List<IGrouping<int, MedicalOption>>> GetGroupedMedicalOptionsAsync()
    {
      var groupedMedicalOptions = await _context.MedicalOptions
        .Include(mo => mo.MedicalOptionCategory)
        .Where(mo =>
          mo.MedicalOptionCategory !=
          null)
        .GroupBy(mo => mo.MedicalOptionCategoryId)
        .ToListAsync();

      if (groupedMedicalOptions.Count == 0)
      {
        throw new KeyNotFoundException("No medical options found in the database");
      }

      return groupedMedicalOptions;
    }

    /// <summary>
    /// Retrieves a medical option by ID with its category data.
    /// </summary>
    /// <param name="id">The medical option ID.</param>
    /// <returns>MedicalOptionDto if found.</returns>
    /// <exception cref="ArgumentException">Thrown when id is invalid (≤ 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the option is not found.</exception>
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
    /// Retrieves the first medical option found in a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>MedicalOptionDto for the first option in the category.</returns>
    /// <remarks>Returns only the first matching option. Use GetAllOptionsUnderCategoryAsync() for all options in a category.</remarks>
    /// <exception cref="ArgumentException">Thrown when id is invalid (≤ 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no options are found for the category.</exception>
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
    /// Retrieves multiple medical options by IDs in a single query.
    /// </summary>
    /// <param name="ids">List of medical option IDs to retrieve.</param>
    /// <returns>List of MedicalOptionDto objects for the provided IDs.</returns>
    /// <exception cref="ArgumentException">Thrown when ids is null or empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no options are found for the provided IDs.</exception>
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
    /// Retrieves a category entity by ID. Returns the raw entity, not a DTO.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>MedicalOptionCategory entity if found.</returns>
    /// <exception cref="ArgumentException">Thrown when id is invalid (≤ 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the category is not found.</exception>
    public async Task<MedicalOptionCategory?> GetCategoryByIdAsync(int id)
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
    /// Retrieves options by name pattern matching across all categories.
    /// </summary>
    /// <param name="optionName">The option name pattern to search for (case-sensitive partial match).</param>
    /// <returns>List of MedicalOptionDto objects matching the pattern.</returns>
    /// <exception cref="ArgumentException">Thrown when optionName is null, empty, or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no options match the pattern.</exception>
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

      if (medicalOptions.Count == 0) // move to service layer where used
      {
        throw new KeyNotFoundException($"No medical options found containing " +
                                       $"'{optionName}'");
      }

      return medicalOptions.Select(mo => mo?.ToMedicalOptionDto()).ToList();
    }

    /// <summary>
    /// Checks if a category exists.
    /// </summary>
    /// <param name="categoryId">The category ID to validate.</param>
    /// <returns>True if the category exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (≤ 0).</exception>
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
    /// Checks if an option exists.
    /// </summary>
    /// <param name="optionId">The medical option ID to validate.</param>
    /// <returns>True if the option exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when optionId is invalid (≤ 0).</exception>
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
    /// Retrieves all options in a category. Returns empty list if category exists but has no options.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>List of MedicalOptionDto objects in the category.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (≤ 0).</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the category does not exist.</exception>
    public async Task<List<MedicalOptionDto?>> GetAllOptionsUnderCategoryAsync(int categoryId)
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

      return allOptions.Select(mo => mo?.ToMedicalOptionDto()).ToList();
    }

    /// <summary>
    /// Validates that an option exists within a specific category.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="optionId">The option ID to validate.</param>
    /// <returns>True if the option belongs to the category; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when categoryId or optionId is invalid (≤ 0).</exception>
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
    /// Bulk updates medical options in a category with salary bracket validation.
    /// </summary>
    /// <param name="categoryId">The category ID containing options to update.</param>
    /// <param name="bulkUpdateDto">Collection of UpdateMedicalOptionVariantsDto with new values.</param>
    /// <returns>Read-only list of updated MedicalOptionDto objects.</returns>
    /// <remarks>Validates salary bracket ranges (min &lt; max). Uses EFCore.BulkExtensions for efficiency.</remarks>
    /// <exception cref="ArgumentException">Thrown when categoryId is invalid (≤ 0), bulkUpdateDto is null/empty, or salary brackets are invalid.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no options are found for the category with provided IDs.</exception>
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

    /// <summary>
    /// Retrieves options eligible for an employee based on salary amount grouped by category.
    /// </summary>
    /// <param name="salaryAmount">The employee's salary to match against salary brackets.</param>
    /// <returns>Options grouped by MedicalOptionCategoryId where salary falls within the bracket.</returns>
    public async Task<List<IGrouping<int, MedicalOption>>> GetAllOptionsWithinEmployeeSalary(
      decimal salaryAmount)
    {
      return await _context.MedicalOptions
        .Include(o => o.MedicalOptionCategory)
        .Where(o => o.SalaryBracketMin != null)
        .Where(o =>
          salaryAmount >= o.SalaryBracketMin &&
          (!o.SalaryBracketMax.HasValue || salaryAmount <= o.SalaryBracketMax))
        .GroupBy(o => o.MedicalOptionCategory.MedicalOptionCategoryId)
        .ToListAsync();
    }

    /// <summary>
    /// Retrieves medical options eligible for an employee. Depends on employee salary and enrolment period.
    /// </summary>
    /// <param name="employeeId">The employee ID.</param>
    /// <returns>Eligible options grouped by category.</returns>
    public async Task<List<IGrouping<int, MedicalOptionDto>>> GetEmployeeEligibleOptions(
      string employeeId)
    {
      // Get employee salary from employee service via context or join
      // For now, we get the employee's current salary bracket and return options they qualify for
      var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

      if (employee == null)
      {
        throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
      }

      var employeeSalary = employee.MonthlySalary;

      // Get all medical options that fall within the employee's salary bracket
      var eligibleOptions = await _context.MedicalOptions
        .Include(o => o.MedicalOptionCategory)
        .Where(o => o.SalaryBracketMin <= employeeSalary &&
                    (o.SalaryBracketMax == null || o.SalaryBracketMax >= employeeSalary))
        .Select(o => o.ToMedicalOptionDto())
        .ToListAsync();

      // Group by category ID
      return [.. eligibleOptions.GroupBy(o => o.MedicalOptionCategoryId)];
    }

    /// <summary>
    /// Retrieves all options in a category, grouped by category ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>Options grouped by MedicalOptionCategoryId.</returns>
    public async Task<List<IGrouping<int, MedicalOption>>> GetAllCategoryOptionsById(int id)
    {
      return await _context.MedicalOptions
        .Include(c => c.MedicalOptionCategory)
        .Where(c => c.MedicalOptionCategoryId == id)
        .GroupBy(c => c.MedicalOptionCategoryId)
        .ToListAsync();
    }

    /// <summary>
    /// Retrieves all medical option categories grouped by ID.
    /// </summary>
    /// <returns>Categories grouped by MedicalOptionCategoryId.</returns>
    public async Task<List<IGrouping<int, MedicalOptionCategory>>> GetAllMedicalOptionCategories()
    {
      var categories = await _context.MedicalOptionCategories.ToListAsync();
      
      if (categories == null || categories.Count == 0)
      {
        return new List<IGrouping<int, MedicalOptionCategory>>();
      }
      
      return categories.GroupBy(c => c.MedicalOptionCategoryId).ToList();
    }

    /// <summary>
    /// Retrieves categories by ID as a list (typically single result).
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>List of matching categories.</returns>
    public async Task<List<MedicalOptionCategory>> GetCategoryById(int id)
    {
      return await _context.MedicalOptionCategories
        .Where(c => c.MedicalOptionCategoryId == id)
        .ToListAsync();
    }

    /// <summary>
    /// Retrieves a snapshot of all medical options from the database.
    /// </summary>
    /// <returns>Read-only list of all MedicalOptionDto objects.</returns>
    public async Task<IReadOnlyList<MedicalOptionDto>> GetCurrentDbCopy()
    {
      var currentDbState = await _context.MedicalOptions
        .Include(opt => opt.MedicalOptionCategory)
        .ToListAsync();

      var response = currentDbState
        .Select(options => options.ToMedicalOptionDto()).ToList();

      return response.AsReadOnly();
    }

    /// <summary>
    /// Creates a new medical option category.
    /// </summary>
    /// <param name="createCategoryPayload">Category details to create.</param>
    /// <returns>Created MedicalOptionCategoryDto.</returns>
    public async Task<MedicalOptionCategoryDto> CreateMedicalOptionCategory(
      CreateMedicalOptionCategoryDto createCategoryPayload)
    {
      ArgumentNullException.ThrowIfNull(createCategoryPayload);

      // Check if category name already exists
      var existingCategory = await _context.MedicalOptionCategories
        .FirstOrDefaultAsync(c => c.MedicalOptionCategoryName == createCategoryPayload.MedicalOptionCategoryName);

      if (existingCategory != null)
      {
        throw new InvalidOperationException($"Medical option category with name '{createCategoryPayload.MedicalOptionCategoryName}' already exists");
      }

      // Create new category entity
      var newCategory = new MedicalOptionCategory
      {
        MedicalOptionCategoryName = createCategoryPayload.MedicalOptionCategoryName
      };

      await _context.MedicalOptionCategories.AddAsync(newCategory);
      await _context.SaveChangesAsync();

      // Map to DTO and return
      return new MedicalOptionCategoryDto
      {
        MedicalOptionCategoryId = newCategory.MedicalOptionCategoryId,
        MedicalOptionCategoryName = newCategory.MedicalOptionCategoryName,
        MedicalOptions = new List<MedicalOptionDto>()
      };
    }

    /// <summary>
    /// Bulk creates medical options in an existing category. Only allowed during update period (Nov-Dec).
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="createOptionsPayload">Collection of options to create.</param>
    /// <returns>List of created CreateMedicalOptionVariantsDto objects.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when category is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when outside the update period or duplicate names exist.</exception>
    public async Task<List<CreateMedicalOptionVariantsDto>> CreateBulkOptionsByExistingCategoryId(
      int id,
      IReadOnlyCollection<CreateMedicalOptionVariantsDto> createOptionsPayload)
    {
      // Validate category Exists
      var categoryExists = await MedicalOptionCategoryExistsAsync(id);
    
      if (!categoryExists)
        throw new KeyNotFoundException($"Medical option category with ID {id} not found");
      
      //2. Validate Update Period (as per the business rules)
      if (!MedicalOptionValidator.ValidateUpdatePeriod(DateTime.Now))
        throw new InvalidOperationException(
          "bulk create operations are only allowed during the update period (November-December)");
      
      //3. Get existing options in category to check for duplicates
      var existingOptions = await _context.MedicalOptions
        .Where(opt => opt.MedicalOptionCategoryId == id)
        .ToListAsync();
    
      var existingOptionNames = existingOptions.Select(o => o.MedicalOptionName).ToHashSet();
    
      //4. validate payload and check for duplicates
      ArgumentNullException.ThrowIfNull(createOptionsPayload);
      
      //5. Create list to hold all new MedicalOption entities
      var newMedicalOptions = new List<MedicalOption>();
      
      foreach (var optionDto in createOptionsPayload)
      {
        // Check if option name already exists in category
        if (existingOptionNames.Contains(optionDto.MedicalOptionName))
          throw new InvalidOperationException(
            $"Medical option '{optionDto.MedicalOptionName}' already exists in this category");
        
        // Create new entity from DTO
        var newMedicalOption = new MedicalOption
        {
          MedicalOptionName = optionDto.MedicalOptionName,
          MedicalOptionCategoryId = id,
          SalaryBracketMin = optionDto.SalaryBracketMin,
          SalaryBracketMax = optionDto.SalaryBracketMax,
          MonthlyRiskContributionPrincipal = optionDto.MonthlyRiskContributionPrincipal,
          MonthlyRiskContributionAdult = optionDto.MonthlyRiskContributionAdult,
          MonthlyRiskContributionChild = optionDto.MonthlyRiskContributionChild,
          MonthlyRiskContributionChild2 = optionDto.MonthlyRiskContributionChild2,
          MonthlyMsaContributionPrincipal = optionDto.MonthlyMsaContributionPrincipal,
          MonthlyMsaContributionAdult = optionDto.MonthlyMsaContributionAdult,
          MonthlyMsaContributionChild = optionDto.MonthlyMsaContributionChild,
          TotalMonthlyContributionsPrincipal = optionDto.TotalMonthlyContributionsPrincipal,
          TotalMonthlyContributionsAdult = optionDto.TotalMonthlyContributionsAdult,
          TotalMonthlyContributionsChild = optionDto.TotalMonthlyContributionsChild,
          TotalMonthlyContributionsChild2 = optionDto.TotalMonthlyContributionsChild2
        };
        
        newMedicalOptions.Add(newMedicalOption);
      }
      
      //6. Perform bulk insert using EFCore.BulkExtensions
      await _context.BulkInsertAsync(newMedicalOptions);
      
      //7. Return the original DTOs (or you could return the created entities)
      return [.. createOptionsPayload];
    }

    /// <summary>
    /// Updates an existing medical option category.
    /// </summary>
    /// <param name="id">The category ID to update.</param>
    /// <param name="updateCategoryPayload">Updated category details.</param>
    /// <returns>Updated MedicalOptionCategoryDto.</returns>
    public async Task<MedicalOptionCategoryDto> UpdateExistingCategoryById(int id,
      UpdateMedicalOptionCategoryDto updateCategoryPayload)
    {
      if (id <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(id), "Category ID must be greater than 0");
      }

      ArgumentNullException.ThrowIfNull(updateCategoryPayload);

      // Find existing category
      var existingCategory = await _context.MedicalOptionCategories
        .FirstOrDefaultAsync(c => c.MedicalOptionCategoryId == id);

      if (existingCategory == null)
      {
        throw new KeyNotFoundException($"Medical option category with ID {id} not found");
      }

      // Check if new name conflicts with another category
      if (!string.Equals(existingCategory.MedicalOptionCategoryName, updateCategoryPayload.MedicalOptionCategoryName, StringComparison.OrdinalIgnoreCase))
      {
        var nameConflict = await _context.MedicalOptionCategories
          .AnyAsync(c => c.MedicalOptionCategoryId != id &&
                         c.MedicalOptionCategoryName == updateCategoryPayload.MedicalOptionCategoryName);

        if (nameConflict)
        {
          throw new InvalidOperationException($"Another category with name '{updateCategoryPayload.MedicalOptionCategoryName}' already exists");
        }
      }

      // Update category properties
      existingCategory.MedicalOptionCategoryName = updateCategoryPayload.MedicalOptionCategoryName;

      _context.MedicalOptionCategories.Update(existingCategory);
      await _context.SaveChangesAsync();

      // Get updated options for the category
      var options = await _context.MedicalOptions
        .Where(o => o.MedicalOptionCategoryId == id)
        .Select(o => o.ToMedicalOptionDto())
        .ToListAsync();

      // Map to DTO and return
      return new MedicalOptionCategoryDto
      {
        MedicalOptionCategoryId = existingCategory.MedicalOptionCategoryId,
        MedicalOptionCategoryName = existingCategory.MedicalOptionCategoryName,
        MedicalOptions = options
      };
    }
  }
}






