namespace HRConnect.Api.Repository
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Infrastructure;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Mappers;
  using Microsoft.Data.SqlClient;
  using EFCore.BulkExtensions;


  public class MedicalOptionRepository: IMedicalOptionRepository
  {
    private readonly ApplicationDBContext _context;

    
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
    /// 
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

        public async Task<MedicalOption> GetMedicalOptionByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Medical option ID must be greater than 0", nameof(id));
            }

            var medicalOption = await _context.MedicalOptions
                .Include(opt => opt.MedicalOptionCategory)
                .FirstOrDefaultAsync(opt => opt.MedicalOptionId == id);

            if (medicalOption == null)
            {
                throw new KeyNotFoundException($"MedicalOption with ID {id} was not found");
            }

            return medicalOption;
        }
        
        public async Task<MedicalOption?> GetMedicalOptionCategoryByIdAsync(int id)
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

          return medicalOptions;
        }

        /// <summary>
        /// Retrieves medical options by category ID.
        /// </summary>
        /// <param name="id">The category ID</param>
        /// <returns>List of medical options in the category</returns>
        /// <exception cref="KeyNotFoundException">Thrown when category is not found</exception>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>


        /// <summary>
        /// Retrieves medical options by their IDs.
        /// </summary>
        /// <param name="ids">List of medical option IDs</param>
        /// <returns>List of medical options</returns>
        /// <exception cref="ArgumentException">Thrown when IDs list is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no options are found</exception>
        public async Task<List<MedicalOption>> GetMedicalOptionsByIdsAsync(List<int> ids)
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

            return medicalOptions;
        }

        /// <summary>
        /// Retrieves a medical option category by its ID.
        /// </summary>
        /// <param name="id">The category ID</param>
        /// <returns>The medical option category</returns>
        /// <exception cref="KeyNotFoundException">Thrown when category is not found</exception>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
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
                throw new KeyNotFoundException($"MedicalOptionCategory with ID {id} was not found");
            }

            return category;
        }

        /// <summary>
        /// Retrieves all medical options under a category variant.
        /// </summary>
        /// <param name="optionName">The option name to search for</param>
        /// <returns>List of medical options</returns>
        /// <exception cref="ArgumentException">Thrown when option name is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no options are found</exception>
        public async Task<List<MedicalOption>> GetAllMedicalOptionsUnderCategoryVariantAsync(string optionName)
        {
            if (string.IsNullOrWhiteSpace(optionName))
            {
                throw new ArgumentException("Option name cannot be null or empty", nameof(optionName));
            }

            var medicalOptions = await _context.MedicalOptions
                .Include(mo => mo.MedicalOptionCategory)
                .Where(mo => mo.MedicalOptionName.Contains(optionName))
                .ToListAsync();

            if (medicalOptions.Count == 0)
            {
                throw new KeyNotFoundException($"No medical options found containing '{optionName}'");
            }

            return medicalOptions;
        }

        /// <summary>
        /// Checks if a medical option category exists.
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <returns>True if category exists</returns>
        /// <exception cref="ArgumentException">Thrown when category ID is invalid</exception>
        public async Task<bool> MedicalOptionCategoryExistsAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(categoryId));
            }

            return await _context.MedicalOptionCategories
                .AnyAsync(moc => moc.MedicalOptionCategoryId == categoryId);
        }

        /// <summary>
        /// Checks if a medical option exists.
        /// </summary>
        /// <param name="optionId">The option ID</param>
        /// <returns>True if option exists</returns>
        /// <exception cref="ArgumentException">Thrown when option ID is invalid</exception>
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
        /// Gets all options under a specific category.
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <returns>List of medical options</returns>
        /// <exception cref="KeyNotFoundException">Thrown when category doesn't exist</exception>
        /// <exception cref="ArgumentException">Thrown when category ID is invalid</exception>
        public async Task<List<MedicalOption>> GetAllOptionsUnderCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(categoryId));
            }

            if (!await MedicalOptionCategoryExistsAsync(categoryId))
            {
                throw new KeyNotFoundException($"MedicalOptionCategory with ID {categoryId} was not found");
            }

            var allOptions = await _context.MedicalOptions
                .Where(co => co.MedicalOptionCategoryId == categoryId)
                .ToListAsync();

            return allOptions;
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
            if (categoryId <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(categoryId));
            }

            if (optionId <= 0)
            {
                throw new ArgumentException("Option ID must be greater than 0", nameof(optionId));
            }

            return await _context.MedicalOptions
                .AnyAsync(o => o.MedicalOptionCategoryId == categoryId && o.MedicalOptionId == optionId);
        }

        /// <summary>
        /// Bulk updates medical options by category ID.
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <param name="bulkUpdateDto">The bulk update data</param>
        /// <returns>List of updated medical options</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no matching options are found</exception>
        public async Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateByCategoryIdAsync(int categoryId,
            IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
        {
            if (categoryId <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(categoryId));
            }

            if (bulkUpdateDto == null || bulkUpdateDto.Count == 0)
            {
                throw new ArgumentException("Bulk update DTO cannot be null or empty", nameof(bulkUpdateDto));
            }

            // Get existing options in category that match with the Payload's IDs
            var optionIdsToUpdate = bulkUpdateDto.Select(dto => dto.MedicalOptionId).ToList();

            var existingOptions = await _context.MedicalOptions
                .Where(o => o.MedicalOptionCategoryId == categoryId && optionIdsToUpdate.Contains(o.MedicalOptionId))
                .ToListAsync();

            if (existingOptions.Count == 0)
            {
                throw new KeyNotFoundException($"No medical options found for category ID {categoryId} with the provided option IDs");
            }

            // Create a dictionary for faster lookups
            var updateDict = bulkUpdateDto.ToDictionary(dto => dto.MedicalOptionId, dto => dto);

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

                    entity.SalaryBracketMin = updateDto.SalaryBracketMin;
                    entity.SalaryBracketMax = updateDto.SalaryBracketMax;
                    entity.MonthlyMsaContributionAdult = updateDto.MonthlyMsaContributionAdult;
                    entity.MonthlyMsaContributionChild = updateDto.MonthlyMsaContributionChild;
                    entity.MonthlyMsaContributionPrincipal = updateDto.MonthlyMsaContributionPrincipal;
                    entity.MonthlyRiskContributionAdult = updateDto.MonthlyRiskContributionAdult;
                    entity.MonthlyRiskContributionChild = updateDto.MonthlyRiskContributionChild;
                    entity.MonthlyRiskContributionChild2 = updateDto.MonthlyRiskContributionChild2;
                    entity.MonthlyRiskContributionPrincipal = updateDto.MonthlyRiskContributionPrincipal;
                    entity.TotalMonthlyContributionsAdult = updateDto.TotalMonthlyContributionsAdult;
                    entity.TotalMonthlyContributionsChild = updateDto.TotalMonthlyContributionsChild;
                    entity.TotalMonthlyContributionsChild2 = updateDto.TotalMonthlyContributionsChild2;
                    entity.TotalMonthlyContributionsPrincipal = updateDto.TotalMonthlyContributionsPrincipal;
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
            var responseDtos = existingOptions.Select(option => new MedicalOptionDto
            {
                MedicalOptionId = option.MedicalOptionId,
                MedicalOptionName = option.MedicalOptionName,
                MedicalOptionCategoryId = option.MedicalOptionCategoryId,
                SalaryBracketMin = option.SalaryBracketMin,
                SalaryBracketMax = option.SalaryBracketMax,
                MonthlyRiskContributionPrincipal = option.MonthlyRiskContributionPrincipal,
                MonthlyRiskContributionAdult = option.MonthlyRiskContributionAdult,
                MonthlyRiskContributionChild = option.MonthlyRiskContributionChild,
                MonthlyRiskContributionChild2 = option.MonthlyRiskContributionChild2,
                MonthlyMsaContributionPrincipal = option.MonthlyMsaContributionPrincipal,
                MonthlyMsaContributionAdult = option.MonthlyMsaContributionAdult,
                MonthlyMsaContributionChild = option.MonthlyMsaContributionChild,
                TotalMonthlyContributionsPrincipal = option.TotalMonthlyContributionsPrincipal,
                TotalMonthlyContributionsAdult = option.TotalMonthlyContributionsAdult,
                TotalMonthlyContributionsChild = option.TotalMonthlyContributionsChild,
                TotalMonthlyContributionsChild2 = option.TotalMonthlyContributionsChild2
            }).ToList();

            return responseDtos.AsReadOnly();
        }
    }
}