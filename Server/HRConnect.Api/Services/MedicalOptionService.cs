namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Utils.Enums;
  using HRConnect.Api.Utils.Factories;
  using Models.MedicalOptions.Records;
  using Utils.Enums.Mappers;


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
      _medicalOptionRepository = medicalOptionRepository;
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
      var groupedOptions = await _medicalOptionRepository
        .GetGroupedMedicalOptionsAsync();
      return groupedOptions.Select(group => group
        .ToMedicalOptionCategoryDto()).ToList();
    }

    public async Task<MedicalOption?> UpdateSalaryBracketAsync(
      int id, UpdateMedicalOptionSalaryBracketRequestDto requestDto)
    {
      //string filterName = null, selectedVariant = null;
      //Task<List<MedicalOption?>> medicalOptionsVariants;
      var existingOption = await _medicalOptionRepository.GetMedicalOptionByIdAsync(id);
      if (existingOption == null) return null;
      
      //TODO : Now I need to validate the Min and Max amount if they are valid
      
      // Checking Category Name
      if (_restrictedPolicyCategoryUpdates
          .Contains(existingOption.MedicalOptionCategory?.MedicalOptionCategoryName)) 
        throw new InvalidOperationException("Salary bracket cannot be updated for this category");

      //Checking Min and Max from request
      if (requestDto.SalaryBracketMin < 0 ) throw 
        new ArgumentException("Minimum salary must be greater than 0.");
      
      if (requestDto.SalaryBracketMax < requestDto.SalaryBracketMin) throw 
        new ArgumentException("Maximum salary must be greater than minimum salary.");
      
      if (requestDto.SalaryBracketMin > requestDto.SalaryBracketMax) throw 
        new ArgumentException("Minimum salary cannot be greater than or equal to Maximum salary.");
      
      //refactored Version - Single factory handles all cases
      var (categoryName, variantName, filterName) = MedicalOptionVariantFactory
        .GetVariantInfoSafe(existingOption);
          
      // Get trimmed down filtered options variants
      var filteredTrimmedDownVariants = await MedicalOptionUtils
        .GetFilteredOptionVariant(filterName, categoryName, _medicalOptionRepository, 
          existingOption);

      if (filteredTrimmedDownVariants == null)
      {
        throw new InvalidOperationException("No variants found for the given category and " +
                                            "variant.");
      }
      
      var isUpdatePayloadValid = MedicalOptionUtils.ValidateSalaryBracketUpdateRequest(
        filteredTrimmedDownVariants, id, requestDto, existingOption);
      
      if (!isUpdatePayloadValid)
      {
        throw new InvalidOperationException("Salary bracket update is not valid.");
      }
      
      return await _medicalOptionRepository.UpdateSalaryBracketAsync(id, requestDto);
    }
    
    public async Task<MedicalOption?> GetMedicalOptionByIdAsync(int id)
    {
      return await _medicalOptionRepository.GetMedicalOptionByIdAsync(id);
    }

    public async Task<MedicalOption?> GetMedicalOptionCategoryByIdAsync(int categoryId)
    {
      return await _medicalOptionRepository.GetMedicalOptionCategoryByIdAsync(categoryId);
    }

    public async Task<bool> MedicalOptionCategoryExistsAsync(int categoryId)
    {
      return await _medicalOptionRepository.MedicalOptionCategoryExistsAsync(categoryId);
    }

    public async Task<bool> MedicalOptionExistsAsync(int optionId)
    {
      return await _medicalOptionRepository.MedicalOptionExistsAsync(optionId);
    }

    public async Task<List<MedicalOption>> GetAllOptionsUnderCategoryAsync(int categoryId)
    {
      return await _medicalOptionRepository.GetAllOptionsUnderCategoryAsync(categoryId);
    }

    public async Task<bool> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId)
    {
      // This method assume the category ID and option ID is valid
      return await _medicalOptionRepository.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId);
    }

    public async Task<IReadOnlyList<MedicalOption?>> BulkUpdateByCategoryIdAsync(int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
    {
      //Define validations dictionary
      var validationDictionary =
        new Dictionary<string, (bool isValid, string? Message, Func<string, Exception> Throw)>();
      int updateCounter = 0;
      // Check if the update is done outside the update period (Nov - Dev) || Approach used is enum(Named Period) + Extension Method
      if (!(DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(DateTime.Now)))
      {
        throw new InvalidOperationException(
          "Bulk update operation cannot be executed outside the update period");
      }


      
      
      // Validations (Strict)
      // Check if category is valid
      if (await _medicalOptionRepository.MedicalOptionCategoryExistsAsync(categoryId) is false)
        throw
          new ArgumentException("Bulk update operation cannot be executed on this category");
      
      //get db copy of the options under the category
      var dbData = await _medicalOptionRepository
        .GetAllOptionsUnderCategoryAsync(categoryId);
      
      // First check if the count of entity is the same as that in the db
      if (dbData.Count != bulkUpdateDto.Count)
        throw new InvalidOperationException(
          "One or more medical options not found in the specified category");
      
      // variables to store the Previous and Current Variant
      string previousVariantName = null;
      string currentVariantName = null;
      
      // Check if all id's within the payload are valid
      foreach (var entity in bulkUpdateDto)
      {
        // Check the ID first 
        if (await _medicalOptionRepository.MedicalOptionExistsAsync(entity.MedicalOptionId) is false)
          throw new InvalidOperationException("One or more medical options are invalid");
        
        // Then validate if it belongs in the category
        if (await _medicalOptionRepository.MedicalOptionExistsWithinCategoryAsync(categoryId, entity.MedicalOptionId) is false)
          throw new InvalidOperationException("One or more medical options are invalid within the specified category");
        // TODO : Might consider adding the option to use as a way to confirm if what is updated really exists and that no one is trying to update a non existing option with a valid id
        // TODO: Add Dict as a storage for validations
        
        //--- Salary Bracket Validation (Alliance and Double are restricted) ---
        
        // Check if cat if Double or Alliance has a salary bracket update
        // Check via ID since we do not have names:
        var getOptionTable = await GetMedicalOptionCategoryByIdAsync(categoryId);
            
        if ((bool)(getOptionTable.MedicalOptionCategory?.MedicalOptionCategoryName.Contains(
              "Double")) ||
            (bool)getOptionTable.MedicalOptionCategory?.MedicalOptionCategoryName.Contains(
              "Alliance"))
        {
          throw new InvalidOperationException("Salary bracket cannot be updated for this category");
        }
        else
        {
          // If not restricted, proceed with salary bracket check
          // Get variant info like Name in this case (Will be used in the Salary Validations)
          var (categoryName, variantName, filterName ) = MedicalOptionVariantFactory
            .GetVariantInfoSafe(getOptionTable);
          // use the filterName as the tracking Name
          currentVariantName = filterName;
          
          //check if it is the first iteration or not 
          if (previousVariantName != null)
          {
           previousVariantName = currentVariantName; 
          }
          else
          {
            //check if the current variant is the same as the same as the previous
            if (previousVariantName != currentVariantName)
            {
              //update variant and reset counter
            }
          }
          
          // Mark the start of contribution validations :
          var salaryBracketValidatorRecord = new SalaryBracketValidatorRecord(
            entity.MedicalOptionId,
            getOptionTable.MedicalOptionName,
            entity.SalaryBracketMin,
            entity.SalaryBracketMax);
        }

      }

      return null;
    }
  }  
}