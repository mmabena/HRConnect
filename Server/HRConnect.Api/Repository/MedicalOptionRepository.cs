namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Infrastructure;

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
    /// <code>
    /// // Repository usage
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
        .GroupBy(mo => mo.MedicalOptionCategoryId)
        .ToListAsync();
      
      return groupedMedicalOptions;
    }
  }
}