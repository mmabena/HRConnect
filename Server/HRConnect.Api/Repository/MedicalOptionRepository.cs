namespace HRConnect.Api.Repository
{
  using DTOs.MedicalOption;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Infrastructure;
  using HRConnect.Api.Utils;
  using Mappers;
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
        .Where(mo => mo.MedicalOptionCategory != null && mo.MedicalOptionCategoryId != null) // Filter out null categories
        .GroupBy(mo => mo.MedicalOptionCategoryId)
        .ToListAsync();
      
      return groupedMedicalOptions;
    }

    public async Task<MedicalOption?> GetMedicalOptionByIdAsync(int id)
    {
      /*return await _context.MedicalOptions
        .Include(mo => mo.MedicalOptionCategory)
        .FirstOrDefaultAsync(mo => mo.MedicalOptionId == id);*/
      return await _context.MedicalOptions
        .Include(opt => opt.MedicalOptionCategory)
        .Where(opt => opt.MedicalOptionId == id)
        .FirstOrDefaultAsync();
    }

    public async Task<MedicalOption?> UpdateSalaryBracketAsync(int id, 
      UpdateMedicalOptionSalaryBracketRequestDto requestDto)
    {
      var existingOption = await _context.MedicalOptions.FindAsync(id);
      if (existingOption == null) return null;
      
      _context.Entry(existingOption).CurrentValues.SetValues(requestDto);
      await _context.SaveChangesAsync();
      
      return existingOption;
    }

    public async Task<MedicalOption?> GetMedicalOptionCategoryByIdAsync(int id)
    {
      return await _context.MedicalOptions
        .Include(mo => mo.MedicalOptionCategory)
        .FirstOrDefaultAsync(mo => mo.MedicalOptionCategoryId == id);
    }

    public async Task<List<MedicalOption?>> GetAllMedicalOptionsUnderCategoryVariantAsync( 
      string optionName)
    {
      //TODO: Place in a util the below code
      // Map to MedicalOptionDto
      // thinking here is to use a sql query to get all medical options under a category variant
      // using "like" so that i can avoid a lot of complexity
      //optionName = _optionUtils.OptionNameFormatter(optionName); //TODO document this
      //optionName =  optionName.Replace(optionName.Last().ToString(), "").TrimEnd();
      return await _context.MedicalOptions
        .Include(mo => mo.MedicalOptionCategory)
        .Where(mo => mo.MedicalOptionName.Contains(optionName))
        .ToListAsync();
    }

    public async Task<bool> MedicalOptionCategoryExistsAsync(int categoryId)
    {
      var categoryExists = await _context.MedicalOptionCategories
        .FirstOrDefaultAsync(moc => moc.MedicalOptionCategoryId == categoryId);

      if (categoryExists == null) return false;

      return true;
    }

    public async Task<bool> MedicalOptionExistsAsync(int optionId)
    {
      var optionExists = await _context.MedicalOptions
        .AnyAsync(o => o.MedicalOptionId == optionId);

      if (optionExists == null || optionExists == false) return false;

      return true;
    }

    public async Task<List<MedicalOption>> GetAllOptionsUnderCategoryAsync(int categoryId)
    {
      if (await (MedicalOptionCategoryExistsAsync(categoryId)) != true) return null;
        // if true: 
        // return list else null
      var allOptions = await _context.MedicalOptions
        .Where(co => co.MedicalOptionCategoryId == categoryId)
        .ToListAsync();
      return allOptions;
      //Will be transformed to the bulk update dto in the service layer
      
    }

    public async Task<bool> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId)
    {
      // This method assume the category ID and option ID is valid
      var optionExistsInCategory = await _context.MedicalOptions
        .FirstOrDefaultAsync(o => o.MedicalOptionCategoryId == categoryId && o.MedicalOptionId == optionId);
      
      if (optionExistsInCategory == null) return false;

      return true;
    }

    public async Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateByCategoryIdAsync(int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
    {
      // get existin options in category that match with the Payload's Ids
      var optionIdsToUpdate = bulkUpdateDto.Select(dto => dto.MedicalOptionId).ToList();

      var existingOptions = await _context.MedicalOptions
        .Where(o =>
          o.MedicalOptionCategoryId == categoryId && optionIdsToUpdate.Contains(o.MedicalOptionId))
        .ToListAsync();
      
      // check if we have data 
      if (existingOptions.Count <= 0)
      {
        return null;
      }
      
      //Create a dictionary for faster lookups
      var updateDict = bulkUpdateDto.ToDictionary(dto => dto.MedicalOptionId, dto => dto);
      
      // Update entities using the dictionary for 0(1) lookups
      foreach (var entity in existingOptions)
      {
        if (updateDict.TryGetValue(entity.MedicalOptionId, out var updateDto))
        {
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
      
      //perform bulk update using EFCore.BulkExtensions
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